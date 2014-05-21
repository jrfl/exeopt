// Program.cs
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

#define fulltest
//#define partialtest
//#define clean
#if clean&&!useconsole
#error Cannot define clean when using the GUI!
#endif
#if !fulltest&&!useconsole
#error Must define fulltest when not using the console
#endif
#if partialtest&&!useconsole
#error Cannot define partialtest unless useconsole is defined
#endif
using System;
using System.IO;
using Process=System.Diagnostics.Process;
using ArrayList=System.Collections.ArrayList;
using System.Windows.Forms;

//Patch locations: 00267AB5 (fpu2sse patch 1) 0012573A (3 calcs inc v2) 00050BC4 (north south bug)
//00267B60 (complicated 3er that fpu2sse finds but I dont) 002E6FF1 (v9 with no complications)
//00267F39 (alligned compicated v4) 00268160 (horrendusly long and compicated m4,4)
//0015FC6C - bugged patch
namespace Patcher {

    public class Program {
        //Stuff which needs to be moved to a config file
        public static string[] Registers={ "eax","ebx","ecx","edx","esi","edi","esp","ebp","xmm0","xmm1","xmm2",
            "xmm3","xmm4","xmm5","xmm6","xmm7","st0","st1","st2","st3","st4","st5","st6","st7" };
        public static string[] JumpInstructions={ "ret","jmp","call","je","jz","jne","jnz","ja","jnbe",
            "jae","jnb","jb","jnae","jbe","jna","jg","jnle","jge","jnl","jl","jnge","jle","jng","jc","jnc","jo",
            "jno","js","jns","jpo","jnp","jpe","jp","jcxz","jecxz","loop","loopz","loope","loopnz","loopne",
            "iret","int","into","bound","enter","leave"};
        public static string[] FpuInstructions={ "fld","fst","fstp","fild","fist","fistp","fbld","fbstp","fxch",
            "fcmove","fcmovne","fcmovb","fcmovb","fcmovbe","fcmovnb","fcmovnbe","fcmovu","fcmovnu","fadd","faddp",
            "fiadd","fsub","fsubp","fisub","fsubr","fsubrp","fisubr","fmul","fmulp","fimul","fdiv","fdivp","fidiv",
            "fdivr","fdivrp","fidivr","fprem","fprem1","fabs","fchs","frndint","fscale","fsqrt","fextract","fcom",
            "fcomp","fcompp","fucom","fucomp","fucompp","ficom","ficomp","fcomi","fucomi","fcomip","fucomip",
            "ftst","fxam","fsin","fcos","fsincos","fptan","tpatan","f2xm1","fyl2x","fyl2xp1","fld1","fldz","fldpi",
            "fldl2e","fldln2","fldl2t","fldlg2","fincstp","fdexstp","ffree","finit","fninit","fclex","fnclex",
            "fstcw","fnstcw","fldcw","fstenv","fnstenv","fldenv","fsave","fnsave","frstor","fstsw","fnstsw",
            "wait","fwait","fnop","fxsave","fxrstor"};
        public static string[] ReadableFpuInstructions={ "fld","fst","fstp","fadd","faddp","fmul","fmulp",
            "fsub","fsubp","fsubr","fsubrp","fdiv","fdivp","fdivr","fdivrp","fxch"};
        public static string[] FpuIgnoreInstructions={ "faddp","fsubp","fsubrp","fmulp","fdivp","fdivrp","fxch" };
        public static string[] FpuReadInstructions={ "fld","fadd","fmul","fdiv","fdivr","fsub","fsubr" };
        public static string[] FpuWriteInstructions={ "fst","fstp" };
        public static string[] ReadOnlyInstructions={ "cmp" };
        public static string[] DissallowedAddresses={
            "0013D7A8", //Program jumps right into the middle of the fpu calculation
        };

        //Program settings
        public static string FileName="Morrowind.exe";
        public static string Directory="";
        public static string NakedFileName;
        public static PEfile FileData;
        public static StreamWriter LogFile;

        //configerable Settings
        public static bool[] Mod={ true,true,true };
        public static byte[] Align={ 0,1,2 };
        public static bool CreateLog=true;
        public static bool Benchmark=true;
        public static bool IgnoreBenchmark=true;
        public static bool Benchmark98Fix=true;
        //public static bool TestPatches=true;

        public static bool Restrict=false;
        public static int FirstPatch=3;
        public static int LastPatch=3;

