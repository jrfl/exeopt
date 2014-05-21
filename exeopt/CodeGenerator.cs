// CodeGenerator.cs
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
using System.IO;
//Invis walls: Swap movss and movhps in v3MoveToReg
namespace Patcher {
    public struct RegisterState {
        public bool occupied;
        public VectorDirection direction;
        public byte contains;

        public RegisterState(VectorDirection vd,byte b) {
            occupied=true;
            direction=vd;
            contains=b;
        }

        public static implicit operator bool(RegisterState r) {
            return r.occupied;
        }
    }

    public class CodeGenerator {
        public static string[] ResLoc;
        public static RegisterState[] Registers;
        public static StreamWriter AsmFile;
        public static byte operation;
        public static int[] FpuLines;
        public static bool aligned;
        public static bool flipped;

        public static void Reset() {
            if(AsmFile!=null) AsmFile.Close();
            ResLoc=new string[10];
            Registers=new RegisterState[8];
            AsmFile=new StreamWriter("out.txt");
            AsmFile.AutoFlush=true;
            operation=0;
            flipped=false;
            FpuLines=null;
        }

        public static void PartialReset() {
            Registers=new RegisterState[8];
            ResLoc=new string[10];
            operation=0;
            flipped=false;
        }

        public static void FindFpuLines(VectorMatch[] function) {
            ArrayList al=new ArrayList();
            foreach(VectorMatch vm in function) {
                for(int i=0;i<vm.fpulines.Length;i++) {
                    al.AddRange(vm.fpulines[i]);
                }
            }
            al.Sort();
            //Remove duplicate references from fxch instructions
            int last=-1;
            for(int i=0;i<al.Count;i++) {
                if((int)al[i]==last) {
                    al.RemoveAt(i--);
                } else {
                    last=(int)al[i];
                }
            }
            FpuLines=(int[])al.ToArray(typeof(int));
        }

        public static void GenerateCode(ArrayList function) {
            Reset();
            AsmFile.WriteLine("bits 32");
            for(int i=0;i<function.Count;i++) {
                PartialReset();
                VectorMatch vm=(VectorMatch)function[i];
                int index=vm.Count-2;
                if(Program.Mod[index]) {
                    switch(Program.Align[index]) {
                        case 3: aligned=true; break;
                        case 2:
                            if(vm.Offset%16==0) {
                                aligned=true;
                                for(int j=0;j<vm.pArguments.Length;j++) {
                                    if(vm.pArguments[j].Direction!=VectorDirection.Static&&
                                        vm.pArguments[j].Offset%16!=0) aligned=false;
                                }
                            } else {
                                aligned=false;
                            }
                            break;
                        case 1:
                            if(vm.Offset==0) goto case 0;
                            for(int j=0;j<Program.CurrentFpu.Pre.Length;j++) {
                                if(Program.CurrentFpu.Pre[j].instruction=="push"||Program.CurrentFpu.Pre[j].instruction=="pop") {
                                    aligned=false;
                                    goto case 0;
                                }
                            }
                            goto case 2;
                        case 0: aligned=false; break;
                    }
                    switch(vm.Count) {
                        case 2: v2CreateCode(vm); break;
                        case 3: v3CreateCode(vm); break;
                        case 4: v4CreateCode(vm); break;
                    }
                } else {
                    function.RemoveAt(i--);
                }
            }
            //Copy any leftover fpu lines to the asm file so the esp pointers get updated
            if(function.Count==0) throw new OptimizationException("Code generator: Unable to generate any code");
            FindFpuLines((VectorMatch[])function.ToArray(typeof(VectorMatch)));
            for(int i=0;i<Program.CurrentFpu.Lines.Length;i++) {
                if(Array.IndexOf(FpuLines,i)==-1) {
                    AsmFile.WriteLine(Program.CurrentFpu.Lines[i].instruction+" "+Program.CurrentFpu.Lines[i].StoredOperands);
                }
            }
            //Return
            AsmFile.Close();
        }

        public static string ToHex(int i) {
            if(i==0) return "+0x00";
            //String s=Conversion.Hex(Math.Abs(i));
            string s=Math.Abs(i).ToString("x").ToUpper();
            if(i>0) { s="+0x"+s; } else { s="-0x"+s; }
            return s;
        }

        public static string MemLoc(Vector v) { return MemLoc(v,0); }
        public static string MemLoc(Vector v,int offset) {
            return "["+v.Register+ToHex(v.Offset+offset)+"]";
        }

