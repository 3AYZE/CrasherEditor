
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public sealed partial class CCSaveCrypto
{

static readonly uint[] S3 = new uint[] {
        0x3A39CE37u, 0xD3FAF5CFu, 0xABC27737u, 0x5AC52D1Bu, 0x5CB0679Eu, 0x4FA33742u, 0xD3822740u, 0x99BC9BBEu,
        0xD5118E9Du, 0xBF0F7315u, 0xD62D1C7Eu, 0xC700C47Bu, 0xB78C1B6Bu, 0x21A19045u, 0xB26EB1BEu, 0x6A366EB4u,
        0x5748AB2Fu, 0xBC946E79u, 0xC6A376D2u, 0x6549C2C8u, 0x530FF8EEu, 0x468DDE7Du, 0xD5730A1Du, 0x4CD04DC6u,
        0x2939BBDBu, 0xA9BA4650u, 0xAC9526E8u, 0xBE5EE304u, 0xA1FAD5F0u, 0x6A2D519Au, 0x63EF8CE2u, 0x9A86EE22u,
        0xC089C2B8u, 0x43242EF6u, 0xA51E03AAu, 0x9CF2D0A4u, 0x83C061BAu, 0x9BE96A4Du, 0x8FE51550u, 0xBA645BD6u,
        0x2826A2F9u, 0xA73A3AE1u, 0x4BA99586u, 0xEF5562E9u, 0xC72FEFD3u, 0xF752F7DAu, 0x3F046F69u, 0x77FA0A59u,
        0x80E4A915u, 0x87B08601u, 0x9B09E6ADu, 0x3B3EE593u, 0xE990FD5Au, 0x9E34D797u, 0x2CF0B7D9u, 0x022B8B51u,
        0x96D5AC3Au, 0x017DA67Du, 0xD1CF3ED6u, 0x7C7D2D28u, 0x1F9F25CFu, 0xADF2B89Bu, 0x5AD6B472u, 0x5A88F54Cu,
        0xE029AC71u, 0xE019A5E6u, 0x47B0ACFDu, 0xED93FA9Bu, 0xE8D3C48Du, 0x283B57CCu, 0xF8D56629u, 0x79132E28u,
        0x785F0191u, 0xED756055u, 0xF7960E44u, 0xE3D35E8Cu, 0x15056DD4u, 0x88F46DBAu, 0x03A16125u, 0x0564F0BDu,
        0xC3EB9E15u, 0x3C9057A2u, 0x97271AECu, 0xA93A072Au, 0x1B3F6D9Bu, 0x1E6321F5u, 0xF59C66FBu, 0x26DCF319u,
        0x7533D928u, 0xB155FDF5u, 0x03563482u, 0x8ABA3CBBu, 0x28517711u, 0xC20AD9F8u, 0xABCC5167u, 0xCCAD925Fu,
        0x4DE81751u, 0x3830DC8Eu, 0x379D5862u, 0x9320F991u, 0xEA7A90C2u, 0xFB3E7BCEu, 0x5121CE64u, 0x774FBE32u,
        0xA8B6E37Eu, 0xC3293D46u, 0x48DE5369u, 0x6413E680u, 0xA2AE0810u, 0xDD6DB224u, 0x69852DFDu, 0x09072166u,
        0xB39A460Au, 0x6445C0DDu, 0x586CDECFu, 0x1C20C8AEu, 0x5BBEF7DDu, 0x1B588D40u, 0xCCD2017Fu, 0x6BB4E3BBu,
        0xDDA26A7Eu, 0x3A59FF45u, 0x3E350A44u, 0xBCB4CDD5u, 0x72EACEA8u, 0xFA6484BBu, 0x8D6612AEu, 0xBF3C6F47u,
        0xD29BE463u, 0x542F5D9Eu, 0xAEC2771Bu, 0xF64E6370u, 0x740E0D8Du, 0xE75B1357u, 0xF8721671u, 0xAF537D5Du,
        0x4040CB08u, 0x4EB4E2CCu, 0x34D2466Au, 0x0115AF84u, 0xE1B00428u, 0x95983A1Du, 0x06B89FB4u, 0xCE6EA048u,
        0x6F3F3B82u, 0x3520AB82u, 0x011A1D4Bu, 0x277227F8u, 0x611560B1u, 0xE7933FDCu, 0xBB3A792Bu, 0x344525BDu,
        0xA08839E1u, 0x51CE794Bu, 0x2F32C9B7u, 0xA01FBAC9u, 0xE01CC87Eu, 0xBCC7D1F6u, 0xCF0111C3u, 0xA1E8AAC7u,
        0x1A908749u, 0xD44FBD9Au, 0xD0DADECBu, 0xD50ADA38u, 0x0339C32Au, 0xC6913667u, 0x8DF9317Cu, 0xE0B12B4Fu,
        0xF79E59B7u, 0x43F5BB3Au, 0xF2D519FFu, 0x27D9459Cu, 0xBF97222Cu, 0x15E6FC2Au, 0x0F91FC71u, 0x9B941525u,
        0xFAE59361u, 0xCEB69CEBu, 0xC2A86459u, 0x12BAA8D1u, 0xB6C1075Eu, 0xE3056A0Cu, 0x10D25065u, 0xCB03A442u,
        0xE0EC6E0Eu, 0x1698DB3Bu, 0x4C98A0BEu, 0x3278E964u, 0x9F1F9532u, 0xE0D392DFu, 0xD3A0342Bu, 0x8971F21Eu,
        0x1B0A7441u, 0x4BA3348Cu, 0xC5BE7120u, 0xC37632D8u, 0xDF359F8Du, 0x9B992F2Eu, 0xE60B6F47u, 0x0FE3F11Du,
        0xE54CDA54u, 0x1EDAD891u, 0xCE6279CFu, 0xCD3E7E6Fu, 0x1618B166u, 0xFD2C1D05u, 0x848FD2C5u, 0xF6FB2299u,
        0xF523F357u, 0xA6327623u, 0x93A83531u, 0x56CCCD02u, 0xACF08162u, 0x5A75EBB5u, 0x6E163697u, 0x88D273CCu,
        0xDE966292u, 0x81B949D0u, 0x4C50901Bu, 0x71C65614u, 0xE6C6C7BDu, 0x327A140Au, 0x45E1D006u, 0xC3F27B9Au,
        0xC9AA53FDu, 0x62A80F00u, 0xBB25BFE2u, 0x35BDD2F6u, 0x71126905u, 0xB2040222u, 0xB6CBCF7Cu, 0xCD769C2Bu,
        0x53113EC0u, 0x1640E3D3u, 0x38ABBD60u, 0x2547ADF0u, 0xBA38209Cu, 0xF746CE76u, 0x77AFA1C5u, 0x20756060u,
        0x85CBFE4Eu, 0x8AE88DD8u, 0x7AAAF9B0u, 0x4CF9AA7Eu, 0x1948C25Cu, 0x02FB8A8Cu, 0x01C36AE4u, 0xD6EBE1F9u,
        0x90D4F869u, 0xA65CDEA0u, 0x3F09252Du, 0xC208E69Fu, 0xB74E6132u, 0xCE77E25Bu, 0x578FDFE3u, 0x3AC372E6u
    };

    uint[] P;
    uint[][] S;

    public CCSaveCrypto(byte[] key)
    {
        if (key == null || key.Length < 4) throw new ArgumentException("Invalid Blowfish key.");

        P = (uint[])P0.Clone();
        S = new uint[][] {
            (uint[])S0.Clone(),
            (uint[])S1.Clone(),
            (uint[])S2.Clone(),
            (uint[])S3.Clone()
        };

        unchecked {
            for (int i=0; i<18; i++) {
                uint d=0;
                for (int j=0; j<4; j++)
                    d=(d<<8) | key[(i*4+j)%key.Length];
                P[i] ^= d;
            }

            uint L=0, R=0;
            for (int i=0; i<18; i+=2) {
                EncryptBlock(ref L,ref R);
                P[i]=L; P[i+1]=R;
            }
            for (int s=0; s<4; s++) {
                for (int j=0; j<256; j+=2) {
                    EncryptBlock(ref L,ref R);
                    S[s][j]=L; S[s][j+1]=R;
                }
            }
        }
    }

    uint F(uint x)
    {
        unchecked {
            int a=(int)((x>>24)&0xFF);
            int b=(int)((x>>16)&0xFF);
            int c=(int)((x>>8)&0xFF);
            int d=(int)(x&0xFF);
            return (((S[0][a]+S[1][b]) ^ S[2][c]) + S[3][d]);
        }
    }

    void EncryptBlock(ref uint L,ref uint R)
    {
        unchecked {
            for (int i=0;i<16;i++) {
                L ^= P[i];
                R ^= F(L);
                uint t=L;L=R;R=t;
            }
            { uint t=L;L=R;R=t; }
            R ^= P[16];
            L ^= P[17];
        }
    }

    void DecryptBlock(ref uint L,ref uint R)
    {
        unchecked {
            for (int i=17;i>1;i--) {
                L ^= P[i];
                R ^= F(L);
                uint t=L;L=R;R=t;
            }
            { uint t=L;L=R;R=t; }
            R ^= P[1];
            L ^= P[0];
        }
    }

    static uint ReadU32LEInternal(byte[] b,int o)
    {
        return unchecked((uint)(
            b[o] |
            (b[o+1]<<8) |
            (b[o+2]<<16) |
            (b[o+3]<<24)));
    }

    static void WriteU32LEInternal(byte[] b,int o,uint v)
    {
        b[o]=(byte)v;
        b[o+1]=(byte)(v>>8);
        b[o+2]=(byte)(v>>16);
        b[o+3]=(byte)(v>>24);
    }

    public byte[] Decrypt(byte[] input)
    {
        if (input==null || input.Length%8!=0)
            throw new ArgumentException("Save length must be a multiple of 8.");

        byte[] output=new byte[input.Length];
        for (int i=0;i<input.Length;i+=8) {
            uint L=ReadU32LEInternal(input,i);
            uint R=ReadU32LEInternal(input,i+4);
            DecryptBlock(ref L,ref R);
            WriteU32LEInternal(output,i,L);
            WriteU32LEInternal(output,i+4,R);
        }
        return output;
    }

    public byte[] Encrypt(byte[] input)
    {
        if (input==null || input.Length%8!=0)
            throw new ArgumentException("Save length must be a multiple of 8.");

        byte[] output=new byte[input.Length];
        for (int i=0;i<input.Length;i+=8) {
            uint L=ReadU32LEInternal(input,i);
            uint R=ReadU32LEInternal(input,i+4);
            EncryptBlock(ref L,ref R);
            WriteU32LEInternal(output,i,L);
            WriteU32LEInternal(output,i+4,R);
        }
        return output;
    }

    public static byte[] BuildKey(string steamId64Text)
    {
        ulong sid;
        if (!UInt64.TryParse(steamId64Text,out sid))
            throw new ArgumentException("Invalid SteamID64.");

        uint lo=(uint)(sid&0xFFFFFFFFUL);
        uint hi=(uint)(sid>>32);

        byte[] key=new byte[24];
        WriteU32LEInternal(key,0,0xB9128343u);
        WriteU32LEInternal(key,4,0x7E636609u);
        WriteU32LEInternal(key,8,0x127AAC36u);
        WriteU32LEInternal(key,12,0x563E6167u);

        ulong q=0x019FD45372723839UL;
        WriteU32LEInternal(key,16,(uint)(q&0xFFFFFFFFUL));
        WriteU32LEInternal(key,20,(uint)(q>>32));

        WriteU16LE(key,0,(ushort)((hi>>16)&0xFFFF));
        WriteU16LE(key,5,(ushort)((lo>>16)&0xFFFF));
        WriteU16LE(key,15,(ushort)(lo&0xFFFF));
        WriteU16LE(key,19,(ushort)(hi&0xFFFF));

        return key;
    }

    static void WriteU16LE(byte[] b,int o,ushort v)
    {
        b[o]=(byte)v;
        b[o+1]=(byte)(v>>8);
    }

    public static uint Checksum(byte[] data,int length)
    {
        unchecked {
            uint local8=0, localC=0;
            uint state=0xD971u;
            int i=0;

            while (i+1<length) {
                uint b5=((state>>8)^data[i])&0xFFu;
                uint s2=((((b5+state)&0xFFFFu)*0xCE6Du)+0x58BFu)&0xFFFFu;
                local8+=b5;

                uint u1=((s2>>8)^data[i+1])&0xFFu;
                state=((((u1+s2)&0xFFFFu)*0xCE6Du)+0x58BFu)&0xFFFFu;
                localC+=u1;
                i+=2;
            }

            uint last=0;
            if (i<length)
                last=((state>>8)^data[i])&0xFFu;

            return local8+localC+last;
        }
    }

    public static uint ReadU32LE(byte[] b,int o) { return ReadU32LEInternal(b,o); }
    public static void WriteU32LE(byte[] b,int o,uint v) { WriteU32LEInternal(b,o,v); }

    public static int ReadI32BE(byte[] b,int o)
    {
        uint u=unchecked(
            ((uint)b[o]<<24) |
            ((uint)b[o+1]<<16) |
            ((uint)b[o+2]<<8) |
            b[o+3]);
        return unchecked((int)u);
    }

    public static void WriteI32BE(byte[] b,int o,int value)
    {
        uint u=unchecked((uint)value);
        b[o]=(byte)(u>>24);
        b[o+1]=(byte)(u>>16);
        b[o+2]=(byte)(u>>8);
        b[o+3]=(byte)u;
    }

    public static bool BasicPlausibility(byte[] plain)
    {
        if (plain==null || plain.Length<0x850) return false;

        int baseOff=0x40;
        for (int i=0;i<4;i++) {
            int o=baseOff+i*0x30;
            if (plain[o]!=0x80) return false;

            int xp=ReadI32BE(plain,o+0x02);
            int gold=ReadI32BE(plain,o+0x13);
            if (xp<0 || xp>100000000) return false;
            if (gold<0 || gold>100000000) return false;

            for (int k=0x08;k<=0x0B;k++)
                if (plain[o+k]>25) return false;
        }
        return true;
    }
}
