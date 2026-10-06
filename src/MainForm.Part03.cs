
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    TableLayoutPanel NewTwoColumnRow(Control left, Control right)
    {
        TableLayoutPanel row = new TableLayoutPanel(); row.ColumnCount=2; row.RowCount=1; row.Margin=Padding.Empty; row.Padding=Padding.Empty; row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50f)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50f)); row.RowStyles.Add(new RowStyle(SizeType.Percent,100f)); left.Dock=DockStyle.Fill; right.Dock=DockStyle.Fill; left.Margin=new Padding(0,0,7,0); right.Margin=new Padding(7,0,0,0); row.Controls.Add(left,0,0); row.Controls.Add(right,1,0); return row;
    }
    TableLayoutPanel NewThreeColumnRow(Control a,Control b,Control c)
    {
        TableLayoutPanel row=new TableLayoutPanel(); row.ColumnCount=3; row.RowCount=1; row.Margin=Padding.Empty; row.Padding=Padding.Empty; row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.34f)); row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f)); row.RowStyles.Add(new RowStyle(SizeType.Percent,100f)); a.Dock=b.Dock=c.Dock=DockStyle.Fill; a.Margin=new Padding(0,0,7,0); b.Margin=new Padding(7,0,7,0); c.Margin=new Padding(7,0,0,0); row.Controls.Add(a,0,0); row.Controls.Add(b,1,0); row.Controls.Add(c,2,0); return row;
    }
    void BuildOverviewTab()
    {
        TabPage page=CreateTab("Overview"); TableLayoutPanel layout=CreatePageLayout(page);
        Panel card=NewCard(0,0,820,218); selectedName.Text="No character selected"; selectedName.Font=new Font("Segoe UI Semibold",17.0f); selectedName.AutoSize=false; selectedName.AutoEllipsis=true; selectedName.Location=new Point(18,16); selectedName.Size=new Size(450,32); selectedName.Anchor=AnchorStyles.Top|AnchorStyles.Left; card.Controls.Add(selectedName);
        selectedState.Text="Load a save to view details."; selectedState.ForeColor=Muted; selectedState.Tag="MutedText"; selectedState.AutoSize=false; selectedState.AutoEllipsis=true; selectedState.Location=new Point(20,51); selectedState.Size=new Size(450,24); selectedState.Anchor=AnchorStyles.Top|AnchorStyles.Left; card.Controls.Add(selectedState);
        Label supportTitle=NewMutedLabel("Supported Limits",520,20); supportTitle.AutoSize=false; supportTitle.Size=new Size(240,20); card.Controls.Add(supportTitle);
        Label supportValue=new Label(); supportValue.Text="Level 1–99 • Stats 1–25"; supportValue.Font=new Font("Segoe UI Semibold",9.0f); supportValue.ForeColor=TextPrimary; supportValue.Tag="PrimaryText"; supportValue.AutoSize=false; supportValue.AutoEllipsis=true; supportValue.Location=new Point(520,40); supportValue.Size=new Size(260,24); supportValue.TextAlign=ContentAlignment.MiddleLeft; card.Controls.Add(supportValue);
        AddSummaryValue(card,"Level","Level",20,92); AddSummaryValue(card,"XP","XP",214,92); AddSummaryValue(card,"Gold","Gold",408,92); AddSummaryValue(card,"Unlocked","Status",602,92); AddSummaryValue(card,"Strength","STR",20,148); AddSummaryValue(card,"Magic","MAG",214,148); AddSummaryValue(card,"Defense","DEF",408,148); AddSummaryValue(card,"Agility","AGI",602,148); AddFullRow(layout,card,218);
        Panel inv=NewCard(0,0,250,194); inv.Controls.Add(NewSectionTitle("Inventory",16,12)); AddCompactSummaryValue(inv,"Potions","Potions",18,42,160); AddCompactSummaryValue(inv,"Bombs","Bombs",18,88,160); AddCompactSummaryValue(inv,"Sandwiches","Sandwiches",18,134,160);
        Panel eq=NewCard(0,0,250,194); eq.Controls.Add(NewSectionTitle("Equipment",16,12)); eq.Controls.Add(NewMutedLabel("Weapon",18,48)); selectedWeapon.Font=new Font("Segoe UI Semibold",9.0f); selectedWeapon.AutoEllipsis=true; selectedWeapon.AutoSize=false; selectedWeapon.Size=new Size(160,22); selectedWeapon.Location=new Point(84,46); selectedWeapon.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right; eq.Controls.Add(selectedWeapon); eq.Controls.Add(NewMutedLabel("Pet",18,100)); selectedPet.Font=new Font("Segoe UI Semibold",9.0f); selectedPet.AutoEllipsis=true; selectedPet.AutoSize=false; selectedPet.Size=new Size(160,22); selectedPet.Location=new Point(84,98); selectedPet.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right; eq.Controls.Add(selectedPet);
        Panel prog=NewCard(0,0,250,194); prog.Controls.Add(NewSectionTitle("Progress",16,12)); selectedProgress.ForeColor=TextSecondary; selectedProgress.Tag="SecondaryText"; selectedProgress.AutoSize=false; selectedProgress.AutoEllipsis=true; selectedProgress.Size=new Size(220,132); selectedProgress.Location=new Point(18,44); selectedProgress.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right; prog.Controls.Add(selectedProgress); AddFullRow(layout,NewThreeColumnRow(inv,eq,prog),194);
        Panel quick=NewCard(0,0,820,104); quick.Controls.Add(NewSectionTitle("Quick Actions",16,12)); ConfigureButton(toggleButton,"Toggle Lock",18,50,154,38); StyleButton(toggleButton,Accent); toggleButton.Enabled=false; toggleButton.Click+=delegate{TryUiAction(ToggleUnlock);}; quick.Controls.Add(toggleButton); ConfigureButton(overviewApplyButton,"Apply All Changes",184,50,164,38); StyleButton(overviewApplyButton,Good); overviewApplyButton.Enabled=false; overviewApplyButton.Click+=delegate{TryUiAction(ApplyPendingChanges);}; quick.Controls.Add(overviewApplyButton); Label quickNote=NewMutedLabel("Use Apply Changes to write every staged edit to the save file.",370,60); quickNote.AutoSize=false; quickNote.AutoEllipsis=true; quickNote.Size=new Size(420,20); quick.Controls.Add(quickNote); AddFullRow(layout,quick,104);
    }
}