        public static void v2CreateCode(VectorMatch function) {
            byte a=0;
            if(!function.Upward) {
                flipped=true;
                for(int i=0;i<function.pArguments.Length;i++) {
                    if(function.pArguments[i].Direction==VectorDirection.Up) {
                        function.pArguments[i].Direction=VectorDirection.Down;
                    } else if(function.pArguments[i].Direction==VectorDirection.Down) {
                        function.pArguments[i].Direction=VectorDirection.Up;
                    }
                }
            }
            while(function.calc.Length>1) {
                Operation op=DeepOp(ref function,a++);
                if(op.args==ArgsToUse.None) {
                    op.op1loc=v2MoveToRegister(op.op1);
                    op.args=ArgsToUse.Left;
                }
                if((op.args&ArgsToUse.Left)==0) {
                    op.op1loc=v2MoveToRegister(op.op1);
                    op.args|=ArgsToUse.Left;
                }
                if((op.args&ArgsToUse.Right)==0&&(!aligned||op.op2.Direction!=VectorDirection.Up)) {
                    op.op2loc=v2MoveToRegister(op.op2);
                    op.args|=ArgsToUse.Right;
                }
                Registers[op.op1loc].contains=(byte)(a-1);
                string s=null;
                switch(op.op) {
                    case '+': s="addps "; break;
                    case '-': s="subps "; break;
                    case '*': s="mulps "; break;
                    case '/': s="divps "; break;
                }
                if((op.args&ArgsToUse.Left)==0) {
                    s+=MemLoc(op.op1)+",";
                } else {
                    s+="xmm"+op.op1loc+",";
                }
                if((op.args&ArgsToUse.Right)==0) {
                    s+=MemLoc(op.op2);
                } else {
                    s+="xmm"+op.op2loc;
                }
                AsmFile.WriteLine(s);
            }
            if(!flipped) {
                AsmFile.WriteLine("movlps ["+function.Reg+ToHex(function.Offset)+"],xmm"+GetRegister(function.calc[0]));
            } else {
                AsmFile.WriteLine("movlps ["+function.Reg+ToHex(function.Offset-4)+"],xmm"+GetRegister(function.calc[0]));
            }

        }

        public static void v3CreateCode(VectorMatch function) {
            byte a=0;
            //If the function is reversed, flip all the arguments
            if(!function.Upward) {
                flipped=true;
                for(int i=0;i<function.pArguments.Length;i++) {
                    if(function.pArguments[i].Direction==VectorDirection.Up) {
                        function.pArguments[i].Direction=VectorDirection.Down;
                    } else if(function.pArguments[i].Direction==VectorDirection.Down) {
                        function.pArguments[i].Direction=VectorDirection.Up;
                    }
                }
            }
            while(function.calc.Length>1) {
                Operation op=DeepOp(ref function,a++);
                if(op.args==ArgsToUse.None) {
                    op.op1loc=v3MoveToRegister(op.op1);
                    op.args=ArgsToUse.Left;
                }
                if((op.args&ArgsToUse.Left)==0) {
                    op.op1loc=v3MoveToRegister(op.op1);
                    op.args|=ArgsToUse.Left;
                }
                if((op.args&ArgsToUse.Right)==0&&(!aligned||op.op2.Direction!=VectorDirection.Up)) {
                    op.op2loc=v3MoveToRegister(op.op2);
                    op.args|=ArgsToUse.Right;
                }
                Registers[op.op1loc].contains=(byte)(a-1);
                string s=null;
                switch(op.op) {
                    case '+': s="addps "; break;
                    case '-': s="subps "; break;
                    case '*': s="mulps "; break;
                    case '/': s="divps "; break;
                }
                if((op.args&ArgsToUse.Left)==0) {
                    s+="["+op.op1.Register+ToHex(op.op1.Offset)+"],";
                } else {
                    s+="xmm"+op.op1loc+",";
                }
                if((op.args&ArgsToUse.Right)==0) {
                    s+="["+op.op2.Register+ToHex(op.op2.Offset)+"]";
                } else {
                    s+="xmm"+op.op2loc;
                }
                AsmFile.WriteLine(s);
            }
            if(aligned) {
                AsmFile.WriteLine("movhlps xmm7,xmm"+GetRegister(function.calc[0]));      
                if(flipped) {
                    AsmFile.WriteLine("movlps ["+function.Reg+ToHex(function.Offset-4)+"],xmm"+GetRegister(function.calc[0]));
                    AsmFile.WriteLine("movss ["+function.Reg+ToHex(function.Offset-8)+"],xmm7");
                } else {
                    AsmFile.WriteLine("movlps ["+function.Reg+ToHex(function.Offset)+"],xmm"+GetRegister(function.calc[0]));
                    AsmFile.WriteLine("movss ["+function.Reg+ToHex(function.Offset+8)+"],xmm7");
                }
            } else {
                if(flipped) {
                    AsmFile.WriteLine("movhps ["+function.Reg+ToHex(function.Offset-8)+"],xmm"+GetRegister(function.calc[0]));
                } else {
                    AsmFile.WriteLine("movhps ["+function.Reg+ToHex(function.Offset+4)+"],xmm"+GetRegister(function.calc[0]));
                }
                AsmFile.WriteLine("movss ["+function.Reg+ToHex(function.Offset)+"],xmm"+GetRegister(function.calc[0]));
            }
        }

