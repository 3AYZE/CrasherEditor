
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    Control BuildHeader()
    {
        Panel header = new Panel(); header.BackColor = Header; header.Tag = "Header";
        TableLayoutPanel layout = new TableLayoutPanel(); layout.Dock = DockStyle.Fill; layout.Padding = new Padding(18,10,18,10); layout.Margin = Padding.Empty; layout.ColumnCount = 3; layout.RowCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,280f)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,360f)); layout.RowStyles.Add(new RowStyle(SizeType.Percent,100f)); header.Controls.Add(layout);
        Panel brand = new Panel(); brand.Dock = DockStyle.Fill; brand.Margin = Padding.Empty; layout.Controls.Add(brand,0,0);
        Label title = new Label(); title.Text = "Crasher Editor V1.3"; title.ForeColor = HeaderText; title.Tag = "HeaderPrimary"; title.Font = new Font("Segoe UI Semibold",19.0f); title.AutoSize = true; title.Location = new Point(2,3); brand.Controls.Add(title);
        Label creator = new Label(); creator.Text = "Created by ThIHuTt"; creator.ForeColor = HeaderMuted; creator.Tag = "HeaderMuted"; creator.AutoSize = true; creator.Location = new Point(4,42); brand.Controls.Add(creator);
        Panel savePanel = new Panel(); savePanel.Size = new Size(500,76); savePanel.Dock = DockStyle.Fill; savePanel.Margin = new Padding(10,0,12,0); layout.Controls.Add(savePanel,1,0);
        saveStateLabel.Text = "Not loaded"; saveStateLabel.ForeColor = HeaderText; saveStateLabel.Tag = "HeaderPrimary"; saveStateLabel.Font = new Font("Segoe UI Semibold",9.5f); saveStateLabel.AutoSize = false; saveStateLabel.Location = new Point(0,8); saveStateLabel.Size = new Size(500,22); saveStateLabel.Anchor = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right; savePanel.Controls.Add(saveStateLabel);
        savePathLabel.Text = "No save selected"; savePathLabel.ForeColor = HeaderMuted; savePathLabel.Tag = "HeaderMuted"; savePathLabel.AutoEllipsis = true; savePathLabel.AutoSize = false; savePathLabel.Location = new Point(0,38); savePathLabel.Size = new Size(500,24); savePathLabel.Anchor = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right; savePanel.Controls.Add(savePathLabel);
        TableLayoutPanel actions = new TableLayoutPanel(); actions.Dock = DockStyle.Fill; actions.Margin = Padding.Empty; actions.Padding = new Padding(0,14,0,12); actions.ColumnCount=4; actions.RowCount=1; actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,27f)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,21f)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30f)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,22f)); actions.RowStyles.Add(new RowStyle(SizeType.Percent,100f)); layout.Controls.Add(actions,2,0);
        ConfigureHeaderAction(loadButton,"Load Save",delegate{TryUiAction(LoadSave);}); ConfigureHeaderAction(refreshButton,"Refresh",delegate{TryUiAction(RefreshSave);}); ConfigureHeaderAction(guideButton,"Guide",delegate{ShowUsageGuide();}); ConfigureHeaderAction(themeButton,darkMode?"Light Mode":"Dark Mode",delegate{ToggleTheme();});
        loadButton.Dock=refreshButton.Dock=guideButton.Dock=themeButton.Dock=DockStyle.Fill; loadButton.Margin=new Padding(0,0,8,0); refreshButton.Margin=new Padding(0,0,8,0); guideButton.Margin=new Padding(0,0,8,0); themeButton.Margin=Padding.Empty; actions.Controls.Add(loadButton,0,0); actions.Controls.Add(refreshButton,1,0); actions.Controls.Add(guideButton,2,0); actions.Controls.Add(themeButton,3,0);
        return header;
    }

    void ConfigureHeaderAction(Button button,string text,EventHandler handler){button.Text=text;button.FlatStyle=FlatStyle.Flat;button.FlatAppearance.BorderColor=HeaderBorder;button.FlatAppearance.BorderSize=1;button.BackColor=HeaderButton;button.ForeColor=HeaderText;button.UseVisualStyleBackColor=false;button.Tag="HeaderButton";button.Click+=handler;}

    Control BuildCharacterPanel()
    {
        Panel side=NewCard(0,0,340,500); side.MinimumSize=new Size(320,420);
        TableLayoutPanel layout=new TableLayoutPanel(); layout.Dock=DockStyle.Fill; layout.Margin=Padding.Empty; layout.Padding=new Padding(14,12,14,12); layout.ColumnCount=1; layout.RowCount=8; layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32f)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,22f)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,34f)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,12f)); layout.RowStyles.Add(new RowStyle(SizeType.Percent,100f)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,30f)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,42f)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute,42f)); side.Controls.Add(layout);
        Label title=NewSectionTitle("Characters",0,0); title.Dock=DockStyle.Fill; title.TextAlign=ContentAlignment.MiddleLeft; layout.Controls.Add(title,0,0);
        Label searchLabel=NewMutedLabel("Search characters",0,0); searchLabel.Dock=DockStyle.Fill; searchLabel.TextAlign=ContentAlignment.BottomLeft; layout.Controls.Add(searchLabel,0,1);
        searchBox.Dock=DockStyle.Fill; searchBox.Margin=new Padding(0,2,0,3); searchBox.TextChanged+=delegate{int caret=searchBox.SelectionStart;PopulateList();searchBox.Focus();searchBox.SelectionStart=Math.Min(caret,searchBox.TextLength);searchBox.SelectionLength=0;}; layout.Controls.Add(searchBox,0,2);
        characterList.Dock=DockStyle.Fill; characterList.Margin=new Padding(0,0,0,10); characterList.View=View.Details; characterList.FullRowSelect=true; characterList.GridLines=false; characterList.HideSelection=false; characterList.MultiSelect=false; characterList.SmallImageList=listRowSpacer; characterList.Font=new Font("Segoe UI",9.0f); characterList.Columns.Add("Slot",42); characterList.Columns.Add("Name",168); characterList.Columns.Add("Status",94); characterList.SelectedIndexChanged+=delegate{UpdateDetails();}; characterList.Resize+=delegate{UpdateCharacterListColumns();}; layout.Controls.Add(characterList,0,4); UpdateCharacterListColumns();
        characterBulkStatus.Text="Load a save to view character status"; characterBulkStatus.ForeColor=Muted; characterBulkStatus.Tag="MutedText"; characterBulkStatus.AutoSize=false; characterBulkStatus.AutoEllipsis=true; characterBulkStatus.Dock=DockStyle.Fill; characterBulkStatus.TextAlign=ContentAlignment.MiddleLeft; characterBulkStatus.Margin=new Padding(0,2,0,4); layout.Controls.Add(characterBulkStatus,0,5);
        ConfigureButton(characterBulkButton,"Unlock All Characters",0,0,100,36); StyleButton(characterBulkButton,Accent); characterBulkButton.Dock=DockStyle.Fill; characterBulkButton.Margin=new Padding(0,3,0,3); characterBulkButton.Enabled=false; characterBulkButton.Click+=delegate{TryUiAction(ToggleAllCharactersSmart);}; layout.Controls.Add(characterBulkButton,0,6);
        ConfigureButton(maxAllCharactersButton,"MAX All Characters",0,0,100,36); StyleButton(maxAllCharactersButton,Good); maxAllCharactersButton.Dock=DockStyle.Fill; maxAllCharactersButton.Margin=new Padding(0,3,0,0); maxAllCharactersButton.Enabled=false; maxAllCharactersButton.Click+=delegate{TryUiAction(StageMaxAllCharacters);}; layout.Controls.Add(maxAllCharactersButton,0,7);
        return side;
    }

    void UpdateCharacterListColumns(){if(characterList.Columns.Count<3)return;int slotWidth=40,statusWidth=82,available=characterList.ClientSize.Width-SystemInformation.VerticalScrollBarWidth-20;if(available<=0)return;int nameWidth=Math.Max(110,available-slotWidth-statusWidth);characterList.Columns[0].Width=slotWidth;characterList.Columns[1].Width=nameWidth;characterList.Columns[2].Width=statusWidth;}

    Control BuildTabs(){tabs.Dock=DockStyle.Fill;tabs.Font=new Font("Segoe UI Semibold",9.5f);tabs.Padding=new Point(16,6);tabs.Appearance=TabAppearance.FlatButtons;tabs.SizeMode=TabSizeMode.Fixed;tabs.ItemSize=new Size(124,34);tabs.BackColor=Canvas;tabs.Tag="Canvas";BuildOverviewTab();BuildStatsTab();BuildEquipmentTab();BuildProgressTab();BuildUnlockTab();BuildBackupsTab();return tabs;}
    TabPage CreateTab(string text){TabPage page=new TabPage(text);page.BackColor=Canvas;page.ForeColor=TextPrimary;page.Tag="Canvas";page.Padding=Padding.Empty;page.AutoScroll=true;page.AutoScrollMinSize=new Size(850,0);tabs.TabPages.Add(page);return page;}
    TableLayoutPanel CreatePageLayout(TabPage page){TableLayoutPanel layout=new TableLayoutPanel();layout.Dock=DockStyle.Top;layout.AutoSize=true;layout.AutoSizeMode=AutoSizeMode.GrowAndShrink;layout.MinimumSize=new Size(850,0);layout.Margin=Padding.Empty;layout.Padding=new Padding(14,16,14,16);layout.ColumnCount=1;layout.RowCount=0;layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));page.Controls.Add(layout);return layout;}
    void AddFullRow(TableLayoutPanel layout,Control control,int height){int row=layout.RowCount;layout.RowCount++;layout.RowStyles.Add(new RowStyle(SizeType.Absolute,height+14));control.Dock=DockStyle.Fill;control.Margin=new Padding(0,0,0,14);layout.Controls.Add(control,0,row);}
}
