// Patcher.cs
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

//#define emitpatch
using System;
using ArrayList=System.Collections.ArrayList;
using Process=System.Diagnostics.Process;
using System.IO;
using System.Runtime.InteropServices;

namespace Patcher {
    public class Patcher {

        public static TimeSpan Bias;
        public static int PatchCount=0;

        public static void ApplyPatch(FpuSegment seg) {
            Process p=new Process();
            p.StartInfo.FileName="nasm.exe";
            p.StartInfo.Arguments="out.txt";
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.Start();
            p.WaitForExit();
            p.Close();
            FileInfo fi=new FileInfo("out");
            if(fi.Length==0) throw new OptimizationException("Patcher: Code generator produced uncompilable code"); 
            ArrayList code=new ArrayList();
            for(int i=0;i<seg.Pre.Length;i++) {
                code.AddRange(HexToString(seg.Pre[i].code));
            }
            code.AddRange(Program.ReadFileBytes("out"));
            for(int i=0;i<seg.Post.Length;i++) {
                code.AddRange(HexToString(seg.Post[i].code));
            }
            if(code.Count>seg.End-seg.Start) throw new OptimizationException("Patcher: Patch to big to fit into code");

            /*if(Program.TestPatches) {
                TestPatch(seg); //TESTPATCH CALL
            }*/
            if(Program.Benchmark) {
                Benchmark(seg); //BENCHMARK CALL!!!!!!!!!!!!!!!!!!!!!!!! <---------------- OVER HERE
            }
            byte[] Code=new byte[seg.End-seg.Start];
            code.CopyTo(Code);
            for(int i=code.Count;i<(seg.End-seg.Start);i++) {
                Code[i]=144;    //Fill the rest of the code up with nop's
            }
            long a=(seg.End-seg.Start)-code.Count;
            if(a>255) {
                throw new OptimizationException("Patcher: Patch end address out of range of a short jump");
            } else if(a>2) {
                Code[code.Count]=235;
                Code[code.Count+1]=(byte)(a-2);
            }
            if(Program.Restrict) {
                if(PatchCount<Program.FirstPatch||PatchCount>Program.LastPatch) {
                    PatchCount++;
                    throw new OptimizationException("Patcher: Patch restricted");
                }
                PatchCount++;
            }
#if emitpatch
            FileStream fs2=File.Create("emittedpatch.patch",4096);
            BinaryWriter bw=new BinaryWriter(fs2);
            bw.Write(seg.Start);
            bw.Write(Code.Length);
            bw.Write(Code,0,Code.Length);
            bw.Close();
#endif
            FileStream fs=File.Open("code",FileMode.Open);
            fs.Position=seg.Start;
            fs.Write(Code,0,Code.Length);
            fs.Close();
        }

        public static ArrayList HexToString(string str) {
            if(str.Length%2!=0) throw new OptimizationException("Patcher: Wrong size string passed to HexToString");
            ArrayList result=new ArrayList();
            while(str.Length>0) {
                string s=""+str[0]+str[1];
                result.Add((byte)Program.HexParse(s));
                str=str.Remove(0,2);
            }
            return result;
        }

        //Windows XP version
        //public const int InjectionPoint=0x23F;
        //public const int LoopPoint=0x21E;
        //Windows 98 version
        public const int InjectionPoint=0x103F; //Where code gets injected into asmDriver.exe
        public const int LoopPoint=0x101E;      //Where the number of code loops gets written in asmDriver.exe
        public const int TestPoint1=0x102B;     //Injection site for fpu code in asmTester.exe
        public const int TestPoint2=0x1451;     //Injection site for sse code in asmTester.exe
        public const long InjectionLength=1000; //Length of injection site in both asm programs
        public static byte[] JumpCode=new byte[] { 255,100,36,196 };  //FF6424C4 (Doesn't need to be changed)
        public static byte[] LoopCode=new byte[] { 0x80,0x96,0x98,0x00 }; //10,000,000
        public static float TimeDifference;
        public static TimeSpan SseTime;
        public static TimeSpan FpuTime;