        public static void v4CreateCode(VectorMatch function) {
            byte a=0;
            while(function.calc.Length>1) {
                Operation op=DeepOp(ref function,a++);
                if(op.args==ArgsToUse.None) {
                    op.op1loc=v4MoveToRegister(op.op1);
                    op.args=ArgsToUse.Left;
                }
                if((op.args&ArgsToUse.Left)==0) {
                    op.op1loc=v4MoveToRegister(op.op1);
                    op.args|=ArgsToUse.Left;
                }
                if((op.args&ArgsToUse.Right)==0&&(!aligned||op.op2.Direction!=VectorDirection.Up)) {
                    op.op2loc=v4MoveToRegister(op.op2);
                    op.args|=ArgsToUse.Right;
                }
                Registers[op.op1loc].contains=(byte)(a-1);
                string s=null;
                switch(op.op) {
                    case '+': s="addps "; break;
                    case '-': s="subps "; break;
                    case '*': s="mulps "; break;
                    case '/': s="divps "; break;
                }
                if((op.args&ArgsToUse.Left)==0) {
                    s+="["+op.op1.Register+ToHex(op.op1.Offset)+"],";
                } else {
                    s+="xmm"+op.op1loc+",";
                }
                if((op.args&ArgsToUse.Right)==0) {
                    s+="["+op.op2.Register+ToHex(op.op2.Offset)+"]";
                } else {
                    s+="xmm"+op.op2loc;
                }
                AsmFile.WriteLine(s);
            }
            if(!function.Upward) {
                AsmFile.WriteLine("shufps xmm"+(byte)function.calc[0]+",xmm"+GetRegister(function.calc[0])+",0x1B");
            }
            if(aligned) {
                AsmFile.WriteLine("movaps ["+function.Reg+ToHex(function.Offset)+"],xmm"+GetRegister(function.calc[0]));
            } else {
                AsmFile.WriteLine("movups ["+function.Reg+ToHex(function.Offset)+"],xmm"+GetRegister(function.calc[0]));
            }
        }

