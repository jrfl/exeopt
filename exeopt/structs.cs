// structs.cs
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
using System.Collections;

namespace Patcher {
    public class OptimizationException : Exception {
        public static ArrayList messages=new ArrayList();
        public static ArrayList counter=new ArrayList();

        public static void LogErrors() {
            Program.LogFile.WriteLine("Errors:");
            for(int i=0;i<messages.Count;i++) {
                Program.LogFile.WriteLine(((int)counter[i]).ToString().PadRight(7,'.')+messages[i]);
            }
        }

        public OptimizationException(string message) : base(message) {
            int i;
            if((i=messages.IndexOf(message))==-1) {
                messages.Add(message);
                counter.Add(1);
            } else {
                int a=(int)counter[i];
                counter.RemoveAt(i);
                counter.Insert(i,++a);
            }
        }
        
        public static void Reset() {
            messages.Clear();
            counter.Clear();
        }
    }

    [Flags]
    public enum ArgsToUse : byte { None=0, Left=1, Right=2, Both=3 }
    public enum VectorDirection { Static, Up, Down, Unknown }
    public enum OperandType { Register, dMemory, rMemory, Immediate }
    public enum OperandSize : byte { Unknown=0, Byte=1, Word=2, dWord=4, qWord=8 }

    public struct Operation {
        public Vector op1;
        public Vector op2;
        public char op;
        public byte op1loc;
        public byte op2loc;
        public ArgsToUse args;

        public Operation(Vector Op1,Vector Op2,char Op) {
            op1=Op1;
            op2=Op2;
            op=Op;
            op1loc=255;
            op2loc=255;
            args=ArgsToUse.None;
        }

        public Operation(byte Op1,Vector Op2,char Op) {
            op1=new Vector();
            op2=Op2;
            op=Op;
            op1loc=Op1;
            op2loc=255;
            args=ArgsToUse.Left;
        }

        public Operation(Vector Op1,byte Op2,char Op) {
            op1=Op1;
            op2=new Vector();
            op=Op;
            op1loc=255;
            op2loc=Op2;
            args=ArgsToUse.Right;
        }

        public Operation(byte Op1,byte Op2,char Op) {
            op1=new Vector();
            op2=new Vector();
            op=Op;
            op1loc=Op1;
            op2loc=Op2;
            args=ArgsToUse.Both;
        }
    }

    public struct Element {
        public int Offset;
        public string Register;

        public Element(string str) {
            str=str.Replace("[","");
            str=str.Replace("]","");
            str=str.Replace("[","");
            if(str.IndexOf('+')!=-1) {
                string[] s=str.Split('+');
                Register=s[0];
                Offset=(int)Program.HexParse(s[1]);
            } else if(str.IndexOf('-')!=-1) {
                string[] s=str.Split('-');
                Register=s[0];
                Offset=-(int)Program.HexParse(s[1]);
            } else {
                Register=str;
                Offset=0;
            }
            if(Array.IndexOf(Program.Registers,Register)==-1) throw new OptimizationException("element: Invalid register");
        }
    }
    
    public struct Vector {
        public int Offset;
        public string Register;
        public int Count;
        public VectorDirection Direction;

        public Vector(int offset,string reg,int count,VectorDirection dir) {
            Offset=offset;
            Register=reg;
            Count=count;
            Direction=dir; ;
        }
    }

    public struct VectorMatch {
        public int Offset;
        public string Reg;
        public int Start;
        public int Count;
        public bool Upward;
        public Element[][] arguments;
        public string calc;
        public Vector[] pArguments;
        public int[][] fpulines;

        public VectorMatch(int start,int count,bool upward,int offset,string reg) {
            Start=start;
            Count=count;
            Upward=upward;
            calc=null;
            arguments=null;
            pArguments=null;
            Offset=offset;
            Reg=reg;
            fpulines=null;
        }

