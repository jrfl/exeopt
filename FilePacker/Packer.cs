// Packer.cs
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
using ICSharpCode.SharpZipLib.BZip2;

namespace FilePacker {
	/// <summary>
	/// Contains methods to generate a c# source file containing bz2 compressed embedded files
	/// </summary>
	/// <remarks>
	///  This produces massive source files! Do not add using directives to the FileData 
	///  namespace, or try to edit files.cs in the code editor. 
	///  Would be far more useful if it searched for files instead of having to hard code them
	/// </remarks>
    public class Packer {
		/// <summary>
		/// Take an input file and compress the whole thing into a stream
		/// </summary>
		/// <remarks>
		/// This method was lifted from OblivionModManager. 
		/// In this case it makes no sense to take the second stream as a parameter: 
		/// It should create a MemoryStream in the method then return it.
		/// </remarks>
        private static void Compress(string InFile,Stream OutFile) {
            int blocksize=9*BZip2Constants.baseBlockSize;
            FileStream infile=File.OpenRead(InFile);
            BZip2.Compress(infile,OutFile,blocksize);
            infile.Close();
        }

		/// <summary>
		/// Application entry point
		/// </summary>
		/// <remarks>
		/// More or less the entire program really. 
		/// Should split into smaller functions, and possibly take command line args. 
		/// Args could set variable compression level or something.
		/// </remarks>
		/// <param name="args">Command line arguments. Ignored completely.</param>
        public static void Main(string[] args) {
            MemoryStream Out=new MemoryStream();
            BinaryWriter bw=new BinaryWriter(Out);
            MemoryStream ms;
            long len;
            //compress nasm.exe
            ms=new MemoryStream();
            Compress("nasm.exe",ms);
            bw.Write(ms.Length);
            ms.WriteTo(Out);
            ms.Close();
            //compress ndisasm.exe
            ms=new MemoryStream();
            Compress("ndisasm.exe",ms);
            bw.Write(ms.Length);
            ms.WriteTo(Out);
            ms.Close();
            //compress asmdriver.exe
            ms=new MemoryStream();
            Compress("asmdriver.exe",ms);
            bw.Write(ms.Length);
            ms.WriteTo(Out);
            ms.Close();
            //Write out the code file
            BinaryReader br=new BinaryReader(Out);
            br.BaseStream.Position=0;
            StreamWriter sw=new StreamWriter(File.Open("files.cs",FileMode.Create));
            sw.WriteLine("using System;");
            sw.WriteLine("using SharpZipLib;");
            sw.WriteLine("using System.Windows.Forms;");
            sw.WriteLine("using System.IO;");
            sw.WriteLine("namespace FileData {");
            sw.WriteLine("class Files {");
            sw.WriteLine("public static void WriteFiles() {");
            sw.WriteLine("FileStream fs;");
            sw.WriteLine("fs=File.Open(\"nasm.exe\",FileMode.Create);");
            sw.WriteLine("BZip2.Decompress(GetNasm(),fs);");
            sw.WriteLine("fs.Close();");
            sw.WriteLine("fs=File.Open(\"ndisasm.exe\",FileMode.Create);");
            sw.WriteLine("BZip2.Decompress(GetNdisasm(),fs);");
            sw.WriteLine("fs.Close();");
            sw.WriteLine("fs=File.Open(\"asmdriver.exe\",FileMode.Create);");
            sw.WriteLine("BZip2.Decompress(GetAsmDriver(),fs);");
            sw.WriteLine("fs.Close();");
            sw.WriteLine("}");
            //nasm
            sw.WriteLine("private static MemoryStream GetNasm() {");
            sw.WriteLine("return new MemoryStream(new byte[] {");
            len=br.ReadInt64();
            for(int i=0;i<len;i++) {
                if(i%500==0) sw.WriteLine();
                sw.Write(br.ReadByte().ToString()+",");
            }
            sw.WriteLine();
            //ndisasm
            sw.WriteLine("});");
            sw.WriteLine("}");
            sw.WriteLine("private static MemoryStream GetNdisasm() {");
            sw.WriteLine("return new MemoryStream(new byte[] {");
            len=br.ReadInt64();
            for(int i=0;i<len;i++) {
                if(i%500==0) sw.WriteLine();
                sw.Write(br.ReadByte().ToString()+",");
            }
            sw.WriteLine();
            //asmdriver
            sw.WriteLine("});");
            sw.WriteLine("}");
            sw.WriteLine("private static MemoryStream GetAsmDriver() {");
            sw.WriteLine("return new MemoryStream(new byte[] {");
            len=br.ReadInt64();
            for(int i=0;i<len;i++) {
                if(i%500==0) sw.WriteLine();
                sw.Write(br.ReadByte().ToString()+",");
            }
            sw.WriteLine();
            sw.WriteLine("});");
            sw.WriteLine("}");
            sw.WriteLine("}");
            sw.WriteLine("}");
            sw.Close();
        }
    }
}
