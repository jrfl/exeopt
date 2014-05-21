// vectorizer.cs
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

    public class vectorizer {

        public static ArrayList elements;
        public static ArrayList FoundVectors;

        public static int Offset;
        public static int ExpectedOffset;
        public static bool Upward;
        public static int VectorStart;
        public static string Reg;

        public static void Reset() {
            elements=new ArrayList();
            FoundVectors=new ArrayList();
            Offset=-1;
            ExpectedOffset=-1;
        }

        public static void AddLine(string line,int[] fpulines) {
            if(line.IndexOf('(')==-1) throw new OptimizationException("Vectorizer: Direct assignment");
            elements.Add(new UnwrappedElement(line,fpulines));
        }

        public static VectorMatch[] Process() {
            if(elements.Count<2) return null;
            for(int i=0;i<elements.Count;i++) {
                ExtractVector(i);
            }
            if(Offset!=-1&&ExpectedOffset!=-1) {
                VectorMatchSplit(new VectorMatch(VectorStart,elements.Count-VectorStart,Upward,Offset,Reg));
            }
            ProcessMatches();
            if(FoundVectors.Count>0) return (VectorMatch[])FoundVectors.ToArray(typeof(VectorMatch));
            return null;
        }

        private static void ProcessMatches() {
            for(int j=0;j<10;j++) {
                for(int i=0;i<FoundVectors.Count;i++) {
                    if(((VectorMatch)FoundVectors[i]).arguments[0].Length>j) {
                        VectorMatch[] vms=ProcessSplit((VectorMatch)FoundVectors[i],j);
                        FoundVectors.RemoveAt(i);
                        FoundVectors.InsertRange(i,vms);
                        i+=vms.Length-1;
                    }
                }
            }
            //If arg1 is ascending and arg2 isn't then swap them round. Makes the code generater more efficent
            for(int i=0;i<FoundVectors.Count;i++) {
                int a=0;
                for(int j=0;j<((VectorMatch)FoundVectors[i]).calc.Length-2;j++) {
                    switch(((VectorMatch)FoundVectors[i]).calc[j]) {
                        case '$':
                            if(((VectorMatch)FoundVectors[i]).calc[j+2]=='$') {
                                if(((VectorMatch)FoundVectors[i]).pArguments[a].Direction==VectorDirection.Up&&
                                   ((VectorMatch)FoundVectors[i]).pArguments[a+1].Direction!=VectorDirection.Up&&
                                  (((VectorMatch)FoundVectors[i]).calc[j+1]=='+'||
                                   ((VectorMatch)FoundVectors[i]).calc[j+1]=='*')) {
                                    Vector v=((VectorMatch)FoundVectors[i]).pArguments[a];
                                    ((VectorMatch)FoundVectors[i]).pArguments[a]=((VectorMatch)FoundVectors[i]).pArguments[a+1];
                                    ((VectorMatch)FoundVectors[i]).pArguments[a+1]=v;
                                }
                            }
                            a++;
                            break;
                    }
                }
            }
        }

        private static VectorMatch[] ProcessSplit(VectorMatch vm,int arg) {
            int Offset=-1;
            int ExpectedOffset=-1;
            string Reg="";
            int VectorStart=0;
            VectorDirection dir=VectorDirection.Unknown;
            ArrayList matches=new ArrayList();
            for(int i=0;i<vm.arguments.Length;i++) {
                if(i==0||Offset==-1) {
                    Offset=vm.arguments[i][arg].Offset;
                    Reg=vm.arguments[i][arg].Register;
                    VectorStart=i;
                } else if(ExpectedOffset==-1) {
                    if(vm.arguments[i][arg].Register==Reg&&vm.arguments[i][arg].Offset==Offset+4) {
                        ExpectedOffset=vm.arguments[i][arg].Offset+4;
                        dir=VectorDirection.Up;
                    } else if(vm.arguments[i][arg].Register==Reg&&vm.arguments[i][arg].Offset==Offset-4) {
                        ExpectedOffset=vm.arguments[i][arg].Offset-4;
                        dir=VectorDirection.Down;
                    } else if(vm.arguments[i][arg].Register==Reg&&vm.arguments[i][arg].Offset==Offset) {
                        ExpectedOffset=vm.arguments[i][arg].Offset;
                        dir=VectorDirection.Static;
                    } else {
                        Offset=vm.arguments[i][arg].Offset;
                        VectorStart=i;
                        Reg=vm.arguments[i][arg].Register;
                        ExpectedOffset=-1;
                    }
                } else {
                    if((!(i-VectorStart==4))&&vm.arguments[i][arg].Register==Reg&&ExpectedOffset==vm.arguments[i][arg].Offset) {
                        if(dir==VectorDirection.Up) {
                            ExpectedOffset+=4;
                        } else if(dir==VectorDirection.Down) {
                            ExpectedOffset-=4;
                        }
                    } else {
                        matches.Add(new VectorMatch(vm,arg,VectorStart,i-VectorStart,dir));
                        Offset=vm.arguments[i][arg].Offset;
                        VectorStart=i;
                        Reg=vm.arguments[i][arg].Register;
                        ExpectedOffset=-1;
                    }
                }
            }
            if(Offset!=-1&&ExpectedOffset!=-1) {
                matches.Add(new VectorMatch(vm,arg,VectorStart,vm.arguments.Length-VectorStart,dir));
            }
            return (VectorMatch[])matches.ToArray(typeof(VectorMatch));
        }

        public static void ExtractVector(int i) {
            if(i==0||Offset==-1) {
                Offset=((UnwrappedElement)elements[i]).Offset;
                Reg=((UnwrappedElement)elements[i]).Register;
                VectorStart=i;
                ExpectedOffset=-1;
            } else if(ExpectedOffset==-1) {
                if(((UnwrappedElement)elements[i]).Register==Reg&&((UnwrappedElement)elements[i]).Offset==Offset+4) {
                    ExpectedOffset=((UnwrappedElement)elements[i]).Offset+4;
                    Upward=true;
                } else if(((UnwrappedElement)elements[i]).Register==Reg&&((UnwrappedElement)elements[i]).Offset==Offset-4) {
                    ExpectedOffset=((UnwrappedElement)elements[i]).Offset-4;
                    Upward=false;
                } else {
                    Offset=((UnwrappedElement)elements[i]).Offset;
                    Reg=((UnwrappedElement)elements[i]).Register;
                    VectorStart=i;
                    ExpectedOffset=-1;
                }
            } else {
                if(((UnwrappedElement)elements[i]).Register==Reg&&ExpectedOffset==((UnwrappedElement)elements[i]).Offset) {
                    if(Upward) {
                        ExpectedOffset+=4;
                    } else {
                        ExpectedOffset-=4;
                    }
                } else {
                    VectorMatchSplit(new VectorMatch(VectorStart,i-VectorStart,Upward,Offset,Reg));
                    Offset=((UnwrappedElement)elements[i]).Offset;
                    Reg=((UnwrappedElement)elements[i]).Register;
                    VectorStart=i;
                    ExpectedOffset=-1;
                }
            }
        }

        private static void VectorMatchSplit(VectorMatch vm) {
            int tempofset=vm.Offset;
            string[] calcs=new string[vm.Count];
            string[] rcalcs=new string[vm.Count];
            //Generate calcs and rcalcs
            for(int i=0;i<vm.Count;i++) {
                calcs[i]=ParseScalarCalc(((UnwrappedElement)elements[vm.Start+i]).stuff);
                rcalcs[i]=ParseScalarCalcWithReg(((UnwrappedElement)elements[vm.Start+i]).stuff);
            }
            //Rotate the registers to try and match up rcalcs
            RotateUnmatchedRegisters(ref rcalcs[0],(UnwrappedElement)elements[vm.Start]);
            for(int i=1;i<vm.Count;i++) {
                if(calcs[i]==calcs[i-1]) {
                    if(rcalcs[i]!=rcalcs[i-1]) {
                        if(!RotateDeepRegisters(ref rcalcs[i],rcalcs[i-1],(UnwrappedElement)elements[vm.Start+i])) {
                            RotateUnmatchedRegisters(ref rcalcs[i],(UnwrappedElement)elements[vm.Start+i]);
                        }
                    }
                }
            }
            //Split vectors wherever rcalcs dont match up
            int start=0;
            for(int i=1;i<vm.Count;i++) {
                if(rcalcs[i]!=rcalcs[i-1]) {
                    if(i>start+1) {
                        UnwrappedElement[] o=new UnwrappedElement[i-start+1];
                        elements.CopyTo(start+vm.Start,o,0,i-start+1);
                        FoundVectors.Add(new VectorMatch(start+vm.Start,i-start+1,vm.Upward,o,vm.Offset,vm.Reg));  
                    }
                    start=i;
                    Offset=((UnwrappedElement)elements[i+vm.Start]).Offset;
                }
            }
            if(start<vm.Count-1) {
                UnwrappedElement[] o=new UnwrappedElement[vm.Count-start];
                elements.CopyTo(start+vm.Start,o,0,vm.Count-start);
                FoundVectors.Add(new VectorMatch(start+vm.Start,vm.Count-start,vm.Upward,o,Offset,vm.Reg));
            }
        }

        private static void RotateUnmatchedRegisters(ref string s,UnwrappedElement el) {
            int a=0;
            char reg='\0';
            for(int i=0;i<s.Length;i++) {
                if(s[i]=='('&&s[i+1]!='(') {
                    reg=s[i+1];
                    break;
                }
            }
            for(int i=0;i<s.Length-2;i++) {
                if(s[i]=='(') {
                    while(el.stuff[a++]!='(') { }
                }
                if(s[i]!='('&&s[i]!=')'&&s[i]!=reg&&(s[i+1]=='*'||s[i+1]=='+')&&s[i+2]==reg) {
                    string s2=s;
                    s2=s2.Remove(i,1);
                    s2=s2.Insert(i,""+s[i+2]);
                    s2=s2.Remove(i+2,1);
                    s2=s2.Insert(i+2,""+s[i]);
                        s=s2;
                        s2="";
                        string s3="";
                        string s4="";
                        int b; int c;
                        b=a;
                        while(el.stuff[a]!=']') {
                            s2+=el.stuff[a++];
                        }
                        c=a;
                        s2+=']';
                        s4+=el.stuff[a++];
                        s4+=el.stuff[a++];
                        while(el.stuff[a]!=']') {
                            s3+=el.stuff[a++];
                        }
                        a++;
                        el.stuff=el.stuff.Remove(b,a-b);
                        el.stuff=el.stuff.Insert(b,s3+s4+s2);
                }
            }
        }

        private static bool RotateDeepRegisters(ref string s,string t,UnwrappedElement el) {
            int a=0;
            for(int i=0;i<s.Length-2;i++) {
                if(s[i]=='(') {
                    while(el.stuff[a++]!='(') { }
                }
                if(s[i]!='('&&s[i]!=')'&&(s[i+1]=='*'||s[i+1]=='+')&&s[i+2]!='('&&s[i+2]!=')') {
                    string s2=s;
                    s2=s2.Remove(i,1);
                    s2=s2.Insert(i,""+s[i+2]);
                    s2=s2.Remove(i+2,1);
                    s2=s2.Insert(i+2,""+s[i]);
                    if(s2[i]==t[i]&&s2[i+2]==t[i+2]) {
                        s=s2;
                        s2="";
                        string s3="";
                        string s4="";
                        int b; int c;
                        b=a;
                        while(el.stuff[a]!=']') {
                            s2+=el.stuff[a++];
                        }
                        c=a;
                        s2+=']';
                        s4+=el.stuff[a++];
                        s4+=el.stuff[a++];
                        while(el.stuff[a]!=']') {
                            s3+=el.stuff[a++];
                        }
                        a++;
                        el.stuff=el.stuff.Remove(b,a-b);
                        el.stuff=el.stuff.Insert(b,s3+s4+s2);
                    }
                }
            }
            return s==t;
        }

        public static string ParseMarkedScalarCalc(string calc) {
            string s="";
            bool c=true;
            for(int i=0;i<calc.Length;i++) {
                switch(calc[i]) {
                    case '[': c=false; s+='$'; break;
                    case ']': c=true; break;
                    default:
                        if(c) s+=calc[i]; 
                        break;
                }
            }
            return s;
        }

        private static string ParseScalarCalc(string calc) {
            string s="";
            bool c=true;
            for(int i=0;i<calc.Length;i++) {
                switch(calc[i]) {
                    case '[': c=false; break;
                    case ']': c=true; break;
                    default:
                        if(c) s+=calc[i]; 
                        break;
                }
            }
            return s;
        }

        private static string ParseScalarCalcWithRegAndOffset(string calc) {
            string s="";
            string r="";
            bool c=true; bool d=false;
            for(int i=0;i<calc.Length;i++) {
                switch(calc[i]) {
                    case '[': c=false; d=true; break;
                    case ']': c=true; break;
                    default:
                        if(c) s+=calc[i];
                        if(d) {
                            r+=calc[i];
                            switch(r) {
                                case "eax": s+='a'; r=""; break;
                                case "ebx": s+='b'; r=""; break;
                                case "ecx": s+='c'; r=""; break;
                                case "edx": s+='d'; r=""; break;
                                case "esi": s+='e'; r=""; break;
                                case "edi": s+='f'; r=""; break;
                                case "ebp": s+='g'; r=""; break;
                                case "esp": s+='h'; r=""; break;
                                case "+0x": case "-0x":
                                    s+=calc[i+3];
                                    d=false;
                                    r="";
                                    break;
                            }
                        }
                        break;
                }
            }
            /*while(result.IndexOf("((")!=-1) {
                result=result.Replace("((","(");
            }
            while(result.IndexOf("))")!=-1) {
                result=result.Replace("))",")");
            }*/
            return s;
        }

        private static string ParseScalarCalcWithReg(string calc) {
            string s="";
            string r="";
            bool c=true; bool d=false;
            for(int i=0;i<calc.Length;i++) {
                switch(calc[i]) {
                    case '[': c=false; d=true; break;
                    case ']': c=true; break;
                    default:
                        if(c) s+=calc[i];
                        if(d) r+=calc[i];
                        switch(r) {
                            case "eax": s+='a'; d=false; r=""; break;
                            case "ebx": s+='b'; d=false; r=""; break;
                            case "ecx": s+='c'; d=false; r=""; break;
                            case "edx": s+='d'; d=false; r=""; break;
                            case "esi": s+='e'; d=false; r=""; break;
                            case "edi": s+='f'; d=false; r=""; break;
                            case "ebp": s+='g'; d=false; r=""; break;
                            case "esp": s+='h'; d=false; r=""; break;
                        }
                        break;
                }
            }
            return s;
        }
    }
}