        public static bool AutoGetCode=true;
        public static uint CodeOffset=0;
        public static uint CodeLength=0;

        //Some other stuff
        public static CodeSegment CurrentSeg;
        public static FpuSegment CurrentFpu;
        public static bool thingy=true; //save debug messages in log file
        public static long LineNo;
        public static int count3;

        [STAThread]
        public static void Main(string[] args) {
#if !useconsole
            Application.Run(new MainForm());
#else
            Patch();
            //EmmitPatch("00268163","0026836C");
            //MorrowindPatch("emittedpatch.patch");
            //EmmitConfigFile();
#endif
        }

        public static string ReadFile(string file) {
            StreamReader sr=new StreamReader(file);
            string s=sr.ReadToEnd();
            sr.Close();
            return s;
        }

        public static byte[] ReadFileBytes(string file) {
            FileStream fs=new FileStream(file,FileMode.Open);
            byte[] bytes=new byte[fs.Length];
            fs.Read(bytes,0,(int)fs.Length);
            fs.Close();
            return bytes;
        }

        public static void Patch() {
            OptimizationException.Reset();
            Directory=Path.GetDirectoryName(FileName)+"\\";
            if(Directory=="\\") Directory="";
            NakedFileName=Path.GetDirectoryName(FileName)+"\\"+Path.GetFileNameWithoutExtension(FileName);
            if(CreateLog) {
                LogFile=new StreamWriter(Directory+"log.txt");
                LogFile.AutoFlush=true;
            }
            if(AutoGetCode) {
                FileData=new PEfile(FileName);
            } else {
                FileData=new PEfile(CodeOffset,CodeLength);
            }            
#if useconsole
            //ExtractCode();
            //DecompileCode();
            ReadCode();
            ApplyPatchedCode();
            Console.WriteLine("Push any key to exit ("+count3.ToString()+" patches)");
            Console.ReadKey();
#else
            ExtractCode();
            DecompileCode();
            ReadCode();
            ApplyPatchedCode();
            Console.WriteLine("Finished. Applied "+count3.ToString()+" patches.");
#endif
            if(CreateLog) {
                OptimizationException.LogErrors();
                LogFile.WriteLine("------------------------------------------------");
                LogFile.WriteLine(count3.ToString()+" patches were applied");
                LogFile.Close();
            }
        }

        public static bool OptimizeSegment(ref CodeSegment segment) {
#if !fulltest
            if(segment.Lines[0].address!="0015FC6C") return false;
            Console.WriteLine(LineNo);
#endif
            if(segment.ContainsFpu==false) return false;
            //if(segment.Lines.Length<10) return false;
            
            //Fpu to sse stuff
            FpuSegment FpuStream=segment.GetFpuStream();
            CurrentSeg=segment;
            CurrentFpu=FpuStream;
            thingy=true;
            fpu.Reset();
            string s;
            for(byte i=0;i<FpuStream.Lines.Length;i++) {
                fpu.PerformOp(FpuStream.Lines[i],i);
            }
            fpu.Process();
            vectorizer.Reset();
            for(int i=0;i<fpu.Result.Length;i++) {
                vectorizer.AddLine(fpu.Result[i],fpu.ResultLines[i]);
            }
            VectorMatch[] PatchedCode=vectorizer.Process();
            if(PatchedCode==null||PatchedCode.Length==0) throw new OptimizationException("Main: fpu code not vectorizable");
            CodeGenerator.GenerateCode(new ArrayList(PatchedCode));
            Patcher.ApplyPatch(FpuStream);
            if(CreateLog) {
                s=string.Join("\r\n",fpu.Result);
                LogFile.Write(segment.Lines[0].address+":");
                LogFile.WriteLine(CodeGenerator.ToHex(HexParse(FpuStream.Lines[0].address)+4198400).Remove(0,3).PadLeft(8,'0'));
                LogFile.WriteLine(s);
                LogFile.WriteLine("\r\nPatched:");
                LogFile.WriteLine(segment.FpuString);
                LogFile.WriteLine("\r\nwith:");
                LogFile.Write(FpuStream.PreString()+ReadFile("out.txt").Replace("bits 32\r\n","")+FpuStream.PostString());
                if(Benchmark) {
                    LogFile.WriteLine("\r\nBenchmark:");
                    LogFile.WriteLine("FPU: "+Patcher.FpuTime.ToString());
                    LogFile.WriteLine("SSE: "+Patcher.SseTime.ToString());
                    LogFile.WriteLine("Ratio: "+((double)Patcher.SseTime.Ticks/(double)Patcher.FpuTime.Ticks).ToString());
                }
                LogFile.WriteLine("------------------------------------------------");
            }
            return true;
        }

