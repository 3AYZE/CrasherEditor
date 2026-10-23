using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    CheckBox BulkOption(string caption,int y,bool initial,Form parent)
    {
        CheckBox check=new CheckBox();
        check.Text=caption;
        check.Checked=initial;
        check.Location=new Point(22,y);
        check.Size=new Size(464,29);
        check.FlatStyle=FlatStyle.Flat;
        check.BackColor=Surface;
        check.ForeColor=TextPrimary;
        check.Font=new Font("Segoe UI",9.5f);
        parent.Controls.Add(check);
        return check;
    }

    void ShowBulkCompletionDialog()
    {
        EnsureLoaded();
        using(Form dialog=new Form())
        {
            dialog.Text="MAX & Complete All Characters";
            dialog.StartPosition=FormStartPosition.CenterParent;
            dialog.ClientSize=new Size(522,440);
            dialog.MinimumSize=dialog.MaximumSize=dialog.Size;
            dialog.MaximizeBox=false;
            dialog.MinimizeBox=false;
            dialog.FormBorderStyle=FormBorderStyle.FixedDialog;
            dialog.BackColor=Surface;
            dialog.ForeColor=TextPrimary;
            dialog.Font=new Font("Segoe UI",9f);
            dialog.ShowInTaskbar=false;
            Label title=new Label();
            title.Text="Complete all supported save progress";
            title.Font=new Font("Segoe UI Semibold",13.5f);
            title.Location=new Point(22,16);
            title.Size=new Size(470,30);
            dialog.Controls.Add(title);
            Label subtitle=new Label();
            subtitle.Text="Select what to stage. Nothing is written until Apply Changes.";
            subtitle.ForeColor=Muted;
            subtitle.Location=new Point(22,51);
            subtitle.Size=new Size(475,26);
            dialog.Controls.Add(subtitle);

            CheckBox workshop=BulkOption("Include 10 custom Workshop slots (42 total)",84,true,dialog);
            CheckBox stats=BulkOption("Level 99, matching XP, all four stats at 25",129,true,dialog);
            CheckBox items=BulkOption("9 Potions, Bombs and Sandwiches",163,true,dialog);
            CheckBox gold=BulkOption("At least 999,999 gold per character",197,true,dialog);
            CheckBox progress=BulkOption("Complete Normal + Insane, relics and gold skulls",231,true,dialog);
            CheckBox collections=BulkOption("Unlock official characters, weapons, Animal Orbs and key items",265,true,dialog);

            Label note=new Label();
            note.Text="Custom slots retain their assignments. Base stats above 25 and levels above 99 are unsupported; equipment bonuses can increase effective stats.";
            note.ForeColor=Muted;
            note.Location=new Point(22,307);
            note.Size=new Size(474,48);
            dialog.Controls.Add(note);

            Button cancel=new Button();
            cancel.Text="Cancel";
            cancel.Location=new Point(253,375);cancel.Size=new Size(110,40);
            cancel.DialogResult=DialogResult.Cancel;
            cancel.BackColor=HeaderButton;cancel.ForeColor=HeaderText;
            cancel.FlatStyle=FlatStyle.Flat;cancel.FlatAppearance.BorderSize=0;
            dialog.Controls.Add(cancel);
            Button stage=new Button();
            stage.Text="Stage Changes";
            stage.Location=new Point(374,375);stage.Size=new Size(126,40);
            stage.DialogResult=DialogResult.OK;
            stage.BackColor=Accent;stage.ForeColor=Color.White;
            stage.FlatStyle=FlatStyle.Flat;stage.FlatAppearance.BorderSize=0;
            dialog.Controls.Add(stage);
            dialog.AcceptButton=stage;dialog.CancelButton=cancel;
            if(dialog.ShowDialog(this)!=DialogResult.OK)return;

            BulkCompletionOptions options=new BulkCompletionOptions();
            options.IncludeWorkshopSlots=workshop.Checked;
            options.MaxLevelAndStats=stats.Checked;
            options.MaxConsumables=items.Checked;
            options.GiveGold=gold.Checked;
            options.CompleteNormalAndInsane=progress.Checked;
            options.UnlockCollections=collections.Checked;
            if(!options.MaxLevelAndStats&&!options.MaxConsumables&&!options.GiveGold&&!options.CompleteNormalAndInsane&&!options.UnlockCollections)
                throw new InvalidOperationException("Select at least one option.");

            byte[] updated=(byte[])session.Plain.Clone();
            int changed=BulkCompletion.Apply(updated,options);
            if(BytesEqual(updated,session.Plain))
            {
                SetStatus("All selected values are already set. No changes staged.");
                return;
            }
            int selected=SelectedIndex;
            session.Plain=updated;
            ClearMaxAllCharacterSnapshot();
            PopulateList();
            SelectCharacter(selected);
            UpdateDetails();
            MarkPending("MAX & Complete All staged for "+(options.IncludeWorkshopSlots?"42":"32")+" slots ("+changed+" character records changed). Review, then Apply Changes to create a backup and save",true);
        }
    }

    Panel BuildBulkCompletionCard()
    {
        Panel card=NewCard(0,0,820,106);
        Label header=NewSectionTitle("MAX & Complete All Characters",16,12);
        card.Controls.Add(header);
        Label description=NewMutedLabel("Set Level 99, max skills, items and completed progression for every selected slot.",16,44);
        description.AutoSize=false;description.AutoEllipsis=true;description.Size=new Size(582,44);
        card.Controls.Add(description);
        Button action=NewActionButton(card,"MAX & Complete All",Accent,delegate{TryUiAction(ShowBulkCompletionDialog);});
        ConfigureButton(action,"MAX & Complete All",620,31,180,42);
        action.Anchor=AnchorStyles.Top|AnchorStyles.Right;
        return card;
    }
}
