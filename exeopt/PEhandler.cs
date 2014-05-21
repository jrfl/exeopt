// PEhandler.cs
//
// This file is based on a microsoft c++ example from the PE format documentation
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.

using System;
using System.IO;


namespace Patcher {
    public class PEfile {

        public FileInfo file;
        public BinaryReader stream;
        public DosHeader dosHeader;
        public ImageHeader imageHeader;
        public ImageOptionalHeader imageOptionalHeader;
        public ImageSectionHeader[] imageSectionHeaders;

        public uint CodeOffset;
        public uint CodeSize;

        public PEfile(string path) {
            if(!File.Exists(path)) throw new Exception("File does not exist");
            file=new FileInfo(path);
            stream=new BinaryReader(file.OpenRead());
            dosHeader=new DosHeader(stream);
            if(dosHeader.Magic!="MZ") throw new Exception("File does not appear to be executable");
            imageHeader=new ImageHeader(stream,dosHeader.ImageHeaderAddress);
            if(imageHeader.Magic!="PE\0\0") throw new Exception("File does not appear to be a WIN32 exe");
            imageOptionalHeader=new ImageOptionalHeader(stream);
            imageSectionHeaders=new ImageSectionHeader[imageHeader.NumberOfSections];
            for(int x=0;x<imageHeader.NumberOfSections;x++) {
                imageSectionHeaders[x] = new ImageSectionHeader(stream);
            }
            CodeOffset=imageOptionalHeader.BaseOfCode;
            CodeSize=imageOptionalHeader.SizeOfCode;
            stream.Close();
        }

        public PEfile(uint offset,uint length) {
            CodeOffset=offset;
            CodeSize=length;
        }
    }

    public struct DosHeader {
        public const int size=64;

        public string Magic;         // Magic number (should be MZ)
        public ushort cblp;          // Bytes on last page of file
        public ushort cp;            // Pages in file
        public ushort crlc;          // Relocations
        public ushort cparhdr;       // Size of header in paragraphs
        public ushort minalloc;      // Minimum extra paragraphs needed
        public ushort maxalloc;      // Maximum extra paragraphs needed
        public ushort ss;            // Initial (relative) SS value
        public ushort sp;            // Initial SP value
        public ushort csum;          // Checksum
        public ushort ip;            // Initial IP value
        public ushort cs;            // Initial (relative) CS value
        public ushort lfarlc;        // File address of relocation table
        public ushort ovno;          // Overlay number
        public ushort[] res;         // Reserved words [4]
        public ushort oemid;         // OEM identifier (for e_oeminfo)
        public ushort oeminfo;       // OEM information; e_oemid specific
        public ushort[] res2;        // Reserved words [10]
        public int ImageHeaderAddress;          // File address of new exe header

        public DosHeader(BinaryReader input) : this(input,-1) {}
        public DosHeader(BinaryReader input,long offset) {
            if(offset!=-1) input.BaseStream.Seek(offset,SeekOrigin.Begin);
            Magic="";
            for(int x=0;x<2;x++) {
                Magic+=input.ReadChar();
            }
            cblp=input.ReadUInt16();
            cp=input.ReadUInt16();
            crlc=input.ReadUInt16();
            cparhdr=input.ReadUInt16();
            minalloc=input.ReadUInt16();
            maxalloc=input.ReadUInt16();
            ss=input.ReadUInt16();
            sp=input.ReadUInt16();
            csum=input.ReadUInt16();
            ip=input.ReadUInt16();
            cs=input.ReadUInt16();
            lfarlc=input.ReadUInt16();
            ovno=input.ReadUInt16();
            res=new ushort[4];
            for(int x=0;x<4;x++) {
                res[x]=input.ReadUInt16();
            }
            oemid=input.ReadUInt16();
            oeminfo=input.ReadUInt16();
            res2=new ushort[10];
            for(int x=0;x<10;x++) {
                res2[x]=input.ReadUInt16();
            }
            ImageHeaderAddress=input.ReadInt32();
        }
    }

    public struct ImageHeader {
        public const int size=24;

        public string Magic;            // Magic number (Should be PE\0\0)
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;