        public static void ReadCode() {
#if clean
            File.Delete("Morrowind.exe");
            File.Delete("code");
            File.Copy("clean\\Morrowind.exe","Morrowind.exe");
            File.Copy("clean\\code","code");
#endif
            Console.WriteLine("Loading code segments");
            StreamReader sr=new StreamReader("dcode.txt");
            ArrayList segLines=new ArrayList();
            int count=0;
            bool DropSegment=false;
            bool StartedSegment=true;
            LineInfo li;
#if !fulltest||partialtest
            int count2=0;
#endif
            count3=0;
            while(sr.Peek()!=-1) {
                string s=sr.ReadLine();
#if !fulltest||partialtest
                count2++;
                LineNo=count2;
                if(count2<500000||count2>510000) continue;
#endif
                try {
                    li=new LineInfo(s);
                } catch(Exception) { DropSegment=true; li=new LineInfo("00000000 00 int3"); }
                if(Array.IndexOf(JumpInstructions,li.instruction)==-1) {
                    if(StartedSegment) {
                        segLines.Add(li);
                    } else if(Array.IndexOf(FpuInstructions,li.instruction)!=-1) {
                        StartedSegment=true;
                        segLines.Add(li);
                    }
                } else if(StartedSegment) {
                    if(!DropSegment) {
                        segLines.Add(li);
                        CodeSegment cs=new CodeSegment(segLines);
                        try {
                            if(OptimizeSegment(ref cs)) count3++;
#if !fulltest
                        } catch(OptimizationException ex) {
                            if(thingy) {
                                try {
                                    LogFile.WriteLine(CurrentSeg.Lines[0].address);
                                    LogFile.WriteLine("\nPatched:");
                                    LogFile.Write(CurrentSeg.ToString());
                                    LogFile.WriteLine("\nwith:");
                                    s=string.Join("\n",fpu.Result);
                                    LogFile.WriteLine(s);
                                    //LogFile.Write(tempFpu.PreString()+File.ReadAll("out.txt")+tempFpu.PostString());
                                    LogFile.WriteLine("------------------------------------------------");
                                } catch { }
                                string ss=ex.ToString();
                                Console.WriteLine(ss);
                                thingy=false;
                            }

#else
                        } catch(OptimizationException) {
#endif
                        }
                        count++;
#if useconsole
                        if(count%100==0) {
                            Console.WriteLine("Processed "+count.ToString()+" segments and found "+count3+" patches.");
                        }
#else
                        Console.WriteLine("Processed "+count.ToString()+" segments and found "+count3+" patches.");
#endif
                    }
                    segLines.Clear();
                    DropSegment=false;
                    StartedSegment=false;
                } else DropSegment=false;
            }
            sr.Close();
        }

        public static int HexParse(string s) {
            bool minus=false;
            if(s.StartsWith("+0x")) {
                s=s.Remove(0,3);
            } else if(s.StartsWith("-0x")) {
                s=s.Remove(0,3);
                minus=true;
            } else if(s.StartsWith("0x")) {
                s=s.Remove(0,2);
            }
            if(minus) {
                return -int.Parse(s,System.Globalization.NumberStyles.AllowHexSpecifier);
            } else {
                return int.Parse(s,System.Globalization.NumberStyles.AllowHexSpecifier);
            }
        }

        public static void ExtractCode() {
            Console.WriteLine("Extracting code");
            //Copy code out of executable file
            FileStream fs=File.OpenRead(FileName);
            fs.Position=FileData.CodeOffset;
            FileStream output=File.Create("code");
            byte[] b=new byte[4096];
            while((fs.Position<FileData.CodeOffset+FileData.CodeSize)&&fs.Position<fs.Length) {
                fs.Read(b,0,4096);
                if(fs.Position>FileData.CodeOffset+FileData.CodeSize) {
                    output.Write(b,0,(int)(FileData.CodeOffset+FileData.CodeSize-fs.Position));
                } else {
                    output.Write(b,0,4096);
                }
            }
            fs.Close();
            output.Close();
        }

