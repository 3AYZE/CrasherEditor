
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
    void SetThemeColors()
    {
        if(darkMode){Header=Color.FromArgb(21,28,37);Canvas=Color.FromArgb(17,20,24);Surface=Color.FromArgb(27,32,38);SurfaceRaised=Color.FromArgb(32,38,45);InputSurface=Color.FromArgb(32,38,45);Border=Color.FromArgb(52,61,71);TextPrimary=Color.FromArgb(242,244,247);TextSecondary=Color.FromArgb(195,203,213);Muted=Color.FromArgb(174,184,196);DisabledText=Color.FromArgb(119,129,141);InvalidInput=Color.FromArgb(74,37,41);Accent=Color.FromArgb(65,105,145);Good=Color.FromArgb(63,117,78);Warn=Color.FromArgb(139,93,43);Danger=Color.FromArgb(169,78,78);AccentTextColor=Color.FromArgb(111,168,220);GoodTextColor=Color.FromArgb(105,193,125);WarnTextColor=Color.FromArgb(225,169,91);DangerTextColor=Color.FromArgb(217,108,108);NeutralButton=Color.FromArgb(89,102,117);HeaderButton=Color.FromArgb(38,51,65);HeaderBorder=Color.FromArgb(66,84,102);HeaderText=Color.FromArgb(245,247,250);HeaderMuted=Color.FromArgb(184,196,208);HeaderGood=Color.FromArgb(185,241,197);HeaderWarn=Color.FromArgb(244,205,112);TabInactive=Color.FromArgb(23,27,32);}
        else{Header=Color.FromArgb(35,51,70);Canvas=Color.FromArgb(244,247,250);Surface=Color.White;SurfaceRaised=Color.FromArgb(248,250,252);InputSurface=Color.White;Border=Color.FromArgb(207,216,225);TextPrimary=Color.FromArgb(36,42,49);TextSecondary=Color.FromArgb(65,73,81);Muted=Color.FromArgb(99,108,118);DisabledText=Color.FromArgb(130,130,130);InvalidInput=Color.FromArgb(255,235,235);Accent=Color.FromArgb(65,105,145);Good=Color.FromArgb(63,117,78);Warn=Color.FromArgb(139,93,43);Danger=Color.FromArgb(169,78,78);AccentTextColor=Accent;GoodTextColor=Good;WarnTextColor=Warn;DangerTextColor=Danger;NeutralButton=Color.FromArgb(110,122,135);HeaderButton=Color.FromArgb(48,68,89);HeaderBorder=Color.FromArgb(88,110,132);HeaderText=Color.FromArgb(225,232,239);HeaderMuted=Color.FromArgb(188,201,214);HeaderGood=Color.FromArgb(190,235,198);HeaderWarn=Color.FromArgb(244,205,112);TabInactive=Color.FromArgb(235,240,245);}
    }
    string SemanticRoleForColor(Color color){int a=color.ToArgb();if(a==Accent.ToArgb())return"Accent";if(a==Good.ToArgb())return"Good";if(a==Warn.ToArgb())return"Warn";if(a==Danger.ToArgb())return"Danger";return"Neutral";}
    Color SemanticColor(string role){if(String.Equals(role,"Accent",StringComparison.OrdinalIgnoreCase))return Accent;if(String.Equals(role,"Good",StringComparison.OrdinalIgnoreCase))return Good;if(String.Equals(role,"Warn",StringComparison.OrdinalIgnoreCase))return Warn;if(String.Equals(role,"Danger",StringComparison.OrdinalIgnoreCase))return Danger;return NeutralButton;}
    Color SemanticTextColor(string role){if(String.Equals(role,"Accent",StringComparison.OrdinalIgnoreCase))return AccentTextColor;if(String.Equals(role,"Good",StringComparison.OrdinalIgnoreCase))return GoodTextColor;if(String.Equals(role,"Warn",StringComparison.OrdinalIgnoreCase))return WarnTextColor;if(String.Equals(role,"Danger",StringComparison.OrdinalIgnoreCase))return DangerTextColor;return Muted;}
    void UpdateCharacterValueModeUi(){valueModeStateLabel.Text="Game-supported values only";valueModeStateLabel.ForeColor=GoodTextColor;valueModeRangeLabel.Text="Level 1–99 • STR / DEF / MAG / AGI 1–25 • Balance available";}
    void UpdateLoadAttentionVisual(){if(session==null){loadButton.BackColor=loadAttentionPulse?Warn:Accent;loadButton.ForeColor=Color.White;loadButton.FlatAppearance.BorderColor=loadAttentionPulse?Color.FromArgb(244,205,112):HeaderBorder;}else{loadButton.BackColor=HeaderButton;loadButton.ForeColor=HeaderText;loadButton.FlatAppearance.BorderColor=HeaderBorder;}}
    void UpdateLoadAvailabilityUi(){bool loaded=session!=null;searchBox.Enabled=loaded;characterList.Enabled=loaded;tabs.Enabled=loaded;refreshButton.Enabled=loaded;if(!loaded){saveStateLabel.Text="Not loaded";savePathLabel.Text="Load your Castle Crashers save to unlock the editor";if(!loadAttentionTimer.Enabled)loadAttentionTimer.Start();}else{loadAttentionPulse=false;loadAttentionTimer.Stop();}UpdateLoadAttentionVisual();}
    void ToggleTheme(){darkMode=!darkMode;SetThemeColors();ApplyTheme();SaveThemePreference();themeButton.Text=darkMode?"Light Mode":"Dark Mode";UpdateCharacterValueModeUi();UpdatePendingUi();UpdateCharacterBalanceUi();PopulateList();SetStatus(darkMode?"Dark mode enabled.":"Light mode enabled.");}
    void ApplyTheme(){BackColor=Canvas;ForeColor=TextPrimary;ApplyThemeToControl(this);tabs.SetTheme(Canvas,Surface,TabInactive,Border,TextPrimary,Muted,AccentTextColor);RefreshInputValidationColors();UpdateLoadAttentionVisual();tabs.Invalidate();characterList.Invalidate();backupList.Invalidate();}
}
