using System;
using System.IO;
using System.Text;
using System.Globalization;

public static class WorkshopSlotLabels
{
    static string NamePath(ulong accountId)
    {
        return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CrasherEditor"),"workshop_"+accountId.ToString(CultureInfo.InvariantCulture)+".txt");
    }
    public static string Get(ulong accountId,int slot)
    {
        if(slot<0||slot>=10)return String.Empty;
        try
        {
            string path=NamePath(accountId);
            if(!File.Exists(path))return String.Empty;
            string[] lines=File.ReadAllLines(path,Encoding.UTF8);
            return slot<lines.Length?lines[slot]:String.Empty;
        }
        catch(IOException){return String.Empty;}
        catch(UnauthorizedAccessException){return String.Empty;}
    }
    public static void Set(ulong accountId,int slot,string name)
    {
        if(slot<0||slot>=10)throw new ArgumentOutOfRangeException("slot");
        name=(name??String.Empty).Trim();
        if(name.Length>50||name.IndexOfAny(new char[]{'\r','\n','\t'})>=0)
            throw new ArgumentException("Invalid slot label.");
        string path=NamePath(accountId);Directory.CreateDirectory(Path.GetDirectoryName(path));
        string[] values=new string[10];
        if(File.Exists(path))
        {
            string[] prior=File.ReadAllLines(path,Encoding.UTF8);
            Array.Copy(prior,values,Math.Min(prior.Length,10));
        }
        values[slot]=name;
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            File.WriteAllLines(temp,values,new UTF8Encoding(false));
            if(File.Exists(path))File.Replace(temp,path,null);
            else File.Move(temp,path);
        }
        finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
