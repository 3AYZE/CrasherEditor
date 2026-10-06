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
    void RefreshBackups()
    {
        backupList.Items.Clear();
        if(session==null){backupInfo.Text="Load a save to view backups.";return;}
        string dir=Path.GetDirectoryName(session.Path);
        string pattern=Path.GetFileName(session.Path)+".crasher_backup_*";
        FileInfo[] files=new DirectoryInfo(dir).GetFiles(pattern);
        Array.Sort(files,delegate(FileInfo a,FileInfo b){return b.LastWriteTime.CompareTo(a.LastWriteTime);});
        foreach(FileInfo fi in files)
        {
            ListViewItem item=new ListViewItem(fi.Name);
            item.SubItems.Add(fi.LastWriteTime.ToString("yyyy-MM-dd  HH:mm:ss",CultureInfo.InvariantCulture));
            item.SubItems.Add(FormatBytes(fi.Length));
            item.Tag=fi.FullName;
            item.ImageIndex=0;
            backupList.Items.Add(item);
        }
        backupInfo.Text=files.Length.ToString(CultureInfo.InvariantCulture)+" backup"+(files.Length==1?"":"s")+" found beside cc_save.dat.";
    }

    static string FormatBytes(long n){if(n>=1024*1024)return(n/(1024.0*1024.0)).ToString("0.0",CultureInfo.InvariantCulture)+" MB";if(n>=1024)return(n/1024.0).ToString("0.0",CultureInfo.InvariantCulture)+" KB";return n.ToString(CultureInfo.InvariantCulture)+" B";}
    string SelectedBackupPath(){if(backupList.SelectedItems.Count==0)throw new InvalidOperationException("Select a backup first.");return Convert.ToString(backupList.SelectedItems[0].Tag,CultureInfo.InvariantCulture);}

    void RestoreLatestBackup()
    {
        if(HasPendingChanges())
        {
            if(!Confirm("You have unsaved changes. Restoring a backup will discard them. Continue?"))return;
            session.Plain=(byte[])committedPlain.Clone();pendingDangerous=false;ClearMaxAllCharacterSnapshot();ClearDraftInputsSilent();UpdatePendingUi();
        }
        EnsureLoaded();
        string dir=Path.GetDirectoryName(session.Path),pattern=Path.GetFileName(session.Path)+".crasher_backup_*";
        FileInfo[] files=new DirectoryInfo(dir).GetFiles(pattern);
        if(files.Length==0)throw new InvalidOperationException("No automatic backup was found.");
        Array.Sort(files,delegate(FileInfo a,FileInfo b){return b.LastWriteTime.CompareTo(a.LastWriteTime);});
        string backup=files[0].FullName;
        SaveSession validated=OpenSpecificSave(backup,session.AccountId);
        if(!Confirm("Restore the newest automatic backup?\n\n"+Path.GetFileName(backup)+"\n\nThe current save will be backed up again before restore.\n\nMake sure Castle Crashers is completely closed before continuing."))return;
        string safety=session.Path+".crasher_backup_before_restore_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff",CultureInfo.InvariantCulture);
        File.Copy(session.Path,safety,true);File.Copy(backup,session.Path,true);
        validated.Path=session.Path;validated.Modified=File.GetLastWriteTime(session.Path);session=validated;committedPlain=(byte[])session.Plain.Clone();pendingDangerous=false;ClearMaxAllCharacterSnapshot();
        int idx=SelectedIndex;savePathLabel.Text=session.Path;PopulateList();SelectCharacter(idx);UpdateDetails();RefreshBackups();saveStateLabel.Text="Restored";saveStateLabel.ForeColor=HeaderGood;SetStatus("Newest backup restored. Safety backup: "+Path.GetFileName(safety));
    }

    void RestoreSelectedBackup()
    {
        if(HasPendingChanges())
        {
            if(!Confirm("You have unsaved changes. Restoring a backup will discard them. Continue?"))return;
            session.Plain=(byte[])committedPlain.Clone();pendingDangerous=false;ClearMaxAllCharacterSnapshot();ClearDraftInputsSilent();UpdatePendingUi();
        }
        EnsureLoaded();
        string backup=SelectedBackupPath();
        if(!File.Exists(backup))throw new FileNotFoundException("The selected backup no longer exists.",backup);
        SaveSession validated=OpenSpecificSave(backup,session.AccountId);
        if(!Confirm("Restore this backup?\n\n"+Path.GetFileName(backup)+"\n\nThe current save will be backed up again before restore.\n\nMake sure Castle Crashers is completely closed before continuing."))return;
        string safety=session.Path+".crasher_backup_before_restore_"+DateTime.Now.ToString("yyyyMMdd_HHmmss",CultureInfo.InvariantCulture);
        File.Copy(session.Path,safety,true);File.Copy(backup,session.Path,true);
        validated.Path=session.Path;validated.Modified=File.GetLastWriteTime(session.Path);session=validated;committedPlain=(byte[])session.Plain.Clone();pendingDangerous=false;ClearMaxAllCharacterSnapshot();
        int idx=SelectedIndex;savePathLabel.Text=session.Path;PopulateList();SelectCharacter(idx);UpdateDetails();RefreshBackups();saveStateLabel.Text="Restored";saveStateLabel.ForeColor=HeaderGood;SetStatus("Backup restored. Safety backup: "+Path.GetFileName(safety));
    }

    void DeleteSelectedBackup(){EnsureLoaded();string backup=SelectedBackupPath();if(!Confirm("Delete this backup file?\n\n"+Path.GetFileName(backup)))return;File.Delete(backup);RefreshBackups();SetStatus("Backup deleted: "+Path.GetFileName(backup));}
    void EnsureLoaded(){if(session==null)throw new InvalidOperationException("Load the save first.");}
    int? ParseOptional(string key,int min,int max){string s=edits[key].Text.Trim();if(s.Length==0)return null;int n;if(!Int32.TryParse(s,NumberStyles.Integer,CultureInfo.InvariantCulture,out n))throw new InvalidOperationException(key+" must be a whole number.");if(n<min||n>max)throw new InvalidOperationException(key+" must be "+min+"–"+max+".");return n;}

    string SaveEncrypted()
    {
        uint sum=CCSaveCrypto.Checksum(session.Plain,session.Plain.Length-4);
        CCSaveCrypto.WriteU32LE(session.Plain,session.Plain.Length-4,sum);
        byte[] enc=session.Crypto.Encrypt(session.Plain);
        string stamp=DateTime.Now.ToString("yyyyMMdd_HHmmss_fff",CultureInfo.InvariantCulture);
        string backup=session.Path+".crasher_backup_"+stamp;
        File.Copy(session.Path,backup,true);
        File.WriteAllBytes(session.Path,enc);
        session.Modified=File.GetLastWriteTime(session.Path);
        return backup;
    }

    static int CountProgressBits(byte[] data,int off){int count=0;for(int i=0;i<3;i++){int b=data[off+i];while(b!=0){count+=b&1;b>>=1;}}return count;}
    static void WriteProgressBits(byte[] data,int off,int total){if(total<0||total>17)throw new InvalidOperationException("Progress value must be 0–17 in the standard Steam checkpoint model.");for(int i=0;i<3;i++){if(total>=8){data[off+i]=0xFF;total-=8;}else if(total>0){data[off+i]=(byte)((1<<total)-1);total=0;}else data[off+i]=0;}}

    static CharacterData ReadCharacter(byte[] plain,int i)
    {
        int o=CharacterOffset(i);CharacterData c=new CharacterData();c.Index=i;c.Name=CharacterNames[i];c.Unlocked=plain[o]==0x80;c.Level=Math.Min(99,plain[o+0x01]+1);c.XP=CCSaveCrypto.ReadI32BE(plain,o+0x02);c.Gold=CCSaveCrypto.ReadI32BE(plain,o+0x13);c.Strength=Math.Min(25,(int)plain[o+0x08]);c.Defense=Math.Min(25,(int)plain[o+0x09]);c.Magic=Math.Min(25,(int)plain[o+0x0A]);c.Agility=Math.Min(25,(int)plain[o+0x0B]);c.Potions=plain[o+0x0F];c.Bombs=plain[o+0x10];c.Sandwiches=plain[o+0x11];return c;
    }
    static int CharacterOffset(int i){return 0x40+i*0x30;}
    void SelectCharacter(int idx){foreach(ListViewItem item in characterList.Items){if((int)item.Tag==idx){item.Selected=true;characterList.Select();break;}}}
}
