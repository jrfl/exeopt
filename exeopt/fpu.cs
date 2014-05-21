// fpu.cs
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
    public class fpureg {
        public string[] reg=new string[8];
        public byte size=0;

        public void Push(string value,string LineNo) {
            if(size++==8) throw new OptimizationException("FPU: stack overflow error.");
            
            for(int i=7;i>0;i--) {
                reg[i]=reg[i-1];
            }
            int index=value.IndexOf("st");
            if(index!=-1) {
                throw new OptimizationException("FPU: Trying to push an fpu register to the fpu stack");
                //int i=Convert.ToByte(""+value[index+2]);
                //if(i==7) i=-1;    //Because it would have looped round and assigned st0 to itself
                //reg[0]=LineNo+reg[i+1];
            } else {
                reg[0]=LineNo+value;
            }
        }

        public void Assign(byte RegNo,string value,string LineNo) {
            if(value.StartsWith("st")) {
                int i=Convert.ToByte(""+value[2]);
                reg[RegNo]=LineNo+reg[i+1];
            } else {
                reg[RegNo]=LineNo+value;
            }
        }

        public void Exchange(string source,string lineno) {
            string s=reg[0];
            int i=Convert.ToInt32(""+source[2]);
            reg[0]=lineno+reg[i];
            reg[i]=lineno+s;
        }

        /*public void SafePop() {
            if(size--==0) {
                size++;
            } else {
                for(int i=0;i<7;i++) {
                    reg[i]=reg[i+1];
                }
            }
        }*/

        public string Pop() {
            if(size--==0) throw new OptimizationException("FPU: stack underflow.");
            string s=reg[0];
            for(int i=0;i<7;i++) {
                reg[i]=reg[i+1];
            }
            return s;
        }

        public string Peek() {
            //TODO: Check if the '--' below is supposed to be there
            if(size==0) throw new OptimizationException("FPU: stack underflow.");
            return reg[0];
        }

        public string Peek(byte RegNo) {
            if(size<=RegNo) throw new OptimizationException("FPU: Partial stack underflow.");
            return reg[RegNo];
        }

        public void Op(string op,string dest) {
            if(dest.StartsWith("st")) {
                int i=Convert.ToByte(""+dest[2]);
                if(size<=i) throw new OptimizationException("FPU: using uninitialized register");
                reg[0]="("+reg[0]+op+reg[i]+")";
            } else {
                if(size==0) throw new OptimizationException("FPU: using uninitialized register (0)");
                reg[0]="("+reg[0]+op+dest+")";
            }
        }

        public void ToOp(string op,string dest) {
            if(dest.StartsWith("st")) {
                int i=Convert.ToByte(""+dest[2]);
                if(size<=i) throw new OptimizationException("FPU: using uninitialized register");
                reg[i]="("+reg[i]+op+reg[0]+")";
            } else {
                throw new OptimizationException("FPU: Cannot apply TO modifier to memory address");
            }
        }

        public void ROp(string op,string dest) {
            if(dest.StartsWith("st")) {
                int i=Convert.ToByte(""+dest[2]);
                if(size<=i) throw new OptimizationException("FPU: using uninitialized register");
                reg[0]="("+reg[i]+op+reg[0]+")";
            } else {
                if(size==0) throw new OptimizationException("FPU: using uninitialized register (0)");
                fpu.Results.Add(reg[0]+"="+dest+op+reg[0]);
            }
        }

        public void RToOp(string op,string dest) {
            if(dest.StartsWith("st")) {
                int i=Convert.ToByte(""+dest[2]);
                if(size<=i) throw new OptimizationException("FPU: using uninitialized register");
                reg[i]="("+reg[0]+op+reg[i]+")";
            } else {
                throw new OptimizationException("FPU: Cannot apply TO modifier to memory address");
            }
        }

    }

    public class fpu {
        private static fpureg Registers;
        public static ArrayList Results;

        public static string[] Result {
            get {
                foreach(string s in Results) {
                    if(s.IndexOf("st")!=-1) throw new OptimizationException("FPU: Unresolved registers");
                }
                if(Results.Count==0) throw new OptimizationException("FPU: operation wasn't saved.");
                return (string[])Results.ToArray(typeof(string));
            }
        }

        public static int[][] ResultLines;

        public static void Reset() {
            Registers=new fpureg();
            Results=new ArrayList();
        }

        public static LineInfo CurrentOp;
        public static void PerformOp(LineInfo op,byte LineNo) {
            string l="\0"+LineNo.ToString().PadLeft(10,'0');
            CurrentOp=op;
            switch(op.instruction) {
                case "fld": Registers.Push(loc(),l); break;
                case "fst":
                    if(CurrentOp.operands[0].OpType==OperandType.Register) {
                        byte b=Convert.ToByte(""+CurrentOp.operands[0].Register[2]);
                        Registers.Assign(b,Registers.Peek(),l);
                    } else {
                        Results.Add(l+loc()+"="+Registers.Peek());
                    }
                    break;
                case "fstp":
                    //This is backwards! Assign first, then pop!
                    if(CurrentOp.operands[0].OpType==OperandType.Register) {
                        byte b=Convert.ToByte(""+CurrentOp.operands[0].Register[2]);
                        Registers.Assign(b,Registers.Peek(),l);
                        Registers.Pop();
                    } else {
                        Results.Add(l+loc()+"="+Registers.Pop());
                    }
                    break;
                case "fadd": Registers.Op(l+"+",loc()); break;
                case "faddp": Registers.ToOp(l+"+",loc()); Registers.Pop(); break;
                case "fmul": Registers.Op(l+"*",loc()); break;
                case "fmulp": Registers.ToOp(l+"*",loc()); Registers.Pop(); break;
                case "fdiv": Registers.Op(l+"/",loc()); break;
                case "fdivp": Registers.ToOp(l+"/",loc()); Registers.Pop(); break;
                case "fdivr": Registers.ROp(l+"/",loc()); break;
                case "fdivrp": Registers.RToOp(l+"/",loc()); Registers.Pop(); break;
                case "fsub": Registers.Op(l+"-",loc()); break;
                case "fsubp": Registers.ToOp(l+"-",loc()); Registers.Pop(); break;
                case "fsubr": Registers.ROp(l+"-",loc()); break;
                case "fsubrp": Registers.RToOp(l+"-",loc()); Registers.Pop(); break;
                case "fxch": Registers.Exchange(loc(),l); break;
                default: throw new OptimizationException("FPU: Unrecognised operation");
            }
        }

        public static string loc() {
            return CurrentOp.operands[0].Operand;
        }

        public static string EditResult(string result) {
            bool found=false;
            string s="";
            int start=0;
            for(int i=0;i<result.Length;i++) {
                switch(result[i]) {
                    case '[': found=true; start=i+1; s=""; break;
                    case ']':
                        int a=s.Length;
                        if(s.Length==3) {
                            s+="+0x0000";
                        }
                        if(s.Length==7) {
                            s=s.Insert(6,"0");
                        }
                        if(s.Length==8) {
                            s=s.Insert(6,"0");
                        }
                        if(s.Length==9) {
                            s=s.Insert(6,"0");
                        }
                        result=result.Remove(start,a).Insert(start,s);
                        found=false;
                        break;
                    default:
                        if(found) {
                            s+=result[i];
                        }
                        break;
                }
            }
            return result;
        }

        public static void Process() {
            if(Registers.size>0) throw new OptimizationException("FPU: Leftovers in the registers");
            ResultLines=new int[Result.Length][];
            for(int j=0;j<Results.Count;j++) {
                ArrayList al=new ArrayList();
                string result=(string)Results[j];
                for(int i=0;i<result.Length;i++) {
                    if(result[i]=='\0') {
                        al.Add(Convert.ToInt32(result.Substring(i+1,10)));
                        result=result.Remove(i--,11);
                    }
                }
                result=EditResult(result);
                Results[j]=result;
                ResultLines[j]=(int[])al.ToArray(typeof(int));
            }
        }

    }
}