        public static void DecompileCode() {
            Console.WriteLine("Decompiling");
            Process p=new Process();
            p.StartInfo.FileName="ndisasm.exe";
            p.StartInfo.Arguments="-b 32 code";
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.StartInfo.RedirectStandardOutput=true;
            FileStream fs=File.Create("dcode.txt");
            StreamWriter sw=new StreamWriter(fs);
            p.Start();
            sw.Write(p.StandardOutput.ReadToEnd());
            p.WaitForExit();
            p.Close();
            sw.Close();
            fs.Close();
        }

        public static void ApplyPatchedCode() {
            Console.WriteLine("Applying patched code");
            FileStream fs=File.OpenRead("code");
            FileStream input=File.Open(FileName,FileMode.Open);
            input.Position=FileData.CodeOffset;
            byte[] b=new byte[4096];
            while(fs.Position<fs.Length-1) {
                fs.Read(b,0,4096);
                input.Write(b,0,4096);
            }
            fs.Close();
            input.Close();
        }

        public static void EmmitPatch(string address) {
            Process p=new Process();
            p.StartInfo.FileName="nasm.exe";
            p.StartInfo.Arguments="patch.asm";
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.Start();
            p.WaitForExit();
            FileInfo fi=new FileInfo("patch");
            if(fi.Length==0) throw new Exception("Patcher: Code generator produced uncompilable code");
            byte[] b=ReadFileBytes("patch");
            BinaryWriter bw=new BinaryWriter(File.Create("emittedpatch.patch",4096));
            bw.Write(HexParse(address));
            bw.Write(b.Length);
            bw.Write(b,0,b.Length);
            bw.Close();
        }

        public static void EmmitPatch(string address,string endaddress) {
            int start=HexParse(address);
            int end=HexParse(endaddress);
            Process p=new Process();
            p.StartInfo.FileName="nasm.exe";
            p.StartInfo.Arguments="patch.asm";
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.Start();
            p.WaitForExit();
            FileInfo fi=new FileInfo("patch");
            if(fi.Length==0) throw new Exception("Patcher: Code generator produced uncompilable code");
            ArrayList al=new ArrayList(ReadFileBytes("patch"));
            byte[] b=new byte[end-start];
            al.CopyTo(b);
            for(int i=al.Count;i<b.Length;i++) {
                b[i]=144;
            }
            int a=(end-start)-al.Count;
            if(a>127) {
                b[al.Count]=235;
                b[al.Count+1]=127;
                //throw new OptimizationException("Patcher: Patch end address out of range of a short jump");
            } else if(a>2) {
                b[al.Count]=235;
                b[al.Count+1]=(byte)(a-2);
            }
            BinaryWriter bw=new BinaryWriter(File.Create("emittedpatch.patch",4096));
            bw.Write(HexParse(address));
            bw.Write(b.Length);
            bw.Write(b,0,b.Length);
            bw.Close();
        }

        public static void MorrowindPatch(string file) {
            BinaryReader br=new BinaryReader(File.OpenRead(file));
            FileStream fs=File.Open(FileName,FileMode.Open);
            fs.Position=br.ReadInt32()+4096;
            byte[] b=new byte[br.ReadInt32()];
            br.Read(b,0,b.Length);
            fs.Write(b,0,b.Length);
            br.Close();
            fs.Close();
        }

        public static void EmmitConfigFile() {
            StreamWriter sw=new StreamWriter("config.ini");
            sw.WriteLine(";comments must be on their own line and begin with ';'");
            sw.WriteLine(";Only new sections may begin with '['");
            sw.WriteLine(";The order of the sections may be modified, and sections may be removed completely");
            sw.WriteLine(";Do not begin a line with a space, or leave blank lines");
            sw.WriteLine(";The lines of each section belong to the header _below_ the line");

            foreach(string s in Registers) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[registers]");
            foreach(string s in JumpInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[jump instructions]");
            foreach(string s in FpuInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[fpu instructions]");
            foreach(string s in ReadableFpuInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[parsable fpu instructions]");
            foreach(string s in FpuIgnoreInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[fpu ignore]");
            foreach(string s in FpuReadInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[fpu read]");
            foreach(string s in FpuWriteInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[fpu write]");
            foreach(string s in ReadOnlyInstructions) {
                sw.WriteLine(s);
            }
            sw.WriteLine("[read only instructions]");
            sw.Close();
        }

    }
}
