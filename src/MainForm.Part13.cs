using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    void UpdateCharacterBulkControl()
    {
        const int total = 24;
        if (session == null)
        {
            characterBulkStatus.Text = "Load a save to manage characters";
            characterBulkButton.Text = "Unlock All Characters";
            StyleButton(characterBulkButton, Accent);
            characterBulkButton.Enabled = false;
            maxAllCharactersButton.Text = "MAX All Characters";
            StyleButton(maxAllCharactersButton, Good);
            maxAllCharactersButton.Enabled = false;
            return;
        }
        int unlocked = CountUnlockedNormalCharacters();
        bool allUnlocked = unlocked == total;
        characterBulkStatus.Text = unlocked.ToString(CultureInfo.InvariantCulture) + "/" + total.ToString(CultureInfo.InvariantCulture) + " unlockable  •  MAX = stats/items";
        characterBulkButton.Text = allUnlocked ? "Lock All Characters" : "Unlock All Characters";
        StyleButton(characterBulkButton, allUnlocked ? Danger : Accent);
        characterBulkButton.Enabled = true;
        maxAllCharactersButton.Text = maxAllCharacterSnapshotActive ? "Undo MAX All" : "MAX All Characters";
        StyleButton(maxAllCharactersButton, maxAllCharacterSnapshotActive ? Warn : Good);
        maxAllCharactersButton.Enabled = true;
    }

    void ToggleAllCharactersSmart()
    {
        EnsureLoaded();
        const int total = 24;
        int unlocked = CountUnlockedNormalCharacters();
        bool allUnlocked = unlocked == total;
        int idx = SelectedIndex;
        if (allUnlocked)
        {
            for (int i = 4; i < 28; i++) session.Plain[CharacterOffset(i)] = 0x00;
            pendingDangerous = true;
            MarkPending("All normal characters locked", true);
        }
        else
        {
            for (int i = 4; i < 28; i++) session.Plain[CharacterOffset(i)] = 0x80;
            MarkPending("All normal characters unlocked", false);
        }
        PopulateList();
        SelectCharacter(idx);
        UpdateDetails();
        UpdateCharacterBulkControl();
    }

    void ClearMaxAllCharacterSnapshot(){maxAllCharacterSnapshot=null;maxAllCharacterSnapshotActive=false;}

    void StageMaxAllCharacters()
    {
        EnsureLoaded();
        if (maxAllCharacterSnapshotActive){UndoMaxAllCharacters();return;}
        byte[,] snapshot=new byte[CharacterNames.Length,MaxAllCharacterFieldOffsets.Length];
        bool changed=false;
        for(int i=0;i<CharacterNames.Length;i++)
        {
            int o=CharacterOffset(i);
            for(int f=0;f<MaxAllCharacterFieldOffsets.Length;f++) snapshot[i,f]=session.Plain[o+MaxAllCharacterFieldOffsets[f]];
            if(session.Plain[o+0x08]!=25||session.Plain[o+0x09]!=25||session.Plain[o+0x0A]!=25||session.Plain[o+0x0B]!=25||session.Plain[o+0x0F]!=9||session.Plain[o+0x10]!=9||session.Plain[o+0x11]!=9) changed=true;
        }
        if(!changed){SetStatus("All official characters already have MAX stats/items.");UpdateCharacterBulkControl();return;}
        maxAllCharacterSnapshot=snapshot;maxAllCharacterSnapshotActive=true;
        for(int i=0;i<CharacterNames.Length;i++)
        {
            int o=CharacterOffset(i);
            session.Plain[o+0x08]=25; session.Plain[o+0x09]=25; session.Plain[o+0x0A]=25; session.Plain[o+0x0B]=25;
            session.Plain[o+0x0F]=9; session.Plain[o+0x10]=9; session.Plain[o+0x11]=9;
        }
        int idx=SelectedIndex;
        PopulateList();SelectCharacter(idx);UpdateDetails();
        MarkPending("MAX stats/items staged for all "+CharacterNames.Length.ToString(CultureInfo.InvariantCulture)+" official characters; press Undo MAX All before Apply to restore the exact prior values",false);
    }

    void UndoMaxAllCharacters()
    {
        EnsureLoaded();
        if(!maxAllCharacterSnapshotActive||maxAllCharacterSnapshot==null){SetStatus("There is no staged MAX All operation to undo.");UpdateCharacterBulkControl();return;}
        for(int i=0;i<CharacterNames.Length;i++)
        {
            int o=CharacterOffset(i);
            for(int f=0;f<MaxAllCharacterFieldOffsets.Length;f++) session.Plain[o+MaxAllCharacterFieldOffsets[f]]=maxAllCharacterSnapshot[i,f];
        }
        ClearMaxAllCharacterSnapshot();
        int idx=SelectedIndex;PopulateList();SelectCharacter(idx);UpdateDetails();UpdatePendingUi();
        SetStatus(HasPendingChanges()?"MAX All undone. Earlier staged changes are still unsaved.":"MAX All undone. Returned to the last saved values.");
    }

    void UnlockAllCharacters(){EnsureLoaded();int idx=SelectedIndex;for(int i=4;i<28;i++)session.Plain[CharacterOffset(i)]=0x80;PopulateList();SelectCharacter(idx);UpdateDetails();MarkPending("All normal characters unlocked",false);UpdateCharacterBulkControl();}
    void LockAllCharacters(){EnsureLoaded();int idx=SelectedIndex;for(int i=4;i<28;i++)session.Plain[CharacterOffset(i)]=0x00;pendingDangerous=true;PopulateList();SelectCharacter(idx);UpdateDetails();MarkPending("All normal characters locked",true);UpdateCharacterBulkControl();}

    void UnlockAllPets(){RunGlobalAction("Unlock all Animal Orbs in your profile?\n\nRestore Latest Backup is the recommended exact undo.",delegate{session.Plain[8]=0xFE;for(int i=9;i<=11;i++)session.Plain[i]=0xFF;},"Animal Orb collection unlock applied");}
    void UnlockAllWeapons(){RunGlobalAction("Unlock the weapon collection in your profile?\n\nRestore Latest Backup is the recommended exact undo.",delegate{for(int i=18;i<=30;i++)session.Plain[i]=0xFF;},"Weapon collection unlock applied");}
    void UnlockGlobalRelics(){throw new InvalidOperationException("Global relic unlocking is not used in this build. Character collectables are edited from the Progress tab.");}
    void EnableKeyItems(){RunGlobalAction("Enable Bow, Boomerang and Shovel in your profile?\n\nRestore Latest Backup is the recommended exact undo.",delegate{session.Plain[12]=0x4E;},"Bow, Boomerang and Shovel enabled");}

    void LockAllPets(){RunGlobalAction("FORCE LOCK all Animal Orbs?\n\nThis is not an exact undo and may remove legitimately earned pet unlocks. Use Restore Latest Backup for an exact return.",delegate{for(int i=8;i<=11;i++)session.Plain[i]=0x00;},"Animal Orb profile range force-locked");}
    void LockAllWeapons(){RunGlobalAction("FORCE LOCK the weapon collection?\n\nThis is not an exact undo and may remove legitimately earned weapon unlocks. Use Restore Latest Backup for an exact return.",delegate{for(int i=18;i<=30;i++)session.Plain[i]=0x00;},"Weapon profile range force-locked");}
    void DisableKeyItems(){RunGlobalAction("Disable the Bow, Boomerang and Shovel profile state?\n\nThis is not an exact undo. Use Restore Latest Backup for an exact return.",delegate{session.Plain[12]=0x00;},"Bow, Boomerang and Shovel profile state cleared");}

    void ForceLockEverything(){EnsureLoaded();for(int i=4;i<28;i++)session.Plain[CharacterOffset(i)]=0x00;for(int i=8;i<=11;i++)session.Plain[i]=0x00;session.Plain[12]=0x00;for(int i=18;i<=30;i++)session.Plain[i]=0x00;pendingDangerous=true;PopulateList();SelectCharacter(SelectedIndex);UpdateDetails();MarkPending("Force Lock All staged",true);}
    void UnlockEverything(){RunGlobalAction("Apply every unlock action supported by this menu?\n\nThis includes normal character flags plus Animal Orb, key item, and weapon profile unlocks.\n\nUse Restore Latest Backup if you want to return.",delegate{for(int i=4;i<28;i++)session.Plain[CharacterOffset(i)]=0x80;session.Plain[8]=0xFE;for(int i=9;i<=11;i++)session.Plain[i]=0xFF;session.Plain[12]=0x4E;for(int i=18;i<=30;i++)session.Plain[i]=0xFF;},"All supported profile unlock operations applied");}
    void ResetAllUnlocks(){throw new InvalidOperationException("Reset All Unlocks is unavailable because clearing profile collection bytes cannot reliably reconstruct the player's legitimate previous state. Use Restore Latest Backup instead.");}
    void RunGlobalAction(string confirmText,Action mutation,string success){EnsureLoaded();mutation();pendingDangerous=true;PopulateList();SelectCharacter(SelectedIndex);UpdateDetails();MarkPending(success+" staged",true);}
    bool Confirm(string message){return MessageBox.Show(this,message,"Confirm",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes;}
    void RefreshUiAfterWrite(int idx){PopulateList();SelectCharacter(idx);UpdateDetails();RefreshBackups();UpdatePendingUi();}
    void ShowSaved(string action,string backup){SetStatus(action+". Backup: "+Path.GetFileName(backup));}
}