        public ImageHeader(BinaryReader input) :this(input,-1) { }
        public ImageHeader(BinaryReader input,long offset) {
            if(offset!=-1) input.BaseStream.Seek(offset,SeekOrigin.Begin);
            Magic="";
            for(int x=0;x<4;x++) {
                Magic+=input.ReadChar();
            }
            Machine=input.ReadUInt16();
            NumberOfSections=input.ReadUInt16();
            TimeDateStamp=input.ReadUInt32();
            PointerToSymbolTable=input.ReadUInt32();
            NumberOfSymbols=input.ReadUInt32();
            SizeOfOptionalHeader=input.ReadUInt16();
            Characteristics=input.ReadUInt16();
        }
    }

    public struct ImageDataDirectory {
        public uint VirtualAddress;
        public uint Size;

        public ImageDataDirectory(uint a,uint b) {
            VirtualAddress=a;
            Size=b;
        }
    }

    public struct ImageOptionalHeader {
        public const int size=224;

        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;            //Must add ImageBase to this!
        public uint BaseOfCode;
        public uint BaseOfData;
        public uint ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Reserved1;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public ImageDataDirectory[] DataDirectories;

        public ImageOptionalHeader(BinaryReader input) :this(input,-1) { }
        public ImageOptionalHeader(BinaryReader input,long offset) {
            if(offset!=-1) input.BaseStream.Seek(offset,SeekOrigin.Begin);
            Magic=input.ReadUInt16();
            MajorLinkerVersion=input.ReadByte();
            MinorLinkerVersion=input.ReadByte();
            SizeOfCode=input.ReadUInt32();
            SizeOfInitializedData=input.ReadUInt32();
            SizeOfUninitializedData=input.ReadUInt32();
            AddressOfEntryPoint=input.ReadUInt32();
            BaseOfCode=input.ReadUInt32();
            BaseOfData=input.ReadUInt32();
            ImageBase=input.ReadUInt32();
            SectionAlignment=input.ReadUInt32();
            FileAlignment=input.ReadUInt32();
            MajorOperatingSystemVersion=input.ReadUInt16();
            MinorOperatingSystemVersion=input.ReadUInt16();
            MajorImageVersion=input.ReadUInt16();
            MinorImageVersion=input.ReadUInt16();
            MajorSubsystemVersion=input.ReadUInt16();
            MinorSubsystemVersion=input.ReadUInt16();
            Reserved1=input.ReadUInt32();
            SizeOfImage=input.ReadUInt32();
            SizeOfHeaders=input.ReadUInt32();
            CheckSum=input.ReadUInt32();
            Subsystem=input.ReadUInt16();
            DllCharacteristics=input.ReadUInt16();
            SizeOfStackReserve=input.ReadUInt32();
            SizeOfStackCommit=input.ReadUInt32();
            SizeOfHeapReserve=input.ReadUInt32();
            SizeOfHeapCommit=input.ReadUInt32();
            LoaderFlags=input.ReadUInt32();
            NumberOfRvaAndSizes=input.ReadUInt32();
            DataDirectories=new ImageDataDirectory[16];
            for(int x=0;x<NumberOfRvaAndSizes;x++) {
                uint a=input.ReadUInt32();
                uint b=input.ReadUInt32();
                DataDirectories[x]=new ImageDataDirectory(a,b);
            }
        }
    }

    public struct ImageSectionHeader {
        public const int size=40;

        public string Name;
        public uint PhysicalAddress;
        public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;

        public ImageSectionHeader(BinaryReader input) :this(input,-1) { }
        public ImageSectionHeader(BinaryReader input,long offset) {
            if(offset!=-1) input.BaseStream.Seek(offset,SeekOrigin.Begin);
            Name="";
            for(int x=0;x<8;x++) {
                Name+=input.ReadChar();
            }
            PhysicalAddress=input.ReadUInt32();
            VirtualSize=input.ReadUInt32();
            VirtualAddress=VirtualSize;
            //VirtualAddress=input.ReadUInt32();
            SizeOfRawData=input.ReadUInt32();
            PointerToRawData=input.ReadUInt32();
            PointerToRelocations=input.ReadUInt32();
            PointerToLinenumbers=input.ReadUInt32();
            NumberOfRelocations=input.ReadUInt16();
            NumberOfLinenumbers=input.ReadUInt16();
            Characteristics=input.ReadUInt32();
        }


    }

}
