import base64
import binascii
import os
import struct
import zlib

PALETTE=[[255,255,255],[253,255,255],[242,239,241],[224,221,225],[218,217,223],[217,216,222],[232,202,202],[204,194,199],[220,159,159],[169,147,155],[208,130,131],[169,136,143],[155,127,134],[193,90,93],[157,43,32],[141,33,17],[133,33,17],[229,5,4],[153,13,9],[98,98,119],[58,55,57],[89,39,40],[82,11,9],[5,4,4]]
PIXELS="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"
SIDE=64

def chunk(kind, data):
    return struct.pack(">I",len(data))+kind+data+struct.pack(">I",binascii.crc32(kind+data)&0xffffffff)

def png(size, pixels):
    scan=bytearray()
    for y in range(size):
        scan.append(0)
        for x in range(size):
            ix=(y*SIDE//size)*SIDE+(x*SIDE//size)
            scan.extend(PALETTE[pixels[ix]])
            scan.append(255)
    raw=zlib.compress(bytes(scan),9)
    return (b"\x89PNG\r\n\x1a\n"+chunk(b"IHDR",struct.pack(">IIBBBBB",size,size,8,6,0,0,0))
            +chunk(b"IDAT",raw)+chunk(b"IEND",b""))

def main():
    pixels=zlib.decompress(base64.b64decode(PIXELS))
    if len(pixels)!=SIDE*SIDE:
        raise RuntimeError("Invalid embedded artwork")
    sizes=(16,24,32,48,64,128,256)
    frames=[png(n,pixels) for n in sizes]
    output=bytearray(struct.pack("<HHH",0,1,len(frames)))
    offset=6+16*len(frames)
    for n,frame in zip(sizes,frames):
        output.extend(struct.pack("<BBBBHHII",n%256,n%256,0,0,1,32,len(frame),offset))
        offset+=len(frame)
    for frame in frames:
        output.extend(frame)
    os.makedirs("assets",exist_ok=True)
    with open(os.path.join("assets","CrasherEditor.ico"),"wb") as f:
        f.write(output)
    print("Generated 7-frame CrasherEditor.ico from the supplied artwork")

if __name__=="__main__":
    main()
