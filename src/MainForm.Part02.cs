
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
        Label title = new Label(); title.Text = "Crasher Editor V1.4"; title.ForeColor = HeaderText; title.Tag = "HeaderPrimary"; title.Font = new Font("Segoe UI Semibold",19.0f); title.AutoSize = true; title.Location = new Point(2,3); brand.Controls.Add(title);
        Label creator = new Label(); creator.Text = "Created by 3AZYE"; creator.ForeColor = HeaderMuted; creator.Tag = "HeaderMuted"; creator.AutoSize = true; creator.Location = new Point(4,42); brand.Controls.Add(creator);
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
        Panel side=new Panel();side.Tag="Sidebar";side.MinimumSize=new Size(260,400);
        TableLayoutPanel layout=new TableLayoutPanel();
        layout.Dock=DockStyle.Fill;layout.Margin=Padding.Empty;layout.Padding=new Padding(14,15,14,10);
        layout.ColumnCount=1;layout.RowCount=6;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,24f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,44f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,43f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,30f));side.Controls.Add(layout);
        Label title=NewSectionTitle("Characters",0,0);
        title.Font=new Font("Segoe UI Semibold",14f);title.Dock=DockStyle.Fill;title.TextAlign=ContentAlignment.MiddleLeft;
        layout.Controls.Add(title,0,0);
        characterCountLabel.Text="32 official  •  10 custom";
        characterCountLabel.Tag="MutedText";characterCountLabel.Dock=DockStyle.Fill;
        characterCountLabel.TextAlign=ContentAlignment.MiddleLeft;layout.Controls.Add(characterCountLabel,0,1);
        searchBox.Dock=DockStyle.Fill;searchBox.Margin=new Padding(0,6,0,7);
        searchBox.TextChanged+=delegate{PopulateList();};layout.Controls.Add(searchBox,0,2);
        characterFilter.Items.AddRange(new object[]{"All","Official","Custom"});
        characterFilter.SelectedIndex=0;
        characterFilter.SelectedIndexChanged+=delegate{PopulateList();UpdateFilterAppearance();};
        TableLayoutPanel filters=new TableLayoutPanel();
        filters.Dock=DockStyle.Fill;filters.Padding=new Padding(0,3,0,3);filters.ColumnCount=3;
        for(int i=0;i<3;i++)
        {
            int index=i;filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3f));
            Button b=filterButtons[i];b.Text=new string[]{"All","Official","Custom"}[i];b.Dock=DockStyle.Fill;
            b.Cursor=Cursors.Hand;b.Tag="Filter";b.Margin=new Padding(i==0?0:3,0,i==2?0:3,0);
            b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderSize=0;
            b.Click+=delegate{characterFilter.SelectedIndex=index;};
            filters.Controls.Add(b,i,0);
        }
        layout.Controls.Add(filters,0,3);
        characterList.Dock=DockStyle.Fill;characterList.Margin=new Padding(0,5,0,0);
        characterList.SelectionChanged+=delegate{UpdateDetails();};
        layout.Controls.Add(characterList,0,4);
        Label foot=NewMutedLabel("Select a character to edit its save",0,0);
        foot.AutoSize=false;foot.Dock=DockStyle.Fill;foot.TextAlign=ContentAlignment.MiddleLeft;
        layout.Controls.Add(foot,0,5);
        return side;
    }

    void UpdateFilterAppearance()
    {
        for(int i=0;i<3;i++)
        {
            bool on=i==characterFilter.SelectedIndex;
            filterButtons[i].BackColor=on?SurfaceRaised:Surface;
            filterButtons[i].ForeColor=on?AccentTextColor:Muted;
            filterButtons[i].UseVisualStyleBackColor=false;
            filterButtons[i].FlatStyle=FlatStyle.Flat;
            filterButtons[i].FlatAppearance.BorderSize=0;
        }
    }

    Control BuildTabs(){tabs.Dock=DockStyle.Fill;tabs.Font=new Font("Segoe UI Semibold",9.5f);tabs.Padding=new Point(16,6);tabs.Appearance=TabAppearance.FlatButtons;tabs.SizeMode=TabSizeMode.Fixed;tabs.ItemSize=new Size(124,34);tabs.BackColor=Canvas;tabs.Tag="Canvas";BuildOverviewTab();BuildStatsTab();BuildEquipmentTab();BuildProgressTab();BuildUnlockTab();BuildBackupsTab();return tabs;}
    TabPage CreateTab(string text){TabPage page=new TabPage(text);page.BackColor=Canvas;page.ForeColor=TextPrimary;page.Tag="Canvas";page.Padding=Padding.Empty;page.AutoScroll=true;page.AutoScrollMinSize=new Size(850,0);tabs.TabPages.Add(page);return page;}
    TableLayoutPanel CreatePageLayout(TabPage page){TableLayoutPanel layout=new TableLayoutPanel();layout.Dock=DockStyle.Top;layout.AutoSize=true;layout.AutoSizeMode=AutoSizeMode.GrowAndShrink;layout.MinimumSize=new Size(850,0);layout.Margin=Padding.Empty;layout.Padding=new Padding(14,16,14,16);layout.ColumnCount=1;layout.RowCount=0;layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));page.Controls.Add(layout);return layout;}
    void AddFullRow(TableLayoutPanel layout,Control control,int height){int row=layout.RowCount;layout.RowCount++;layout.RowStyles.Add(new RowStyle(SizeType.Absolute,height+14));control.Dock=DockStyle.Fill;control.Margin=new Padding(0,0,0,14);layout.Controls.Add(control,0,row);}
}
