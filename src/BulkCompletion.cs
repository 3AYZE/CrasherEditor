using System;
using System.IO;

public sealed class BulkCompletionOptions
{
    public bool IncludeWorkshopSlots = true;
    public bool MaxLevelAndStats = true;
    public bool MaxConsumables = true;
    public bool GiveGold = true;
    public bool CompleteNormalAndInsane = true;
    public bool UnlockCollections = true;
}

public static class BulkCompletion
{
    public const int OfficialCharacters = 32;
    public const int TotalCharacters = 42;
    public const int RecordSize = 0x30;
    public const int RecordStart = 0x40;
    public const int Level99Xp = 113680;
    public const int GoldTarget = 999999;

    public static int Apply(byte[] data, BulkCompletionOptions options)
    {
        if(data==null)throw new ArgumentNullException("data");
        if(options==null)throw new ArgumentNullException("options");
        if(data.Length<RecordStart+TotalCharacters*RecordSize+4)
            throw new InvalidDataException("Castle Crashers save does not contain all expected character records.");

        if(!options.MaxLevelAndStats && !options.MaxConsumables && !options.GiveGold &&
           !options.CompleteNormalAndInsane && !options.UnlockCollections)
            throw new ArgumentException("Choose at least one bulk edit.");

        int count=options.IncludeWorkshopSlots?TotalCharacters:OfficialCharacters;
        int changedCharacters=0;

        for(int i=0;i<count;i++)
        {
            int o=RecordStart+i*RecordSize;
            byte[] before=new byte[RecordSize];
            Buffer.BlockCopy(data,o,before,0,RecordSize);

            if(options.MaxLevelAndStats)
            {
                data[o+0x01]=98;
                CCSaveCrypto.WriteI32BE(data,o+0x02,Level99Xp);
                data[o+0x08]=25;
                data[o+0x09]=25;
                data[o+0x0A]=25;
                data[o+0x0B]=25;
            }
            if(options.MaxConsumables)
            {
                data[o+0x0F]=9;
                data[o+0x10]=9;
                data[o+0x11]=9;
            }
            if(options.GiveGold && CCSaveCrypto.ReadI32BE(data,o+0x13)<GoldTarget)
                CCSaveCrypto.WriteI32BE(data,o+0x13,GoldTarget);

            if(options.CompleteNormalAndInsane)
            {
                data[o+0x0C]=0xFF;
                data[o+0x0D]=0xFF;
                data[o+0x0E]=0x01;
                data[o+0x17]=0x01;
                data[o+0x18]=0xFF;
                data[o+0x19]=0xFF;
                data[o+0x1A]=0x01;
                data[o+0x12]=0xFF;
                data[o+0x1B]=0xFF;
            }

            if(options.UnlockCollections && i>=4 && i<28)
                data[o]=0x80;

            bool changed=false;
            for(int j=0;j<RecordSize;j++)
                if(before[j]!=data[o+j]) { changed=true; break; }
            if(changed)changedCharacters++;
        }

        if(options.UnlockCollections)
        {
            data[8]=(byte)(data[8]|0xFE);
            for(int i=9;i<=11;i++)data[i]=0xFF;
            data[12]=(byte)(data[12]|0x4E);
            for(int i=18;i<=30;i++)data[i]=0xFF;
        }

        return changedCharacters;
    }
}