        public static byte v2MoveToRegister(Vector v) {
            for(byte x=0;x<8;x++) {
                if(!Registers[x]) {
                    string r=x.ToString();
                    switch(v.Direction) {
                        case VectorDirection.Up:
                            if(flipped) {
                                AsmFile.WriteLine("movlps xmm"+r+",["+v.Register+ToHex(v.Offset-4)+"]");
                            } else {
                                AsmFile.WriteLine("movlps xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                            }
                            Registers[x]=new RegisterState(VectorDirection.Up,operation);
                            break;
                        case VectorDirection.Down:
                            throw new OptimizationException("Code generator: Code using flipped v2's doesn't fit");
                        case VectorDirection.Static:
                            AsmFile.WriteLine("movss xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                            AsmFile.WriteLine("shufps xmm"+r+",xmm"+r+",0x00");
                            Registers[x]=new RegisterState(VectorDirection.Static,operation);
                            break;
                    }
                    return x;
                }
            }
            throw new OptimizationException("Code generater: Ran out of registers");
        }

        public static byte v3MoveToRegister(Vector v) {
            for(byte x=0;x<8;x++) {
                if(!Registers[x]) {
                    string r=x.ToString();
                    switch(v.Direction) {
                        case VectorDirection.Up:
                            if(aligned) {
                                if(flipped) {
                                    AsmFile.WriteLine("movaps xmm"+r+",["+v.Register+ToHex(v.Offset-8)+"]");
                                } else {
                                    AsmFile.WriteLine("movaps xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                                }
                            } else {
                                AsmFile.WriteLine("movss xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                                if(flipped) {
                                    AsmFile.WriteLine("movhps xmm"+r+",["+v.Register+ToHex(v.Offset-8)+"]");
                                } else {
                                    AsmFile.WriteLine("movhps xmm"+r+",["+v.Register+ToHex(v.Offset+4)+"]");
                                }
                            }
                            Registers[x]=new RegisterState(VectorDirection.Up,operation);
                            break;
                        case VectorDirection.Down:
                            throw new OptimizationException("Code generator: Haven't got reversed v3 input working yet");
                        case VectorDirection.Static:
                            AsmFile.WriteLine("movss xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                            AsmFile.WriteLine("shufps xmm"+r+",xmm"+r+",0x00");
                            Registers[x]=new RegisterState(VectorDirection.Static,operation);
                            break;
                    }
                    return x;
                }
            }
            throw new OptimizationException("Code generater: Ran out of registers");
        }

        public static byte v4MoveToRegister(Vector v) {
            for(byte x=0;x<8;x++) {
                if(!Registers[x]) {
                    string r=x.ToString();
                    switch(v.Direction) {
                        case VectorDirection.Up:
                            if(aligned) {
                                AsmFile.WriteLine("movaps xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                            } else {
                                AsmFile.WriteLine("movups xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                            }
                            Registers[x]=new RegisterState(VectorDirection.Up,operation);
                            break;
                        case VectorDirection.Down:
                            AsmFile.WriteLine("movhps xmm"+r+",["+v.Register+ToHex(v.Offset-(v.Count-2)*4)+"]");
                            AsmFile.WriteLine("shufps xmm"+r+",["+v.Register+ToHex(v.Offset-v.Count*4)+"],0x1B");
                            Registers[x]=new RegisterState(VectorDirection.Down,operation);
                            break;
                        case VectorDirection.Static:
                            AsmFile.WriteLine("movss xmm"+r+",["+v.Register+ToHex(v.Offset)+"]");
                            AsmFile.WriteLine("shufps xmm"+r+",xmm"+r+",0x00");
                            Registers[x]=new RegisterState(VectorDirection.Static,operation);
                            break;
                    }
                    return x;
                }
            }
            throw new OptimizationException("Code generater: Ran out of registers");
        }

        public static byte GetRegister(char c) {
            for(byte i=0;i<8;i++) {
                if(Registers[i].contains==(byte)c) return i;
            }
            throw new OptimizationException("Code generator: Using unoccupied register");
        }

        public static Operation DeepOp(ref VectorMatch vm,byte depth) {
            if(depth>254) throw new OptimizationException("Code generater: Too deep recursion");
            int count=0; int maxcount=0; int pos=0;
            string s=vm.calc;
            for(int i=0;i<s.Length;i++) {
                switch(s[i]) {
                    case '(':
                        count++;
                        if(count>maxcount) maxcount=count;
                        break;
                    case ')': count--; break;
                }
            }
            for(int i=0;i<s.Length;i++) {
                switch(s[i]) {
                    case '(':
                        count++;
                        if(count==maxcount) {
                            ArgsToUse atu=ArgsToUse.None;
                            int b=2;
                            if(s[i+1]!='$') { atu|=ArgsToUse.Left; b--; }
                            if(s[i+3]!='$') { atu|=ArgsToUse.Right; b--; }
                            Operation o=new Operation();
                            switch(atu) {
                                case ArgsToUse.None:
                                    o=new Operation(vm.pArguments[pos],vm.pArguments[pos+1],s[i+2]);
                                    break;
                                case ArgsToUse.Left:
                                    o=new Operation(GetRegister(s[i+1]),vm.pArguments[pos],s[i+2]);
                                    //Registers[GetRegister(s[i+1])].contains=depth;
                                    break;
                                case ArgsToUse.Right:
                                    o=new Operation(vm.pArguments[pos],GetRegister(s[i+3]),s[i+2]);
                                    break;
                                case ArgsToUse.Both:
                                    o=new Operation(GetRegister(s[i+3]),GetRegister(s[i+1]),s[i+2]);
                                    //Registers[GetRegister(s[i+1])].contains=depth;
                                    break;
                            }
                            Vector[] vs=new Vector[vm.pArguments.Length-b];
                            int c=0;
                            for(int a=0;a<vm.pArguments.Length;a++) {
                                if(a==c&&b>0) continue;
                                if(a==c+1&&b>1) continue;
                                vs[c++]=vm.pArguments[a];
                            }
                            vm.pArguments=vs;
                            vm.calc=s.Remove(i,5);
                            vm.calc=vm.calc.Insert(i,""+(char)depth);
                            return o;
                        }
                        break;
                    case ')': count--; break;
                    case '$':
                        pos++;
                        break;
                }
            }
            throw new OptimizationException("Code generater: No operation to perform");
        }
    }
}