        public VectorMatch(int start,int count,bool upward,UnwrappedElement[] elements,int offset,string reg) {
            fpulines=null;
            Offset=offset;
            Reg=reg;
            Start=start;
            Count=count;
            Upward=upward;
            calc=vectorizer.ParseMarkedScalarCalc(elements[0].stuff);
            arguments=new Element[elements.Length][];
            for(int i=0;i<elements.Length;i++) {
                arguments[i]=GetArgs(elements[i].stuff);
            }
            pArguments=null;
        }

        public VectorMatch(VectorMatch vm,int arg,int start,int count,VectorDirection dir) {
            Offset=vm.Offset+4*start; ;
            Reg=vm.Reg;
            Start=start+vm.Start;
            Count=count;
            Upward=vm.Upward;
            calc=vm.calc;
            if(dir==VectorDirection.Unknown) throw new OptimizationException("VectorMatch: Unknown vector direction");
            fpulines=new int[Count][];
            for(int i=Start;i<count+Start;i++) {
                fpulines[i-Start]=((UnwrappedElement)vectorizer.elements[i]).FpuLines;
            }
            arguments=new Element[count][];
            Array.Copy(vm.arguments,start,arguments,0,count);
            if(vm.pArguments!=null) {
                pArguments=new Vector[vm.pArguments.Length+1];
                vm.pArguments.CopyTo(pArguments,0);
                for(int i=0;i<vm.pArguments.Length-1;i++) {
                    pArguments[i].Count=count;
                    if(start>0) {
                        if(pArguments[i].Direction==VectorDirection.Up) {
                            pArguments[i].Offset+=4*start;
                        } else if(pArguments[i].Direction==VectorDirection.Down) {
                            pArguments[i].Offset-=4*start;
                        }
                    }
                }
            } else {
                pArguments=new Vector[1];
            }
            pArguments[arg]=new Vector(arguments[0][arg].Offset,arguments[0][arg].Register,count,dir);
        }

        private static Element[] GetArgs(string s) {
            ArrayList els=new ArrayList();
            string r="";
            bool d=false;
            for(int i=0;i<s.Length;i++) {
                switch(s[i]) {
                    case '[': d=true; break;
                    case ']': d=false; els.Add(new Element(r)); r=""; break;
                    case ';': break;
                    default:
                        if((byte)s[i]>127) break;
                        if(d) r+=s[i];
                        break;
                }
            }
            return (Element[])els.ToArray(typeof(Element));
        }
    }

    public class UnwrappedElement {
        public int Offset;
        public string Register;
        public string stuff;
        public int[] FpuLines;

        public override string ToString() {
            return "["+Register+CodeGenerator.ToHex(Offset)+"]="+stuff;
        }


        public UnwrappedElement(string str,int[] fpulines) {
            string[] ss=str.Split('=');
            str=ss[0];
            str=str.Replace("[","");
            str=str.Replace("]","");
            if(str.IndexOf('+')!=-1) {
                string[] s=str.Split('+');
                Register=s[0];
                Offset=(int)Program.HexParse(s[1]);
            } else if(str.IndexOf('-')!=-1) {
                string[] s=str.Split('-');
                Register=s[0];
                Offset=-(int)Program.HexParse(s[1]);
            } else {
                Register=str;
                Offset=0;
            }
            if(Array.IndexOf(Program.Registers,Register)==-1) throw new OptimizationException("UnwrappedElement: Invalid register");
            stuff=ss[1];
            FpuLines=fpulines;
        }
    }

    public struct OperandInfo {
        public string Operand;
        public OperandType OpType;
        public OperandSize OpSize;
        public bool MultiPart;
        public string Register;
        public int Offset;
        public string Qualifier;

        public void ModOffset(int i) {
            Offset+=i;
            string s=Register+CodeGenerator.ToHex(Offset);
            if(Operand.StartsWith("[")) {
                Operand="["+s+"]";
            } else {
                Operand=s;
            }
        }

