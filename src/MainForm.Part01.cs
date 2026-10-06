
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

public sealed partial class MainForm : Form
{
    const ulong SteamIdBase = 76561197960265728UL;
    const int AppId = 204360;
    const int CharacterRecordCount = 42;

    static readonly string[] CharacterNames = new string[] {
        "Green Knight","Red Knight","Blue Knight","Orange Knight","Gray Knight",
        "Barbarian","Thief","Fencer","Beekeeper","Industrialist","Alien Hominid",
        "The King","The Brute","Snakey","Saracen","Royal Guard","Stove Face",
        "Peasant","Bear","Necromancer","Conehead","Civilian","Open-Faced Gray Knight",
        "Fire Demon","Skeleton","Iceskimo","Ninja","Cult Minion","Pink Knight",
        "Blacksmith","Hatty Hattington","Paint Junior"
    };

    static readonly string[] WeaponNames = new string[] {
        "Skinny Sword","Skinny Sword","Skinny Sword","Thin Sword","Thick Sword",
        "Pumpkin Peeler","Gladiator Sword","Butcher Knife","Half Sword","Carrot",
        "Thief Sword","Gold Sword","Dual Prong Sword","Zigzag","Playdo Pasta Maker",
        "Falchion","Pointy Sword","Chewed Up Sword","Fencer's Foil","Barbarian Axe",
        "Pitchfork","Curved Sword","Key Sword","Apple Peeler","Rubber Handle Sword",
        "Mace","Club","Ugly Mace","Refined Mace","Fish","Wrapped Sword",
        "Skeletor Mace","Clunky Mace","Snakey Mace","Rat Beating Bat",
        "Black Morning Star","King's Mace","Meat Tenderizer","Leaf","Sheathed Sword",
        "Practice Foil","Twig","Leafy Twig","Light Saber","Staff","Wooden Spoon",
        "Bone Leg","Alien Gun","Fishing Spear","Lance","Sai","Unicorn Horn",
        "Ribeye","Kielbasa","Lobster","Umbrella","Broad Ax","Evil Sword",
        "Ice Sword","Candlestick","Panic Mallet","Fishing Rod","Wrench",
        "NG Lollipop","Gold Skull Mace","NG Gold Sword","Chainsaw","Broad Spear",
        "Glowstick","Chicken Stick","Demon Sword","Broccoli Sword","Man Catcher",
        "Wooden Mace","Ninja Claw","Buffalo Mace","Electric Eel","Scissors",
        "Dinner Fork","Cattle Prod","Lightning Bolt","2x4","Wooden Sword",
        "Cardboard Tube","Emerald Sword","Hammer","Pencil"
    };

    static readonly string[] PetNames = new string[] {
        "None","Cardinal","Owlet","Rammy","Frogglet","Monkeyface","BiPolar Bear",
        "Bitey Bat","Yeti","Troll","Snailburt","Giraffey","Zebra","Meowburt",
        "Pazzo","Burly Bear","Hawkster","Snoot","Piggy","Spiny","Scratchpaw",
        "Seahorse","Chicken","Install Ball","Mr. Buddy","Sherbert","Pelter",
        "Dragonhead","Beholder","Golden Whale"
    };

    static readonly string[] ProgressNames = new string[] {
        "Start / No checkpoint","Castle Keep","Barbarian Boss","Thieves' Forest","Catfish",
        "Pipistrello's Cave","Parade","Cyclops' Fortress","Lava World",
        "Industrial Castle","Pirate Ship","Desert Chase","Sand Castle Roof",
        "Corn Boss","Medusa's Lair","Full Moon","Ice Castle","Final Battle"
    };

    SaveSession session;

    readonly ListView characterList = new ListView();
    readonly TextBox searchBox = new TextBox();
    readonly Label savePathLabel = new Label();
    readonly Label saveStateLabel = new Label();
    readonly Label selectedName = new Label();
    readonly Label selectedState = new Label();
    readonly Label selectedWeapon = new Label();
    readonly Label selectedPet = new Label();
    readonly Label selectedProgress = new Label();
    readonly Dictionary<string, Label> summary = new Dictionary<string, Label>();
    readonly Dictionary<string, TextBox> edits = new Dictionary<string, TextBox>();
    readonly Label statusText = new Label();
    readonly Label pendingLabel = new Label();
    readonly Button applyPendingButton = new Button();
    readonly Button discardPendingButton = new Button();
    byte[] committedPlain;
    bool pendingDangerous = false;
    bool suppressControlEvents = false;

    static readonly int[] MaxAllCharacterFieldOffsets = new int[] { 0x08, 0x09, 0x0A, 0x0B, 0x0F, 0x10, 0x11 };
    byte[,] maxAllCharacterSnapshot;
    bool maxAllCharacterSnapshotActive = false;

