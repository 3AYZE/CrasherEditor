
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    void DrawListItem(object sender,DrawListViewItemEventArgs e){if(((ListView)sender).View==View.Details)return;Color background=e.Item.Selected?Accent:InputSurface;Color foreground=e.Item.Selected?Color.White:TextPrimary;using(SolidBrush brush=new SolidBrush(background))e.Graphics.FillRectangle(brush,e.Bounds);TextRenderer.DrawText(e.Graphics,e.Item.Text,((ListView)sender).Font,e.Bounds,foreground,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);}
    int CurrentLevelInputMax(){return 99;} int CurrentStatInputMax(){return 25;}
    bool TryGetFieldRange(string key,out int min,out int max){min=0;max=0;if(key=="Level"){min=1;max=CurrentLevelInputMax();return true;}if(key=="XP"||key=="Gold"){min=0;max=100000000;return true;}if(key=="Potions"||key=="Bombs"||key=="Sandwiches"){min=0;max=9;return true;}if(key=="Strength"||key=="Magic"||key=="Defense"||key=="Agility"){min=1;max=CurrentStatInputMax();return true;}return false;}
    bool IsDraftFieldValid(string key,string raw){if(String.IsNullOrWhiteSpace(raw))return true;int min,max,value;if(!TryGetFieldRange(key,out min,out max))return true;return Int32.TryParse(raw.Trim(),NumberStyles.Integer,CultureInfo.InvariantCulture,out value)&&value>=min&&value<=max;}
    void RefreshInputValidationColors(){foreach(KeyValuePair<string,TextBox> kv in edits)kv.Value.BackColor=IsDraftFieldValid(kv.Key,kv.Value.Text)?InputSurface:InvalidInput;UpdateCharacterValueModeUi();}
    void TryUiAction(Action action){try{action();}catch(Exception ex){Log(ex.ToString());MessageBox.Show(this,ex.Message,"Crasher Editor V1.4",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    void SetStatus(string message){statusText.Text=message;}
    static void Log(string text){try{string p=Path.Combine(Path.GetTempPath(),"Crasher_Editor_V1.4.log");File.AppendAllText(p,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)+"  "+text+Environment.NewLine);}catch{}}
    static bool IsToggleable(int idx){return idx>=4&&idx<28;}
    string AccessText(int idx,bool rawUnlocked){if(idx<4)return"Starter";if(idx>=OfficialCharacterCount)return"Workshop";if(idx>=28)return"DLC";return rawUnlocked?"Unlocked":"Locked";}
    string CharacterDisplayName(int idx)
    {
        if(idx<0||idx>=CharacterNames.Length)return "Unknown";
        if(idx<OfficialCharacterCount||session==null)return CharacterNames[idx];
        string label=WorkshopSlotLabels.Get(session.AccountId,idx-OfficialCharacterCount);
        return String.IsNullOrWhiteSpace(label)?CharacterNames[idx]:label+" (C"+(idx-OfficialCharacterCount+1).ToString("00",CultureInfo.InvariantCulture)+")";
    }

    void EditWorkshopSlotName()
    {
        EnsureLoaded();
        int index=SelectedIndex;
        if(index<OfficialCharacterCount||index>=CharacterRecordCount)
            throw new InvalidOperationException("Select Custom Slot 01–10 first.");
        int slot=index-OfficialCharacterCount;
        using(Form dialog=new Form())
        {
            dialog.Text="Name custom slot";
            dialog.ClientSize=new Size(420,170);
            dialog.FormBorderStyle=FormBorderStyle.FixedDialog;
            dialog.StartPosition=FormStartPosition.CenterParent;
            dialog.MaximizeBox=false;dialog.MinimizeBox=false;dialog.ShowInTaskbar=false;
            Label description=new Label();
            description.Text="Local label only. Removing it never resets saved stats.";
            description.Location=new Point(16,15);description.Size=new Size(385,45);
            dialog.Controls.Add(description);
            TextBox name=new TextBox();
            name.Text=WorkshopSlotLabels.Get(session.AccountId,slot);
            name.MaxLength=50;name.Location=new Point(16,68);name.Width=388;
            dialog.Controls.Add(name);
            Button cancel=new Button();
            cancel.Text="Cancel";cancel.DialogResult=DialogResult.Cancel;cancel.Location=new Point(215,121);
            dialog.Controls.Add(cancel);
            Button save=new Button();
            save.Text="Save";save.DialogResult=DialogResult.OK;save.Location=new Point(310,121);
            dialog.Controls.Add(save);dialog.AcceptButton=save;dialog.CancelButton=cancel;
            if(dialog.ShowDialog(this)!=DialogResult.OK)return;
            WorkshopSlotLabels.Set(session.AccountId,slot,name.Text);
        }
        PopulateList();SelectCharacter(index);UpdateDetails();
        SetStatus("Custom slot label updated locally. The game save was not modified.");
    }

    void PopulateList()
    {
        int preferred=characterList.SelectedSlot;
        string filter=searchBox.Text.Trim();
        List<ModernCharacterList.Entry> entries=new List<ModernCharacterList.Entry>();
        for(int i=0;i<CharacterNames.Length;i++)
        {
            if(characterFilter.SelectedIndex==1&&i>=OfficialCharacterCount)continue;
            if(characterFilter.SelectedIndex==2&&i<OfficialCharacterCount)continue;
            string name=CharacterDisplayName(i);
            string code=i>=OfficialCharacterCount?"C"+(i-OfficialCharacterCount+1).ToString("00",CultureInfo.InvariantCulture):(i+1).ToString("00",CultureInfo.InvariantCulture);
            if(filter.Length>0&&name.IndexOf(filter,StringComparison.OrdinalIgnoreCase)<0&&code.IndexOf(filter,StringComparison.OrdinalIgnoreCase)<0)continue;
            CharacterData data=session==null?null:ReadCharacter(session.Plain,i);
            string kind=i>=OfficialCharacterCount?"CUSTOM "+code:i<4?"STARTER":i>=28?"DLC":"OFFICIAL";
            string detail=data==null?kind:kind+"  •  LV "+data.Level.ToString(CultureInfo.InvariantCulture)+(IsToggleable(i)&&!data.Unlocked?"  •  LOCKED":"");
            entries.Add(new ModernCharacterList.Entry(i,name,detail,data!=null&&IsToggleable(i)&&!data.Unlocked));
        }
        characterList.SetEntries(entries,preferred);
        characterCountLabel.Text=entries.Count.ToString(CultureInfo.InvariantCulture)+" shown  •  32 official  •  10 custom";
        UpdateCharacterBulkControl();
    }
    int SelectedIndex { get { return characterList.SelectedSlot; } }

    void UpdateDetails(){int idx=SelectedIndex;string name=idx>=0&&idx<CharacterNames.Length?CharacterDisplayName(idx):"No character selected";workshopNameButton.Enabled=session!=null&&idx>=OfficialCharacterCount&&idx<CharacterRecordCount;selectedName.Text=name;if(session==null){selectedState.Text="Load a save to view details.";toggleButton.Text="Toggle Unlock";toggleButton.Enabled=false;foreach(Label l in summary.Values)l.Text="—";selectedWeapon.Text="—";selectedPet.Text="—";selectedProgress.Text="—";currentLevelOverrideLabel.Text="Current: —";UpdateCharacterBalanceUi();return;}CharacterData c=ReadCharacter(session.Plain,idx);int o=CharacterOffset(idx);int weapon=session.Plain[o+0x06];int pet=session.Plain[o+0x07];int normalCount=CountProgressBits(session.Plain,o+0x0C);bool insaneUnlocked=session.Plain[o+0x17]!=0;int insaneCount=CountProgressBits(session.Plain,o+0x18);int skull=session.Plain[o+0x1B];byte collects=session.Plain[o+0x12];selectedName.Text=CharacterDisplayName(idx);selectedState.Text="Slot "+(idx+1).ToString("D2",CultureInfo.InvariantCulture)+"  •  "+AccessText(idx,c.Unlocked);toggleButton.Text=c.Unlocked?"Toggle Lock":"Toggle Unlock";toggleButton.Enabled=IsToggleable(idx);summary["Level"].Text=c.Level.ToString(CultureInfo.InvariantCulture);currentLevelOverrideLabel.Text="Current • Lv "+c.Level.ToString(CultureInfo.InvariantCulture)+" • STR "+c.Strength.ToString(CultureInfo.InvariantCulture)+" • DEF "+c.Defense.ToString(CultureInfo.InvariantCulture)+" • MAG "+c.Magic.ToString(CultureInfo.InvariantCulture)+" • AGI "+c.Agility.ToString(CultureInfo.InvariantCulture);summary["XP"].Text=c.XP.ToString("N0",CultureInfo.InvariantCulture);summary["Gold"].Text=c.Gold.ToString("N0",CultureInfo.InvariantCulture);summary["Unlocked"].Text=AccessText(idx,c.Unlocked);summary["Strength"].Text=c.Strength.ToString(CultureInfo.InvariantCulture);summary["Magic"].Text=c.Magic.ToString(CultureInfo.InvariantCulture);summary["Defense"].Text=c.Defense.ToString(CultureInfo.InvariantCulture);summary["Agility"].Text=c.Agility.ToString(CultureInfo.InvariantCulture);summary["Potions"].Text=c.Potions.ToString(CultureInfo.InvariantCulture);summary["Bombs"].Text=c.Bombs.ToString(CultureInfo.InvariantCulture);summary["Sandwiches"].Text=c.Sandwiches.ToString(CultureInfo.InvariantCulture);selectedWeapon.Text=WeaponDisplayName(weapon);selectedPet.Text=PetDisplayName(pet);selectedProgress.Text="Normal: "+ProgressDisplayName(normalCount)+Environment.NewLine+"Insane: "+(insaneUnlocked?ProgressDisplayName(insaneCount):"Locked")+Environment.NewLine+"Skull: "+SkullDisplayName(skull);PopulateEquipmentControls(weapon,pet);PopulateProgressControls(normalCount,insaneUnlocked,insaneCount,skull,collects);ClearDraftInputsSilent();UpdateCharacterBalanceUi();}
    string WeaponDisplayName(int id){if(id>=0&&id<WeaponNames.Length)return WeaponNames[id];return"Special ID "+id.ToString(CultureInfo.InvariantCulture);} string PetDisplayName(int id){if(id>=0&&id<PetNames.Length)return PetNames[id];return"ID "+id.ToString(CultureInfo.InvariantCulture);} string ProgressDisplayName(int count){if(count>=0&&count<ProgressNames.Length)return ProgressNames[count];return count.ToString(CultureInfo.InvariantCulture)+" checkpoint bits";} string SkullDisplayName(int v){if(v==1)return"White";if(v==255)return"Gold";if(v==2)return"Legacy Gold value (0x02)";if(v==0)return"None";return"Raw "+v.ToString(CultureInfo.InvariantCulture);}
    void PopulateEquipmentControls(int currentWeapon,int currentPet){bool oldSuppress=suppressControlEvents;suppressControlEvents=true;weaponCombo.BeginUpdate();weaponCombo.Items.Clear();if(currentWeapon>=WeaponNames.Length)weaponCombo.Items.Add(new ComboItem("(Keep current special ID "+currentWeapon.ToString(CultureInfo.InvariantCulture)+")",currentWeapon));for(int i=0;i<WeaponNames.Length;i++)weaponCombo.Items.Add(new ComboItem(i.ToString("D2",CultureInfo.InvariantCulture)+" — "+WeaponNames[i],i));SelectComboValue(weaponCombo,currentWeapon);weaponCombo.EndUpdate();petCombo.BeginUpdate();petCombo.Items.Clear();if(currentPet>=PetNames.Length)petCombo.Items.Add(new ComboItem("(Keep current ID "+currentPet.ToString(CultureInfo.InvariantCulture)+")",currentPet));for(int i=0;i<PetNames.Length;i++)petCombo.Items.Add(new ComboItem(i.ToString("D2",CultureInfo.InvariantCulture)+" — "+PetNames[i],i));SelectComboValue(petCombo,currentPet);petCombo.EndUpdate();weaponCurrentLabel.Text="Current ID "+currentWeapon.ToString(CultureInfo.InvariantCulture);petCurrentLabel.Text="Current ID "+currentPet.ToString(CultureInfo.InvariantCulture);suppressControlEvents=oldSuppress;}
}