        public OperandInfo(string operand) {
            operand=operand.ToLower();
            string[] ss=operand.Split(' ');
            if(ss.Length>2) throw new OptimizationException("Operand: More than two parts");
            if(ss.Length==2) {
                Qualifier=ss[0];
                operand=ss[1];
            } else { Qualifier=null; }
            if(operand.IndexOf('*')!=-1) throw new OptimizationException("Operand: Haven't programmed '*' yet");
            Operand=operand;
            bool p=false;
            if(operand.StartsWith("[")) {
                p=true;
                operand=operand.Replace("[","").Replace("]","");
            }
            if(operand.StartsWith("+0x")||operand.StartsWith("-0x")||operand.StartsWith("0x")) {
                if(p) { OpType=OperandType.dMemory; } else { OpType=OperandType.Immediate; }
                Offset=Program.HexParse(operand);
                Register=null;
                MultiPart=false;
            } else {
                if(p) { OpType=OperandType.rMemory; } else { OpType=OperandType.Register; }
                ss=operand.Split(':');
                if(ss.Length>2) throw new OptimizationException("Operand: Illegal operand");
                if(ss.Length==2) {
                    operand=ss[1];
                    MultiPart=true;
                } else { MultiPart=false; }
                if(operand.IndexOf("+")!=-1) {
                    ss=operand.Split('+');
                    operand=ss[0];
                    if(!ss[1].StartsWith("0x")) throw new OptimizationException("Operand: Illegal offset");
                    Offset=Program.HexParse(ss[1]);
                } else if(operand.IndexOf("-")!=-1) {
                    ss=operand.Split('-');
                    operand=ss[0];
                    if(!ss[1].StartsWith("0x")) throw new OptimizationException("Operand: Illegal offset");
                    Offset=-Program.HexParse(ss[1]);
                } else { Offset=0; }
                if(Array.IndexOf(Program.Registers,operand)==-1) throw new OptimizationException("Operand: unknown register");
                Register=operand;
            }
            if(OpType==OperandType.Register) {
                OpSize=OperandSize.dWord;
            } else if(Qualifier!=null) {
                switch(Qualifier) {
                    case ("qword"): OpSize=OperandSize.qWord; break;
                    case ("dword"): OpSize=OperandSize.dWord; break;
                    case ("word"): OpSize=OperandSize.Word; break;
                    case ("byte"): OpSize=OperandSize.Byte; break;
                    default: throw new OptimizationException("Operand: Unknown prefix");
                }
            } else {
                OpSize=OperandSize.Unknown;
            }
        }
    }

    public struct LineInfo {
        public string address;
        public string code;
        public string instruction;
        public string StoredOperands;

        private OperandInfo[] RealOperands;
        public OperandInfo[] operands {
            get {
                if(RealOperands==null) FetchOperands();
                return RealOperands;
            }
        }

        private void FetchOperands() {
            string[] ops=StoredOperands.Split(',');
            RealOperands=new OperandInfo[ops.Length];
            for(int i=0;i<ops.Length;i++) {
                RealOperands[i]=new OperandInfo(ops[i]);
            }
        }

        public override string ToString() {
            return instruction.PadRight(7,' ')+StoredOperands;
        }

        public LineInfo(string str) {
            while(str.IndexOf("  ")!=-1) {
                str=str.Replace("  "," ");
            }
            string[] line=str.Split(' ');
            if(line.Length<3) {
                throw new OptimizationException("Segmenter: Unparsable line in segment");
            } 
            address=line[0].ToUpper();
            code=line[1].ToUpper();
            instruction=line[2].ToLower();
            if(line.Length>3) {
                StoredOperands=string.Join(" ",line,3,line.Length-3);
                RealOperands=null;
            } else {
                RealOperands=new OperandInfo[0];
                StoredOperands="";
            }
            //Check for specific cases and fix them
            if(instruction=="fxch"&&StoredOperands=="") {
                RealOperands=null;
                StoredOperands="st1";
            }
        }
    }