        /*
        public static void TestPatch(FpuSegment seg) {
            ArrayList TempCode;
            //Generate the code
            TempCode=new ArrayList(Program.ReadFileBytes("out"));
            byte[] sseCode=(byte[])TempCode.ToArray(typeof(byte));
            TempCode.Clear();
            StreamWriter sr=new StreamWriter("tout.txt");
            sr.WriteLine("bits 32");
            foreach(string s in seg.TestData) {
                sr.WriteLine(s);
            }
            sr.Close();
            Process p=new Process();
            p.StartInfo.FileName="nasm.exe";
            p.StartInfo.Arguments="-o tout tout.txt";
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.Start();
            p.WaitForExit();
            p.Close();
            FileInfo fi=new FileInfo("tout");
            if(fi.Length==0) throw new OptimizationException("Patcher: patch tester produced uncompilable code");
            TempCode.AddRange(Program.ReadFileBytes("tout"));
            byte[] fpuCode=(byte[])TempCode.ToArray(typeof(byte));
            //inject and run the code
            if(sseCode.Length>InjectionLength||fpuCode.Length>InjectionLength)
                throw new OptimizationException("Checker: Injection site is too short");
            try {
                File.Delete("asmTester2.exe");
            } catch { }
            try {
                File.Copy("asmTester.exe","asmTester2.exe");
                FileStream fs=File.Open("asmTester2.exe",FileMode.Open);
                fs.Position=TestPoint1;
                fs.Write(fpuCode,0,fpuCode.Length);
                fs.Position=TestPoint2;
                fs.Write(sseCode,0,sseCode.Length);
                fs.Close();
                p=new Process();
                p.StartInfo.UseShellExecute=false;
                p.StartInfo.CreateNoWindow=true;
                p.StartInfo.FileName="asmTester2.exe";
                p.Start();
                p.WaitForExit();
                if(!p.HasExited) {
                    p.Kill();
                    p.Close();
                    throw new OptimizationException("Checker: Process has not exited");
                }
                if(p.ExitCode!=1) {
                    p.Close();
                    throw new OptimizationException("Checker: Patch appears to be corrupt");
                }
                p.Close();
            } catch (Exception e) {
                if(e is OptimizationException) throw;
                throw new OptimizationException("Checker: Something threw an exception");
            }

        }
        */

        public static void Benchmark(FpuSegment seg) {
            ArrayList TempCode;
            //Get the sse code
            TempCode=new ArrayList(Program.ReadFileBytes("out"));
            TempCode.AddRange(JumpCode);
            byte[] bytes=(byte[])TempCode.ToArray(typeof(byte));
            TempCode.Clear();
            //Get the fpu code
            int a=0;
            for(int i=0;i<seg.Lines.Length;i++) {
                if(a==CodeGenerator.FpuLines.Length) break;
                if(CodeGenerator.FpuLines[a]==i) {
                    TempCode.AddRange(HexToString(seg.Lines[i].code));
                    a++;
                }
            }
            TempCode.AddRange(JumpCode);
            byte[] bytes2=(byte[])TempCode.ToArray(typeof(byte));
            Benchmark(bytes,bytes2);
            if(!Program.IgnoreBenchmark) {
                if(SseTime>=FpuTime+Bias) throw new OptimizationException("Benchmarker: Patch was slower than original code");
            }
        }

        public static void Benchmark(byte[] newcode,byte[] oldcode) {
            if(newcode.Length>InjectionLength) throw new OptimizationException("Benchmarker: Injection site too short");
            if(oldcode.Length>InjectionLength) throw new OptimizationException("Benchmarker: Injection site too short");
            FileStream fs;
#if !dotnet2
            DateTime sStartTime;
            DateTime fStartTime;
#endif
            //Stick the loop size into asmdriver.exe
            fs=File.Open("asmdriver.exe",FileMode.Open);
            fs.Position=LoopPoint;
            fs.Write(LoopCode,0,4);
            fs.Close();
            //Copy the files
            if(File.Exists("sse.exe")) File.Delete("sse.exe");
            File.Copy("asmDriver.exe","sse.exe");
            if(File.Exists("fpu.exe")) File.Delete("fpu.exe");
            File.Copy("asmDriver.exe","fpu.exe");
            //Inject the code
            fs=new FileStream("sse.exe",FileMode.Open);
            fs.Position=InjectionPoint;
            fs.Write(newcode,0,newcode.Length);
            fs.Close();
            fs=new FileStream("fpu.exe",FileMode.Open);
            fs.Position=InjectionPoint;
            fs.Write(oldcode,0,oldcode.Length);
            fs.Close();
            //Run the executables and time how long they run for
            Process p=new Process();
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.StartInfo.FileName="sse.exe";
#if !dotnet2
            sStartTime=DateTime.Now;
#endif
            p.Start();
            p.WaitForExit();
#if !dotnet2
            if(Program.Benchmark98Fix) {
                SseTime=DateTime.Now-sStartTime;
            } else {
                try {
                    SseTime=p.ExitTime-sStartTime;        //Have to get start time before process exit in .net 1.1
                } catch(PlatformNotSupportedException) {
                    try { p.Close(); } catch { }
                    throw new PlatformNotSupportedException("Benchmarker threw a PlatformNotSupported exception. If you are "+
                        "running windows 98 and want to use the benchmarker, please make sure that the windows 98 compatibility fix "+
                        "checkbox on the benchmarker tab is checked.");
                }
            }
#else
            SseTime=p.TotalProcessorTime;
#endif
            p.Close();
            p.StartInfo.UseShellExecute=false;
            p.StartInfo.CreateNoWindow=true;
            p.StartInfo.FileName="fpu.exe";
#if !dotnet2
            fStartTime=DateTime.Now;
#endif
            p.Start();
            p.WaitForExit();
#if !dotnet2
            if(Program.Benchmark98Fix) {
                FpuTime=DateTime.Now-fStartTime;
            } else {
                FpuTime=p.ExitTime-fStartTime;
            }
#else
            FpuTime=p.TotalProcessorTime;
#endif
            p.Close();
        }
    }
}
