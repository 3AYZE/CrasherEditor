using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

public sealed partial class MainForm : Form
{
    static SaveSession OpenSpecificSave(string path, ulong accountId)
    {
        byte[] enc=File.ReadAllBytes(path);
        if((enc.Length%8)!=0)throw new InvalidDataException("Encrypted save length is not a Blowfish block multiple.");

        ulong steamId64=SteamIdBase+accountId;
        byte[] key=CCSaveCrypto.BuildKey(steamId64.ToString(CultureInfo.InvariantCulture));
        CCSaveCrypto bf=new CCSaveCrypto(key);
        byte[] plain=bf.Decrypt(enc);

        if(plain.Length<0x40+CharacterRecordCount*0x30+4)
            throw new InvalidDataException("The decrypted file is shorter than the expected Castle Crashers save structure.");

        uint stored=CCSaveCrypto.ReadU32LE(plain,plain.Length-4);
        uint calc=CCSaveCrypto.Checksum(plain,plain.Length-4);
        if(stored!=calc)
            throw new InvalidDataException("Checksum validation failed after decryption. The save may belong to a different Steam account, be damaged, or use an unsupported save format.");

        if(!CCSaveCrypto.BasicPlausibility(plain))
            throw new InvalidDataException("The decrypted save failed the Castle Crashers structure check.");

        SaveSession ss=new SaveSession();
        ss.Path=path;
        ss.AccountId=accountId;
        ss.SteamId64=steamId64;
        ss.Plain=plain;
        ss.Crypto=bf;
        ss.Modified=File.GetLastWriteTime(path);
        return ss;
    }

    static SaveSession SelectAndOpenSave()
    {
        using(OpenFileDialog dialog=new OpenFileDialog())
        {
            dialog.Title="Select Castle Crashers cc_save.dat";
            dialog.Filter="Castle Crashers save (cc_save.dat)|cc_save.dat|All files (*.*)|*.*";
            dialog.FileName="cc_save.dat";
            dialog.CheckFileExists=true;
            dialog.Multiselect=false;

            if(dialog.ShowDialog()!=DialogResult.OK)return null;

            string path=Path.GetFullPath(dialog.FileName);
            ulong accountId;
            string pathProblem;
            if(!TryGetSteamAccountIdFromPath(path,out accountId,out pathProblem))
            {
                Log("Save selection rejected: "+path+" :: "+pathProblem);
                throw new InvalidOperationException(
                    pathProblem+
                    "\n\nSelect the original save at:\nSteam\\userdata\\<account>\\204360\\remote\\cc_save.dat"+
                    "\n\nCrasher Editor V1.3 no longer scans the registry, Steam accounts, or running processes.");
            }

            Log("Selected save: "+path);
            Log("Steam account ID inferred from path: "+accountId.ToString(CultureInfo.InvariantCulture));

            try
            {
                return OpenSpecificSave(path,accountId);
            }
            catch(Exception ex)
            {
                Log("Save open failed: "+path+" :: "+ex.ToString());
                throw new InvalidDataException(
                    "Crasher Editor could not validate this save.\n\n"+
                    ex.Message+
                    "\n\nAccount folder: "+accountId.ToString(CultureInfo.InvariantCulture)+
                    "\nLog: "+Path.Combine(Path.GetTempPath(),"Crasher_Editor_V1.3.log"),ex);
            }
        }
    }

    static bool TryGetSteamAccountIdFromPath(string path,out ulong accountId,out string problem)
    {
        accountId=0;
        problem=null;

        if(!String.Equals(Path.GetFileName(path),"cc_save.dat",StringComparison.OrdinalIgnoreCase))
        {
            problem="The selected file is not named cc_save.dat.";
            return false;
        }

        DirectoryInfo remote=Directory.GetParent(path);
        DirectoryInfo app=remote==null?null:remote.Parent;
        DirectoryInfo account=app==null?null:app.Parent;
        DirectoryInfo userdata=account==null?null:account.Parent;

        if(remote==null||!String.Equals(remote.Name,"remote",StringComparison.OrdinalIgnoreCase)||
           app==null||!String.Equals(app.Name,AppId.ToString(CultureInfo.InvariantCulture),StringComparison.Ordinal)||
           account==null||!UInt64.TryParse(account.Name,NumberStyles.None,CultureInfo.InvariantCulture,out accountId)||
           userdata==null||!String.Equals(userdata.Name,"userdata",StringComparison.OrdinalIgnoreCase))
        {
            accountId=0;
            problem="The editor could not infer the Steam account from the selected file path.";
            return false;
        }

        return true;
    }
}