    public struct CodeSegment {
        public struct IntString {
            public int i;
            public string s;
            public IntString(int I,string S) {
                i=I;
                s=S;
            }
        }
        public bool ContainsFpu;
        public LineInfo[] Lines;
        public ArrayList ChangedMem;
        public ArrayList ReadReg;
        public ArrayList ReadMem;
        public int[] LineNos;
        public LineInfo[] FpuLines;
        public string FpuString;

        public CodeSegment(ArrayList lines) {
            Lines=(LineInfo[])lines.ToArray(typeof(LineInfo));
            ContainsFpu=false;
            int i=0;
            foreach(LineInfo li in Lines) {
                if(Array.IndexOf(Program.ReadableFpuInstructions,li.instruction)!=-1) i++;
            }
            if(i>6) ContainsFpu=true;
            ChangedMem=new ArrayList();
            ReadMem=new ArrayList();
            ReadReg=new ArrayList();
            LineNos=null;
            FpuLines=null;
            FpuString="";
        }

        

        /// <summary>
        /// This is too add an fpu line to a new fpu segment - all lines in here are fpu commands
        /// </summary>
        public void AddLine(LineInfo li,int LinoNo) {
            //Check for illegal opperand data
            if(li.StoredOperands.IndexOf(",")!=-1) {
                //TODO: Make this a bit more general
                if(li.instruction=="fdivp"&&li.operands[0].Register=="st1"&&li.operands[1].Register=="st0") {
                    li=new LineInfo(li.address+" "+li.code+" fdivrp st1");
                } else {
                    throw new OptimizationException("Segmenter: Fpu instruction had 2 operands");
                }
            }
            if(li.StoredOperands=="") throw new OptimizationException("Segmenter: Fpu instruction had no operands");
            //TODO: Check that this line of code doesn't break anything!
            if(Array.IndexOf(Program.FpuIgnoreInstructions,li.instruction)!=-1||li.operands[0].OpType==
                OperandType.Register) return;
            if(li.operands[0].OpType!=OperandType.rMemory)
                throw new OptimizationException("Segmenter: Cannot do anything with non memory operands");
            if(li.operands[0].MultiPart)
                throw new OptimizationException("Segmenter: Cannot do anything with multipart operands");
            if(li.operands[0].OpSize!=OperandSize.Unknown&&li.operands[0].OpSize!=OperandSize.dWord)
                throw new OptimizationException("Segmenter: Cannot do anything with non float data values");
            if(Math.Abs(li.operands[0].Offset)>65535)
                throw new OptimizationException("Segmenter: Memory offset is too great");
            //Add the operand to the appropriate arraylists
            if(Array.IndexOf(Program.FpuReadInstructions,li.instruction)!=-1) {
                ReadMem.Add(new IntString(LinoNo,li.operands[0].Operand));
                if(!ScanList(ChangedMem,LinoNo,li.operands[0].Operand,true)) throw new OptimizationException("Segmenter: Interdependent fpu calculations");
            }
            if(Array.IndexOf(Program.FpuWriteInstructions,li.instruction)!=-1) {
                ChangedMem.Add(new IntString(LinoNo,li.operands[0].Operand));
            }
            ReadReg.Add(new IntString(LinoNo,li.operands[0].Register));
        }

        public bool ScanList(ArrayList list,int LineNo,string SearchFor,bool upward) {
            if(list.Count==0) return true;
            if(upward) {
                foreach(IntString intstr in list) {
                    if(intstr.i<LineNo&&intstr.s==SearchFor) return false;
                }
            } else {
                foreach(IntString intstr in list) {
                    if(intstr.i>LineNo&&intstr.s==SearchFor) return false;
                }
            }
            return true;
        }

        public void ModEsp(int LineNo,bool upward,int amount) {
            int a; int b; int c=0;
            for(int i=0;i<LineNos.Length;i++) {
                if(LineNos[i]>LineNo) {
                    c=i;
                    break;
                }
            }
            if(upward) {
                a=0;
                b=c;
            } else {
                a=c;
                b=FpuLines.Length;
            }
            for(int i=a;i<b;i++) {
                if(Array.IndexOf(Program.ReadableFpuInstructions,FpuLines[i].instruction)!=-1&&
                  FpuLines[i].operands[0].Register=="esp") {
                    FpuLines[i].operands[0].ModOffset(amount);
                    FpuLines[i].StoredOperands="DWORD "+FpuLines[i].operands[0].Operand;
                }
            }
        }

