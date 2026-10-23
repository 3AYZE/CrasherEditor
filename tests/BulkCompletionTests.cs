using System;
using System.IO;

static class BulkCompletionTests
{
    static int tests;
    static void Check(bool condition,string message)
    {
        tests++;
        if(!condition)throw new Exception(message);
    }
    static int Record(int slot) { return 0x40+slot*0x30; }
    static bool Equal(byte[] a,byte[] b)
    {
        if(a.Length!=b.Length)return false;
        for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;
        return true;
    }
    static void VerifyFull()
    {
        byte[] data=new byte[0x850];
        data[Record(0)+6]=66;
        data[Record(0)+7]=18;
        data[Record(41)]=0;
        CCSaveCrypto.WriteI32BE(data,Record(41)+0x13,5000000);
        data[0x824]=0xAB;
        int changed=BulkCompletion.Apply(data,new BulkCompletionOptions());
        Check(changed==42,"All 42 slots should change.");
        for(int i=0;i<42;i++)
        {
            int o=Record(i);
            Check(data[o+1]==98,"Level must be 99.");
            Check(CCSaveCrypto.ReadI32BE(data,o+2)==BulkCompletion.Level99Xp,"XP must match Level 99.");
            for(int j=8;j<=11;j++)Check(data[o+j]==25,"All four stats capped at 25.");
            for(int j=15;j<=17;j++)Check(data[o+j]==9,"All consumables maxed.");
            Check(CCSaveCrypto.ReadI32BE(data,o+0x13)>=BulkCompletion.GoldTarget,"Gold cannot decrease.");
            Check(data[o+0x0C]==255&&data[o+0x0D]==255&&data[o+0x0E]==1,"Normal progress complete.");
            Check(data[o+0x18]==255&&data[o+0x19]==255&&data[o+0x1A]==1,"Insane progress complete.");
            Check(data[o+0x12]==255&&data[o+0x17]==1&&data[o+0x1B]==255,"Skulls/relics complete.");
            if(i>=4&&i<28)Check(data[o]==0x80,"Unlock official slots.");
        }
        Check(data[Record(0)+6]==66&&data[Record(0)+7]==18,"Do not change equipped gear.");
        Check(data[Record(41)]==0,"Do not change custom-slot assignment/unlock byte.");
        Check(CCSaveCrypto.ReadI32BE(data,Record(41)+0x13)==5000000,"Preserve larger gold balance.");
        Check(data[0x824]==0xAB,"Preserve unrelated trailing save data.");
        Check(data[8]==0xFE&&data[9]==255&&data[12]==0x4E&&data[30]==255,"Profile unlock bytes.");
        byte[] snapshot=(byte[])data.Clone();
        Check(BulkCompletion.Apply(data,new BulkCompletionOptions())==0,"Repeat apply should change no records.");
        Check(Equal(data,snapshot),"Repeat apply should be idempotent.");
    }
    static void VerifyOfficialOnly()
    {
        byte[] data=new byte[0x850];
        for(int i=0;i<data.Length;i++)data[i]=(byte)(i%131);
        byte[] before=(byte[])data.Clone();
        BulkCompletionOptions options=new BulkCompletionOptions();
        options.IncludeWorkshopSlots=false;
        BulkCompletion.Apply(data,options);
        for(int i=32;i<42;i++)
            for(int j=0;j<0x30;j++)Check(data[Record(i)+j]==before[Record(i)+j],"Official-only must preserve Workshop slots.");
    }
    static void VerifySelective()
    {
        byte[] data=new byte[0x850];
        byte[] before=(byte[])data.Clone();
        BulkCompletionOptions options=new BulkCompletionOptions();
        options.MaxLevelAndStats=false;options.GiveGold=false;options.MaxConsumables=false;options.UnlockCollections=false;
        BulkCompletion.Apply(data,options);
        Check(data[Record(0)+0x0C]==255&&data[Record(0)+0x18]==255,"Progress-only option applies.");
        Check(data[Record(0)+1]==before[Record(0)+1],"Progress-only preserves level.");
        Check(data[8]==before[8],"Progress-only preserves profile unlocks.");
        Check(data[Record(0)+8]==before[Record(0)+8],"Progress-only preserves skills.");
        Check(CCSaveCrypto.ReadI32BE(data,Record(0)+0x13)==0,"Progress-only preserves gold.");
    }
    static void VerifyGuards()
    {
        bool threw=false;
        try {BulkCompletion.Apply(new byte[10],new BulkCompletionOptions());}catch(InvalidDataException){threw=true;}
        Check(threw,"Reject undersized saves.");
        threw=false;
        BulkCompletionOptions options=new BulkCompletionOptions();
        options.MaxLevelAndStats=options.MaxConsumables=options.GiveGold=options.CompleteNormalAndInsane=options.UnlockCollections=false;
        try {BulkCompletion.Apply(new byte[0x850],options);}catch(ArgumentException){threw=true;}
        Check(threw,"Reject empty selection.");
    }
    static int Main()
    {
        try
        {
            VerifyFull();VerifyOfficialOnly();VerifySelective();VerifyGuards();
            Console.WriteLine("BulkCompletionTests: "+tests+" checks PASSED");
            return 0;
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }
}