    readonly Button loadButton = new Button();
    readonly Button refreshButton = new Button();
    readonly Button guideButton = new Button();
    readonly Button themeButton = new Button();
    readonly Button overviewApplyButton = new Button();
    readonly Button statsApplyButton = new Button();
    readonly Button toggleButton = new Button();
    readonly Button characterBulkButton = new Button();
    readonly Button maxAllCharactersButton = new Button();
    readonly Label characterBulkStatus = new Label();
    readonly Button clearButton = new Button();
    readonly Label balanceStateLabel = new Label();
    readonly Label balancePointsLabel = new Label();
    readonly Button balanceCharacterButton = new Button();
    readonly ComboBox weaponCombo = new ComboBox();
    readonly ComboBox petCombo = new ComboBox();
    readonly Label weaponCurrentLabel = new Label();
    readonly Label petCurrentLabel = new Label();
    readonly Button equipmentApplyButton = new Button();
    readonly ComboBox normalProgressCombo = new ComboBox();
    readonly ComboBox insaneProgressCombo = new ComboBox();
    readonly CheckBox insaneUnlockedCheck = new CheckBox();
    readonly ComboBox skullCombo = new ComboBox();
    readonly CheckBox[] collectChecks = new CheckBox[8];
    readonly Button progressApplyButton = new Button();
    readonly Button insaneProgressApplyButton = new Button();
    readonly Label currentLevelOverrideLabel = new Label();
    readonly Label valueModeStateLabel = new Label();
    readonly Label valueModeRangeLabel = new Label();
    readonly Label normalProgressPreview = new Label();
    readonly Label insaneProgressPreview = new Label();
    readonly Label progressPrereqSummary = new Label();
    readonly ListView backupList = new ListView();
    readonly Label backupInfo = new Label();
    readonly ImageList listRowSpacer = new ImageList();
    readonly ThemedTabControl tabs = new ThemedTabControl();
    readonly Timer loadAttentionTimer = new Timer();
    bool loadAttentionPulse = false;
    bool darkMode = true;
    Color Header, Surface, SurfaceRaised, Canvas, Muted, TextPrimary, TextSecondary, Border, InputSurface, InvalidInput, DisabledText;
    Color Accent, Good, Warn, Danger, AccentTextColor, GoodTextColor, WarnTextColor, DangerTextColor, NeutralButton;
    Color HeaderButton, HeaderBorder, HeaderText, HeaderMuted, HeaderGood, HeaderWarn, TabInactive;

    public MainForm()
    {
        Text = "Crasher Editor V1.3";
        try { Icon exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); if (exeIcon != null) this.Icon = exeIcon; } catch { }
        LoadThemePreference();
        SetThemeColors();
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1380, 860);
        MinimumSize = new Size(1220, 760);
        BackColor = Canvas;
        Font = new Font("Segoe UI", 9.0f);
        DoubleBuffered = true;
        BuildUi();
        UpdateCharacterValueModeUi();
        ApplyTheme();
        PopulateList();
        listRowSpacer.ColorDepth = ColorDepth.Depth32Bit;
        listRowSpacer.ImageSize = new Size(1, 22);
        listRowSpacer.Images.Add(new Bitmap(1, 22));
        loadAttentionTimer.Interval = 520;
        loadAttentionTimer.Tick += delegate { if (session != null) { loadAttentionPulse = false; loadAttentionTimer.Stop(); UpdateLoadAttentionVisual(); return; } loadAttentionPulse = !loadAttentionPulse; UpdateLoadAttentionVisual(); };
        FormClosing += MainForm_FormClosing;
        SetStatus("Ready. Click Load Save to begin editing.");
        UpdatePendingUi();
        UpdateLoadAvailabilityUi();
    }

    void BuildUi()
    {
        SuspendLayout();
        TableLayoutPanel root = new TableLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.Margin = Padding.Empty;
        root.Padding = Padding.Empty;
        root.BackColor = Canvas;
        root.Tag = "Canvas";
        root.ColumnCount = 1;
        root.RowCount = 3;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96f));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
        Controls.Add(root);
        Control header = BuildHeader(); header.Dock = DockStyle.Fill; header.Margin = Padding.Empty; root.Controls.Add(header, 0, 0);
        TableLayoutPanel workspace = new TableLayoutPanel();
        workspace.Dock = DockStyle.Fill; workspace.Margin = Padding.Empty; workspace.Padding = new Padding(16,16,16,8); workspace.BackColor = Canvas; workspace.Tag = "Canvas";
        workspace.ColumnCount = 2; workspace.RowCount = 1; workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360f)); workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f)); workspace.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
        root.Controls.Add(workspace,0,1);
        Control side = BuildCharacterPanel(); side.Dock = DockStyle.Fill; side.Margin = new Padding(0,0,14,0); workspace.Controls.Add(side,0,0);
        Control tabHost = BuildTabs(); tabHost.Dock = DockStyle.Fill; tabHost.Margin = Padding.Empty; workspace.Controls.Add(tabHost,1,0);
        Control status = BuildStatusBar(); status.Dock = DockStyle.Fill; status.Margin = Padding.Empty; root.Controls.Add(status,0,2);
        ResumeLayout(true);
    }
}