        public bool CanMoveInstruction(LineInfo li,int LineNo,bool upward) {
            if(li.operands.Length>2) throw new OptimizationException("Segmenter: Cannot move instructions with 3 operands");
            if(li.operands.Length==0) throw new OptimizationException("Segmenter: Cannot move instructions with implicit operands");
            if(li.operands.Length==1) {
                switch(li.instruction) {
                    case "push":
                        switch(li.operands[0].OpType) {
                            case OperandType.rMemory:
                                if(!ScanList(ChangedMem,LineNo,li.operands[0].Operand,upward)) return false;
                                break;
                            case OperandType.Immediate: throw new OptimizationException("Segmenter: Immediate operand pushed onto stack");
                        }
                        if(upward) {
                            ModEsp(LineNo,upward,4);
                        } else {
                            ModEsp(LineNo,upward,-4);
                        }
                        break;                       
                        //li=new LineInfo("00000000 00 mov esp,"+li.operands[0].Operand);break;
                    case "pop":
                        switch(li.operands[0].OpType) {
                            case OperandType.rMemory:
                                if(!ScanList(ReadMem,LineNo,li.operands[0].Operand,upward)) return false;
                                break;
                            case OperandType.Register:
                                if(!ScanList(ReadReg,LineNo,li.operands[0].Operand,upward)) return false;
                                break;
                        }
                        if(upward) {
                            ModEsp(LineNo,upward,-4);
                        } else {
                            ModEsp(LineNo,upward,4);
                        }
                        break;
                    //li=new LineInfo("00000000 00 mov esp,[esp]"); break;
                    default: throw new OptimizationException("Segmenter: Cannot move instruction with one operand");
                }
            }
            if(li.operands.Length==2) {
                if(Array.IndexOf(Program.ReadOnlyInstructions,li.instruction)==-1) {
                    switch(li.operands[0].OpType) {
                        case OperandType.Register:
                            if(!ScanList(ReadReg,LineNo,li.operands[0].Register,upward)) return false;
                            break;
                        case OperandType.rMemory:
                            if(!ScanList(ReadMem,LineNo,li.operands[0].Operand,upward)) return false;
                            break;
                    }
                } else {
                    switch(li.operands[0].OpType) {
                        case OperandType.rMemory:
                            if(!ScanList(ChangedMem,LineNo,li.operands[1].Operand,upward)) return false;
                            break;
                    }
                }
                switch(li.operands[1].OpType) {
                    case OperandType.rMemory:
                        if(!ScanList(ChangedMem,LineNo,li.operands[1].Operand,upward)) return false;
                        break;
                }
            }
            return true;
        }

        public FpuSegment GetFpuStream() {
            if(Array.IndexOf(Program.DissallowedAddresses,Lines[0].address)!=-1) throw new OptimizationException("Segmenter: Blacklisted address");
            //Make a list of all the fpu instructions and their locations
            ArrayList lineNos=new ArrayList();
            ArrayList lines=new ArrayList();
            for(int i=0;i<Lines.Length;i++) {
                if(Array.IndexOf(Program.FpuInstructions,Lines[i].instruction)!=-1) {
                    if(Array.IndexOf(Program.ReadableFpuInstructions,Lines[i].instruction)==-1)
                        throw new OptimizationException("Segmenter: Found unparsable FPU instruction");
                    lineNos.Add(i);
                    lines.Add(Lines[i]);
                    AddLine(Lines[i],i);
                }
            }
            LineNos=(int[])lineNos.ToArray(typeof(int));
            FpuLines=(LineInfo[])lines.ToArray(typeof(LineInfo));
            //Move all the non fpu instructions out of the way
            ArrayList pre=new ArrayList();
            ArrayList post=new ArrayList();
            bool switched=false; int LastFpu=LineNos[0];
            for(int i=LineNos[0];i<=LineNos[LineNos.Length-1];i++) {
                FpuString+=Lines[i].ToString()+"\r\n";
                if(Array.IndexOf(Program.ReadableFpuInstructions,Lines[i].instruction)==-1) {
                    if(!switched) {
                        if(CanMoveInstruction(Lines[i],i,true)) {
                            pre.Add(Lines[i]);
                        } else if(CanMoveInstruction(Lines[i],i,false)) {
                            switched=true;
                            post.Add(Lines[i]);
                        } else {
                            throw new OptimizationException("Segmenter: Unable to separate fpu code from segment");
                        }
                    } else {
                        if(CanMoveInstruction(Lines[i],i,false)) {
                            post.Add(Lines[i]);
                        } else {
                            throw new OptimizationException("Segmenter: Unable to seperate fpu code from segment");
                        }
                    }
                } else {
                    LastFpu=i;
                }
            }
            /*
            //If necessery, make a list of instructions for testing later
            //TODO: Change to account for non 32 bit values being pushed to the stack
            ArrayList TestData=new ArrayList();
            ArrayList TestData2=new ArrayList();
            if(Program.TestPatches) {
                for(int i=0;i<Lines.Length;i++) {
                    if(Array.IndexOf(Program.ReadableFpuInstructions,Lines[i].instruction)!=-1) {
                        TestData.Add(Lines[i].ToString());
                    } else if(Lines[i].instruction=="push") {
                        TestData.Add("sub esp,0x04");
                        TestData2.Add("add esp,0x04");
                    } else if(Lines[i].instruction=="pop") {
                        TestData.Add("add esp,0x04");
                        TestData2.Add("sub esp,0x04");
                    }
                }
            }
            TestData.AddRange(TestData2);
            */
            //Get the start and end address of the segment, and exit
            int start; int end;
            start=Program.HexParse(Lines[LineNos[0]].address);
            end=Program.HexParse(Lines[LineNos[LineNos.Length-1]].address)+(Lines[LineNos[LineNos.Length-1]].code.Length/2);
            return new FpuSegment(FpuLines,pre,post,start,end);
        }

        public override string ToString() {
            string str="";
            for(int i=0;i<Lines.Length;i++) {
                str+=Lines[i].instruction+" "+Lines[i].StoredOperands+"\r\n";
            }
            return str;
        }

    }

    public struct FpuSegment {
        public int Start;
        public int End;
        public LineInfo[] Lines;
        public LineInfo[] Pre;
        public LineInfo[] Post;
        //public string[] TestData;

        public FpuSegment(LineInfo[] lines,ArrayList pre,ArrayList post,int start,int end) {
            Lines=lines;
            Pre=(LineInfo[])pre.ToArray(typeof(LineInfo));
            Post=(LineInfo[])post.ToArray(typeof(LineInfo));
            Start=start;
            End=end;
            //TestData=(string[])testData.ToArray(typeof(string));
        }

        public override string ToString() {
            string str="";
            for(int i=0;i<Pre.Length;i++) {
                str+=Pre[i].instruction+" "+Pre[i].StoredOperands+"\r\n";
            }
            for(int i=0;i<Lines.Length;i++) {
                str+=Lines[i].instruction+" "+Lines[i].StoredOperands+"\r\n";
            }
            for(int i=0;i<Post.Length;i++) {
                str+=Post[i].instruction+" "+Post[i].StoredOperands+"\r\n";
            }
            return str;
        }

        public string PreString() {
            string str="";
            for(int i=0;i<Pre.Length;i++) {
                str+=Pre[i].instruction+" "+Pre[i].StoredOperands+"\r\n";
            }
            return str;
        }

        public string PostString() {
            string str="";
            for(int i=0;i<Post.Length;i++) {
                str+=Post[i].instruction+" "+Post[i].StoredOperands+"\r\n";
            }
            return str;
        }

    }

}   
