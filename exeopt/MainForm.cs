// MainForm.cs
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

#if !useconsole
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using ArrayList=System.Collections.ArrayList;
using FileVersionInfo=System.Diagnostics.FileVersionInfo;
using Microsoft.Win32;

namespace Patcher {
    public class MainForm : System.Windows.Forms.Form {
        private System.Windows.Forms.OpenFileDialog OpenFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bFile;
        private System.Windows.Forms.TabPage tpMain;
        internal System.Windows.Forms.TextBox tbBlacklist;
        private System.Windows.Forms.Button bCreateConfig;
        private System.Windows.Forms.Button bMorrowind;
        private System.Windows.Forms.TabPage tpSegmenter;
        internal System.Windows.Forms.CheckBox cbBenchmark;
        internal System.Windows.Forms.TextBox tbCodeSize;
        internal System.Windows.Forms.CheckBox cbPrompts;
        internal System.Windows.Forms.TextBox tbBias;
        private System.Windows.Forms.ContextMenu DudMenu;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bFolder;
        internal System.Windows.Forms.CheckBox cbModv3s;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.CheckBox cbLogs;
        private System.Windows.Forms.TabPage tpBench;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.OpenFileDialog OpenPatch;
        internal System.Windows.Forms.ComboBox cbAlignv2s;
        internal System.Windows.Forms.ComboBox cbAlignv4s;
        internal System.Windows.Forms.TextBox tbLastPatch;
        private System.Windows.Forms.TabPage tpCode;
        internal System.Windows.Forms.ComboBox cbLoops;
        private System.Windows.Forms.Button bPatch;
        internal System.Windows.Forms.CheckBox cbModv2s;
        internal System.Windows.Forms.TextBox tbOffset;
        private System.Windows.Forms.Label lVersion;
        private System.Windows.Forms.TabPage tpMisc;
        private System.Windows.Forms.Label lMorrowindVersion;
        internal System.Windows.Forms.ComboBox cbAlignv3s;
        private System.Windows.Forms.FolderBrowserDialog OpenFolder;
        internal System.Windows.Forms.TextBox tbFirstPatch;
        internal System.Windows.Forms.CheckBox cbRestrict;
        internal System.Windows.Forms.CheckBox cbModv4s;
        internal System.Windows.Forms.CheckBox cbDiscard;
        private System.Windows.Forms.CheckBox cbSave;
        internal System.Windows.Forms.CheckBox cbBackups;
        internal System.Windows.Forms.CheckBox cbReadHeader;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Label label3;
        internal CheckBox cbWin98;
        internal CheckBox cbAutoDelete;

        /// <summary>
        /// Version string that's displayed in the main window
        /// </summary>
        public const string version="v1.7";
        /// <summary>
        /// Cant remember what this one does
        /// </summary>
        public const string sVersion="d";
        /// <summary>
        /// True to display warning prompts. Errors are always displayed
        /// </summary>
        public static bool ShowPrompts=true;
        /// <summary>
        /// True when in the middle of patching something
        /// </summary>
        public static bool PatchInProgress=false;
        /// <summary>
        /// For other classes to access this form
        /// </summary>
        public static MainForm mainform;

        public static bool MorrowindInstalled=false;
        public static string MorrowindPath=null;
        public static bool MorrowindLatestVersion=false;

        public static string[] ExecutableExtensions={ ".exe",".dll",".scr" };
        public static char[] Numbers={'1','2','3','4','5','6','7','8','9','0','-',
	        (char)8};
        public static char[] Numbers2={ '1','2','3','4','5','6','7','8','9','0',(char)8 };
        public static char[] Hex={'0','1','2','3','4','5','6','7','8','9','A',
	        'B','C','D','E','F','a','b','c','d','e','f',(char)8,(char)13};
        private Button bFindInstall;
        private Button bHelp;
        
        public static char[] Hex2={ '0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F' };

        //private static byte[] MorrowindNocdMD5= { 172,252,170,234,11,184,94,254,190,35,82,235,117,170,153,249 };

        public MainForm() {
            InitializeComponent();
            lVersion.Text="Exe optimizer "+version;
            this.Text="Exe optimizer";
            mainform=this;
            PatchDialog.mainform=this;
            //Load settings
            if(File.Exists("ExeOpt.ini")) {
                BinaryReader br=new BinaryReader(File.OpenRead("ExeOpt.ini"));
                if(br.ReadString()!=sVersion) {
                    br.Close();
                    try { File.Delete("ExeOpt.ini"); } catch { }
                    return;
                }
                tbBlacklist.Text=br.ReadString();
                cbBenchmark.Checked=br.ReadBoolean();
                cbDiscard.Checked=br.ReadBoolean();
                cbWin98.Checked=br.ReadBoolean();
                tbBias.Text=br.ReadString();
                cbModv2s.Checked=br.ReadBoolean();
                cbModv3s.Checked=br.ReadBoolean();
                cbModv4s.Checked=br.ReadBoolean();
                cbAlignv2s.Text=br.ReadString();
                cbAlignv3s.Text=br.ReadString();
                cbAlignv4s.Text=br.ReadString();
                cbBackups.Checked=br.ReadBoolean();
                cbLogs.Checked=br.ReadBoolean();
                cbPrompts.Checked=br.ReadBoolean();
                cbSave.Checked=br.ReadBoolean();
                cbLoops.Text=br.ReadString();
                cbReadHeader.Checked=br.ReadBoolean();
                tbOffset.Text=br.ReadString();
                tbCodeSize.Text=br.ReadString();
                cbRestrict.Checked=br.ReadBoolean();
                tbFirstPatch.Text=br.ReadString();
                tbLastPatch.Text=br.ReadString();
                MorrowindPath=br.ReadString();
                if(MorrowindPath=="null"||!File.Exists(MorrowindPath+"\\Morrowind.exe")) {
                    MorrowindPath=null;
                    MorrowindInstalled=false;
                } else {
                    MorrowindInstalled=true;
                    lMorrowindVersion.Text="Morrowind installation found in:\n   "+MorrowindPath;
                }
                br.Close();
            }
            //Load advanced settings
            if(File.Exists("config.ini")) {
                StreamReader sr=new StreamReader(File.OpenRead("config.ini"));
                ArrayList list=new ArrayList();
                string s=";";
                while(s[0]!='[') {
                    s=sr.ReadLine().Trim().ToLower();
                    if(s==""||s[0]==';') continue;
                    if(s[0]!='[') list.Add(s);
                    else {
                        string[] lines=(string[])list.ToArray(typeof(string));
                        list.Clear();
                        switch(s) {
                            case "[registers]":
                                Program.Registers=lines;
                                break;
                            case "[jump instructions]":
                                Program.JumpInstructions=lines;
                                break;
                            case "[fpu instructions]":
                                Program.FpuInstructions=lines;
                                break;
                            case "[parsable fpu instructions]":
                                Program.ReadableFpuInstructions=lines;
                                break;
                            case "[fpu ignore]":
                                Program.FpuIgnoreInstructions=lines;
                                break;
                            case "[fpu read]":
                                Program.FpuReadInstructions=lines;
                                break;
                            case "[fpu write]":
                                Program.FpuWriteInstructions=lines;
                                break;
                            case "[read only instructions]":
                                Program.ReadOnlyInstructions=lines;
                                break;
                        }
                    }
                }
            } else {
                if(Environment.OSVersion.Platform==PlatformID.Win32NT) {
                    cbWin98.Checked=false;
                } else {
                    cbWin98.Checked=true;
                }
            }
            //Get morrowind installation directory
            if(!MorrowindInstalled) {
                RegistryKey rk=Registry.LocalMachine.OpenSubKey("Software\\Bethesda softworks\\morrowind");
                string mPath;
                if(rk==null||(mPath=(string)rk.GetValue("Installed Path",null))==null||
                    !File.Exists(mPath+"\\Morrowind.exe")) {
                    MorrowindInstalled=false;
                    MorrowindPath=null;
                    lMorrowindVersion.Text="Morrowind not installed";
                } else {
                    MorrowindInstalled=true;
                    MorrowindPath=mPath;
                    lMorrowindVersion.Text="Morrowind installation found in:\n   "+MorrowindPath;
                }
            }
        }

        #region Windows Forms Designer generated code
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent() {
            this.label3=new System.Windows.Forms.Label();
            this.tabControl=new System.Windows.Forms.TabControl();
            this.tpMain=new System.Windows.Forms.TabPage();
            this.bFindInstall=new System.Windows.Forms.Button();
            this.lMorrowindVersion=new System.Windows.Forms.Label();
            this.lVersion=new System.Windows.Forms.Label();
            this.bFolder=new System.Windows.Forms.Button();
            this.bFile=new System.Windows.Forms.Button();
            this.bMorrowind=new System.Windows.Forms.Button();
            this.tpCode=new System.Windows.Forms.TabPage();
            this.tbLastPatch=new System.Windows.Forms.TextBox();
            this.DudMenu=new System.Windows.Forms.ContextMenu();
            this.label9=new System.Windows.Forms.Label();
            this.tbFirstPatch=new System.Windows.Forms.TextBox();
            this.cbRestrict=new System.Windows.Forms.CheckBox();
            this.label2=new System.Windows.Forms.Label();
            this.label1=new System.Windows.Forms.Label();
            this.cbAlignv2s=new System.Windows.Forms.ComboBox();
            this.cbAlignv3s=new System.Windows.Forms.ComboBox();
            this.cbAlignv4s=new System.Windows.Forms.ComboBox();
            this.cbModv4s=new System.Windows.Forms.CheckBox();
            this.cbModv3s=new System.Windows.Forms.CheckBox();
            this.cbModv2s=new System.Windows.Forms.CheckBox();
            this.tpBench=new System.Windows.Forms.TabPage();
            this.cbWin98=new System.Windows.Forms.CheckBox();
            this.label6=new System.Windows.Forms.Label();
            this.cbLoops=new System.Windows.Forms.ComboBox();
            this.label4=new System.Windows.Forms.Label();
            this.tbBias=new System.Windows.Forms.TextBox();
            this.cbDiscard=new System.Windows.Forms.CheckBox();
            this.cbBenchmark=new System.Windows.Forms.CheckBox();
            this.tpSegmenter=new System.Windows.Forms.TabPage();
            this.label8=new System.Windows.Forms.Label();
            this.label7=new System.Windows.Forms.Label();
            this.tbCodeSize=new System.Windows.Forms.TextBox();
            this.tbOffset=new System.Windows.Forms.TextBox();
            this.cbReadHeader=new System.Windows.Forms.CheckBox();
            this.label5=new System.Windows.Forms.Label();
            this.tbBlacklist=new System.Windows.Forms.TextBox();
            this.tpMisc=new System.Windows.Forms.TabPage();
            this.bHelp=new System.Windows.Forms.Button();
            this.bCreateConfig=new System.Windows.Forms.Button();
            this.bPatch=new System.Windows.Forms.Button();
            this.cbSave=new System.Windows.Forms.CheckBox();
            this.cbPrompts=new System.Windows.Forms.CheckBox();
            this.cbLogs=new System.Windows.Forms.CheckBox();
            this.cbBackups=new System.Windows.Forms.CheckBox();
            this.OpenFolder=new System.Windows.Forms.FolderBrowserDialog();
            this.OpenPatch=new System.Windows.Forms.OpenFileDialog();
            this.OpenFile=new System.Windows.Forms.OpenFileDialog();
            this.cbAutoDelete=new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.tpMain.SuspendLayout();
            this.tpCode.SuspendLayout();
            this.tpBench.SuspendLayout();
            this.tpSegmenter.SuspendLayout();
            this.tpMisc.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location=new System.Drawing.Point(192,56);
            this.label3.Name="label3";
            this.label3.Size=new System.Drawing.Size(56,16);
            this.label3.TabIndex=9;
            this.label3.Text="Align v4s";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tpMain);
            this.tabControl.Controls.Add(this.tpCode);
            this.tabControl.Controls.Add(this.tpBench);
            this.tabControl.Controls.Add(this.tpSegmenter);
            this.tabControl.Controls.Add(this.tpMisc);
            this.tabControl.Dock=System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location=new System.Drawing.Point(0,0);
            this.tabControl.Name="tabControl";
            this.tabControl.SelectedIndex=0;
            this.tabControl.Size=new System.Drawing.Size(314,162);
            this.tabControl.TabIndex=0;
            // 
            // tpMain
            // 
            this.tpMain.Controls.Add(this.bFindInstall);
            this.tpMain.Controls.Add(this.lMorrowindVersion);
            this.tpMain.Controls.Add(this.lVersion);
            this.tpMain.Controls.Add(this.bFolder);
            this.tpMain.Controls.Add(this.bFile);
            this.tpMain.Controls.Add(this.bMorrowind);
            this.tpMain.Location=new System.Drawing.Point(4,22);
            this.tpMain.Name="tpMain";
            this.tpMain.Size=new System.Drawing.Size(306,136);
            this.tpMain.TabIndex=24;
            this.tpMain.Text="Main";
            // 
            // bFindInstall
            // 
            this.bFindInstall.Location=new System.Drawing.Point(160,72);
            this.bFindInstall.Name="bFindInstall";
            this.bFindInstall.Size=new System.Drawing.Size(139,23);
            this.bFindInstall.TabIndex=2;
            this.bFindInstall.Text="Find Morrowind manually";
            this.bFindInstall.Click+=new System.EventHandler(this.bFindInstall_Click);
            // 
            // lMorrowindVersion
            // 
            this.lMorrowindVersion.Location=new System.Drawing.Point(10,24);
            this.lMorrowindVersion.Name="lMorrowindVersion";
            this.lMorrowindVersion.Size=new System.Drawing.Size(165,45);
            this.lMorrowindVersion.TabIndex=5;
            this.lMorrowindVersion.Text="Temp text";
            this.lMorrowindVersion.TextChanged+=new System.EventHandler(this.lMorrowindVersion_TextChanged);
            // 
            // lVersion
            // 
            this.lVersion.Location=new System.Drawing.Point(10,10);
            this.lVersion.Name="lVersion";
            this.lVersion.Size=new System.Drawing.Size(200,14);
            this.lVersion.TabIndex=4;
            this.lVersion.Text="Exe optimizer v1.5";
            // 
            // bFolder
            // 
            this.bFolder.Location=new System.Drawing.Point(160,103);
            this.bFolder.Name="bFolder";
            this.bFolder.Size=new System.Drawing.Size(139,23);
            this.bFolder.TabIndex=4;
            this.bFolder.Text="Patch a folder";
            this.bFolder.Click+=new System.EventHandler(this.BFolderClick);
            // 
            // bFile
            // 
            this.bFile.Location=new System.Drawing.Point(9,103);
            this.bFile.Name="bFile";
            this.bFile.Size=new System.Drawing.Size(139,23);
            this.bFile.TabIndex=3;
            this.bFile.Text="Patch a file";
            this.bFile.Click+=new System.EventHandler(this.BFileClick);
            // 
            // bMorrowind
            // 
            this.bMorrowind.Location=new System.Drawing.Point(9,72);
            this.bMorrowind.Name="bMorrowind";
            this.bMorrowind.Size=new System.Drawing.Size(139,23);
            this.bMorrowind.TabIndex=1;
            this.bMorrowind.Text="Patch Morrowind";
            this.bMorrowind.Click+=new System.EventHandler(this.BMorrowindClick);
            // 
            // tpCode
            // 
            this.tpCode.Controls.Add(this.tbLastPatch);
            this.tpCode.Controls.Add(this.label9);
            this.tpCode.Controls.Add(this.tbFirstPatch);
            this.tpCode.Controls.Add(this.cbRestrict);
            this.tpCode.Controls.Add(this.label3);
            this.tpCode.Controls.Add(this.label2);
            this.tpCode.Controls.Add(this.label1);
            this.tpCode.Controls.Add(this.cbAlignv2s);
            this.tpCode.Controls.Add(this.cbAlignv3s);
            this.tpCode.Controls.Add(this.cbAlignv4s);
            this.tpCode.Controls.Add(this.cbModv4s);
            this.tpCode.Controls.Add(this.cbModv3s);
            this.tpCode.Controls.Add(this.cbModv2s);
            this.tpCode.Location=new System.Drawing.Point(4,22);
            this.tpCode.Name="tpCode";
            this.tpCode.Size=new System.Drawing.Size(306,136);
            this.tpCode.TabIndex=1;
            this.tpCode.Text="Code generator";
            // 
            // tbLastPatch
            // 
            this.tbLastPatch.ContextMenu=this.DudMenu;
            this.tbLastPatch.Enabled=false;
            this.tbLastPatch.Location=new System.Drawing.Point(232,96);
            this.tbLastPatch.MaxLength=4;
            this.tbLastPatch.Name="tbLastPatch";
            this.tbLastPatch.Size=new System.Drawing.Size(32,20);
            this.tbLastPatch.TabIndex=9;
            this.tbLastPatch.Text="0";
            this.tbLastPatch.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.NumberBoxKeyPressed);
            // 
            // label9
            // 
            this.label9.Location=new System.Drawing.Point(200,96);
            this.label9.Name="label9";
            this.label9.Size=new System.Drawing.Size(40,23);
            this.label9.TabIndex=12;
            this.label9.Text="and";
            // 
            // tbFirstPatch
            // 
            this.tbFirstPatch.ContextMenu=this.DudMenu;
            this.tbFirstPatch.Enabled=false;
            this.tbFirstPatch.Location=new System.Drawing.Point(160,96);
            this.tbFirstPatch.MaxLength=4;
            this.tbFirstPatch.Name="tbFirstPatch";
            this.tbFirstPatch.Size=new System.Drawing.Size(32,20);
            this.tbFirstPatch.TabIndex=8;
            this.tbFirstPatch.Text="0";
            this.tbFirstPatch.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.NumberBoxKeyPressed);
            // 
            // cbRestrict
            // 
            this.cbRestrict.Location=new System.Drawing.Point(8,96);
            this.cbRestrict.Name="cbRestrict";
            this.cbRestrict.Size=new System.Drawing.Size(152,24);
            this.cbRestrict.TabIndex=7;
            this.cbRestrict.Text="Restrict patches between";
            this.cbRestrict.CheckedChanged+=new System.EventHandler(this.cbRestrictCheckedChanged);
            // 
            // label2
            // 
            this.label2.Location=new System.Drawing.Point(192,32);
            this.label2.Name="label2";
            this.label2.Size=new System.Drawing.Size(56,16);
            this.label2.TabIndex=8;
            this.label2.Text="Align v3s";
            // 
            // label1
            // 
            this.label1.Location=new System.Drawing.Point(192,8);
            this.label1.Name="label1";
            this.label1.Size=new System.Drawing.Size(56,16);
            this.label1.TabIndex=7;
            this.label1.Text="Align v2s";
            // 
            // cbAlignv2s
            // 
            this.cbAlignv2s.ContextMenu=this.DudMenu;
            //this.cbAlignv2s.FormattingEnabled=true;
            this.cbAlignv2s.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.cbAlignv2s.Location=new System.Drawing.Point(136,8);
            this.cbAlignv2s.Name="cbAlignv2s";
            this.cbAlignv2s.Size=new System.Drawing.Size(40,21);
            this.cbAlignv2s.TabIndex=2;
            this.cbAlignv2s.Text="0";
            this.cbAlignv2s.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.cbNoKeyPresses);
            // 
            // cbAlignv3s
            // 
            this.cbAlignv3s.ContextMenu=this.DudMenu;
            //this.cbAlignv3s.FormattingEnabled=true;
            this.cbAlignv3s.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.cbAlignv3s.Location=new System.Drawing.Point(136,32);
            this.cbAlignv3s.Name="cbAlignv3s";
            this.cbAlignv3s.Size=new System.Drawing.Size(40,21);
            this.cbAlignv3s.TabIndex=4;
            this.cbAlignv3s.Text="1";
            this.cbAlignv3s.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.cbNoKeyPresses);
            // 
            // cbAlignv4s
            // 
            this.cbAlignv4s.ContextMenu=this.DudMenu;
            //this.cbAlignv4s.FormattingEnabled=true;
            this.cbAlignv4s.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3"});
            this.cbAlignv4s.Location=new System.Drawing.Point(136,56);
            this.cbAlignv4s.Name="cbAlignv4s";
            this.cbAlignv4s.Size=new System.Drawing.Size(40,21);
            this.cbAlignv4s.TabIndex=6;
            this.cbAlignv4s.Text="2";
            this.cbAlignv4s.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.cbNoKeyPresses);
            // 
            // cbModv4s
            // 
            this.cbModv4s.Checked=true;
            this.cbModv4s.CheckState=System.Windows.Forms.CheckState.Checked;
            this.cbModv4s.Location=new System.Drawing.Point(8,56);
            this.cbModv4s.Name="cbModv4s";
            this.cbModv4s.Size=new System.Drawing.Size(104,24);
            this.cbModv4s.TabIndex=5;
            this.cbModv4s.Text="Patch v4s";
            this.cbModv4s.CheckedChanged+=new System.EventHandler(this.CbModv4sCheckedChanged);
            // 
            // cbModv3s
            // 
            this.cbModv3s.Checked=true;
            this.cbModv3s.CheckState=System.Windows.Forms.CheckState.Checked;
            this.cbModv3s.Location=new System.Drawing.Point(8,32);
            this.cbModv3s.Name="cbModv3s";
            this.cbModv3s.Size=new System.Drawing.Size(104,24);
            this.cbModv3s.TabIndex=3;
            this.cbModv3s.Text="Patch v3s";
            this.cbModv3s.CheckedChanged+=new System.EventHandler(this.CbModv3sCheckedChanged);
            // 
            // cbModv2s
            // 
            this.cbModv2s.Checked=true;
            this.cbModv2s.CheckState=System.Windows.Forms.CheckState.Checked;
            this.cbModv2s.Location=new System.Drawing.Point(8,8);
            this.cbModv2s.Name="cbModv2s";
            this.cbModv2s.Size=new System.Drawing.Size(104,24);
            this.cbModv2s.TabIndex=1;
            this.cbModv2s.Text="Patch v2s";
            this.cbModv2s.CheckedChanged+=new System.EventHandler(this.CbModv2sCheckedChanged);
            // 
            // tpBench
            // 
            this.tpBench.Controls.Add(this.cbWin98);
            this.tpBench.Controls.Add(this.label6);
            this.tpBench.Controls.Add(this.cbLoops);
            this.tpBench.Controls.Add(this.label4);
            this.tpBench.Controls.Add(this.tbBias);
            this.tpBench.Controls.Add(this.cbDiscard);
            this.tpBench.Controls.Add(this.cbBenchmark);
            this.tpBench.Location=new System.Drawing.Point(4,22);
            this.tpBench.Name="tpBench";
            this.tpBench.Size=new System.Drawing.Size(306,136);
            this.tpBench.TabIndex=2;
            this.tpBench.Text="Benchmarker";
            // 
            // cbWin98
            // 
            this.cbWin98.Location=new System.Drawing.Point(8,62);
            this.cbWin98.Name="cbWin98";
            this.cbWin98.Size=new System.Drawing.Size(154,17);
            this.cbWin98.TabIndex=3;
            this.cbWin98.Text="Windows 98 compatibility fix";
            // 
            // label6
            // 
            this.label6.Location=new System.Drawing.Point(239,100);
            this.label6.Name="label6";
            this.label6.Size=new System.Drawing.Size(64,16);
            this.label6.TabIndex=5;
            this.label6.Text="Code loops";
            // 
            // cbLoops
            // 
            this.cbLoops.ContextMenu=this.DudMenu;
            //this.cbLoops.FormattingEnabled=true;
            this.cbLoops.Items.AddRange(new object[] {
            "1,000,000",
            "5,000,000",
            "10,000,000",
            "20,000,000",
            "50,000,000",
            "100,000,000"});
            this.cbLoops.Location=new System.Drawing.Point(129,97);
            this.cbLoops.MaxLength=9;
            this.cbLoops.Name="cbLoops";
            this.cbLoops.Size=new System.Drawing.Size(104,21);
            this.cbLoops.TabIndex=5;
            this.cbLoops.Text="10,000,000";
            this.cbLoops.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.cbNoKeyPresses);
            // 
            // label4
            // 
            this.label4.Location=new System.Drawing.Point(67,100);
            this.label4.Name="label4";
            this.label4.Size=new System.Drawing.Size(56,16);
            this.label4.TabIndex=3;
            this.label4.Text="Bias (ms)";
            // 
            // tbBias
            // 
            this.tbBias.ContextMenu=this.DudMenu;
            this.tbBias.Location=new System.Drawing.Point(8,97);
            this.tbBias.MaxLength=9;
            this.tbBias.Name="tbBias";
            this.tbBias.Size=new System.Drawing.Size(54,20);
            this.tbBias.TabIndex=4;
            this.tbBias.Text="1";
            this.tbBias.Leave+=new System.EventHandler(this.TbBiasLeave);
            this.tbBias.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.TbBiasKeyPress);
            // 
            // cbDiscard
            // 
            this.cbDiscard.Checked=true;
            this.cbDiscard.CheckState=System.Windows.Forms.CheckState.Checked;
            this.cbDiscard.Location=new System.Drawing.Point(8,32);
            this.cbDiscard.Name="cbDiscard";
            this.cbDiscard.Size=new System.Drawing.Size(176,24);
            this.cbDiscard.TabIndex=2;
            this.cbDiscard.Text="Discard inefficient patches";
            this.cbDiscard.CheckedChanged+=new System.EventHandler(this.CbDiscardCheckedChanged);
            // 
            // cbBenchmark
            // 
            this.cbBenchmark.Checked=true;
            this.cbBenchmark.CheckState=System.Windows.Forms.CheckState.Checked;
            this.cbBenchmark.Location=new System.Drawing.Point(8,8);
            this.cbBenchmark.Name="cbBenchmark";
            this.cbBenchmark.Size=new System.Drawing.Size(128,24);
            this.cbBenchmark.TabIndex=1;
            this.cbBenchmark.Text="Benchmark patches";
            this.cbBenchmark.CheckedChanged+=new System.EventHandler(this.CbBenchmarkCheckedChanged);
            // 
            // tpSegmenter
            // 
            this.tpSegmenter.Controls.Add(this.label8);
            this.tpSegmenter.Controls.Add(this.label7);
            this.tpSegmenter.Controls.Add(this.tbCodeSize);
            this.tpSegmenter.Controls.Add(this.tbOffset);
            this.tpSegmenter.Controls.Add(this.cbReadHeader);
            this.tpSegmenter.Controls.Add(this.label5);
            this.tpSegmenter.Controls.Add(this.tbBlacklist);
            this.tpSegmenter.Location=new System.Drawing.Point(4,22);
            this.tpSegmenter.Name="tpSegmenter";
            this.tpSegmenter.Size=new System.Drawing.Size(306,136);
            this.tpSegmenter.TabIndex=4;
            this.tpSegmenter.Text="Segmenter";
            // 
            // label8
            // 
            this.label8.Location=new System.Drawing.Point(184,104);
            this.label8.Name="label8";
            this.label8.Size=new System.Drawing.Size(100,23);
            this.label8.TabIndex=7;
            this.label8.Text="Code size";
            // 
            // label7
            // 
            this.label7.Location=new System.Drawing.Point(184,72);
            this.label7.Name="label7";
            this.label7.Size=new System.Drawing.Size(100,23);
            this.label7.TabIndex=6;
            this.label7.Text="Code offset";
            // 
            // tbCodeSize
            // 
            this.tbCodeSize.CharacterCasing=System.Windows.Forms.CharacterCasing.Upper;
            this.tbCodeSize.ContextMenu=this.DudMenu;
            this.tbCodeSize.Enabled=false;
            this.tbCodeSize.Location=new System.Drawing.Point(104,104);
            this.tbCodeSize.MaxLength=8;
            this.tbCodeSize.Name="tbCodeSize";
            this.tbCodeSize.Size=new System.Drawing.Size(64,20);
            this.tbCodeSize.TabIndex=4;
            this.tbCodeSize.Text="00000000";
            this.tbCodeSize.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.HexBoxKeyPress);
            // 
            // tbOffset
            // 
            this.tbOffset.CharacterCasing=System.Windows.Forms.CharacterCasing.Upper;
            this.tbOffset.ContextMenu=this.DudMenu;
            this.tbOffset.Enabled=false;
            this.tbOffset.Location=new System.Drawing.Point(104,72);
            this.tbOffset.MaxLength=8;
            this.tbOffset.Name="tbOffset";
            this.tbOffset.Size=new System.Drawing.Size(64,20);
            this.tbOffset.TabIndex=3;
            this.tbOffset.Text="00001000";
            this.tbOffset.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.HexBoxKeyPress);
            // 
            // cbReadHeader
            // 
            this.cbReadHeader.Checked=true;
            this.cbReadHeader.CheckState=System.Windows.Forms.CheckState.Checked;
            this.cbReadHeader.Location=new System.Drawing.Point(104,40);
            this.cbReadHeader.Name="cbReadHeader";
            this.cbReadHeader.Size=new System.Drawing.Size(112,24);
            this.cbReadHeader.TabIndex=2;
            this.cbReadHeader.Text="Read PE header";
            this.cbReadHeader.CheckedChanged+=new System.EventHandler(this.cbReadHeaderCheckedChanged);
            // 
            // label5
            // 
            this.label5.Location=new System.Drawing.Point(104,8);
            this.label5.Name="label5";
            this.label5.Size=new System.Drawing.Size(112,23);
            this.label5.TabIndex=2;
            this.label5.Text="Blacklisted segments";
            // 
            // tbBlacklist
            // 
            this.tbBlacklist.CharacterCasing=System.Windows.Forms.CharacterCasing.Upper;
            this.tbBlacklist.Location=new System.Drawing.Point(8,8);
            this.tbBlacklist.Multiline=true;
            this.tbBlacklist.Name="tbBlacklist";
            this.tbBlacklist.ScrollBars=System.Windows.Forms.ScrollBars.Vertical;
            this.tbBlacklist.Size=new System.Drawing.Size(88,120);
            this.tbBlacklist.TabIndex=1;
            this.tbBlacklist.Text="0013D7A8";
            this.tbBlacklist.WordWrap=false;
            this.tbBlacklist.Leave+=new System.EventHandler(this.tbBlacklistLeave);
            this.tbBlacklist.KeyPress+=new System.Windows.Forms.KeyPressEventHandler(this.tbBlacklistKeyPress);
            // 
            // tpMisc
            // 
            this.tpMisc.Controls.Add(this.cbAutoDelete);
            this.tpMisc.Controls.Add(this.bHelp);
            this.tpMisc.Controls.Add(this.bCreateConfig);
            this.tpMisc.Controls.Add(this.bPatch);
            this.tpMisc.Controls.Add(this.cbSave);
            this.tpMisc.Controls.Add(this.cbPrompts);
            this.tpMisc.Controls.Add(this.cbLogs);
            this.tpMisc.Controls.Add(this.cbBackups);
            this.tpMisc.Location=new System.Drawing.Point(4,22);
            this.tpMisc.Name="tpMisc";
            this.tpMisc.Size=new System.Drawing.Size(306,136);
            this.tpMisc.TabIndex=3;
            this.tpMisc.Text="Misc";
            // 
            // bHelp
            // 
            this.bHelp.Location=new System.Drawing.Point(158,66);
            this.bHelp.Name="bHelp";
            this.bHelp.Size=new System.Drawing.Size(136,23);
            this.bHelp.TabIndex=7;
            this.bHelp.Text="Help";
            this.bHelp.Click+=new System.EventHandler(this.BHelpClick);
            // 
            // bCreateConfig
            // 
            this.bCreateConfig.Location=new System.Drawing.Point(160,37);
            this.bCreateConfig.Name="bCreateConfig";
            this.bCreateConfig.Size=new System.Drawing.Size(136,23);
            this.bCreateConfig.TabIndex=6;
            this.bCreateConfig.Text="Create a config file";
            this.bCreateConfig.Click+=new System.EventHandler(this.BCreateConfigClick);
            // 
            // bPatch
            // 
            this.bPatch.Location=new System.Drawing.Point(160,8);
            this.bPatch.Name="bPatch";
            this.bPatch.Size=new System.Drawing.Size(136,23);
            this.bPatch.TabIndex=5;
            this.bPatch.Text="Apply precreated patch";
            this.bPatch.Click+=new System.EventHandler(this.BPatchClick);
            // 
            // cbSave
            // 
            this.cbSave.Location=new System.Drawing.Point(8,79);
            this.cbSave.Name="cbSave";
            this.cbSave.Size=new System.Drawing.Size(144,24);
            this.cbSave.TabIndex=4;
            this.cbSave.Text="Save settings";
            this.cbSave.CheckedChanged+=new System.EventHandler(this.CbSaveCheckedChanged);
            // 
            // cbPrompts
            // 
            this.cbPrompts.AutoCheck=false;
            this.cbPrompts.Location=new System.Drawing.Point(8,56);
            this.cbPrompts.Name="cbPrompts";
            this.cbPrompts.Size=new System.Drawing.Size(152,24);
            this.cbPrompts.TabIndex=3;
            this.cbPrompts.Text="Disable warning prompts";
            this.cbPrompts.Click+=new System.EventHandler(this.CbPromptsClick);
            // 
            // cbLogs
            // 
            this.cbLogs.Location=new System.Drawing.Point(8,33);
            this.cbLogs.Name="cbLogs";
            this.cbLogs.Size=new System.Drawing.Size(128,24);
            this.cbLogs.TabIndex=2;
            this.cbLogs.Text="Don\'t generate logs";
            // 
            // cbBackups
            // 
            this.cbBackups.AutoCheck=false;
            this.cbBackups.Location=new System.Drawing.Point(8,10);
            this.cbBackups.Name="cbBackups";
            this.cbBackups.Size=new System.Drawing.Size(136,24);
            this.cbBackups.TabIndex=1;
            this.cbBackups.Text="Don\'t make backups";
            this.cbBackups.Click+=new System.EventHandler(this.CbBackupsClick);
            // 
            // OpenFolder
            // 
            this.OpenFolder.Description="Select folder containing files to patch";
            this.OpenFolder.ShowNewFolderButton=false;
            // 
            // OpenPatch
            // 
            this.OpenPatch.Filter="Patch files (*.patch)|*.patch";
            this.OpenPatch.RestoreDirectory=true;
            this.OpenPatch.Title="Select patch file to apply";
            // 
            // OpenFile
            // 
            this.OpenFile.Filter="Executable files (exe,scr,dll)|*.exe;*.scr;*.dll|All files|*.*";
            this.OpenFile.RestoreDirectory=true;
            this.OpenFile.Title="Select file to patch";
            // 
            // cbAutoDelete
            // 
            this.cbAutoDelete.Location=new System.Drawing.Point(8,102);
            this.cbAutoDelete.Name="cbAutoDelete";
            this.cbAutoDelete.Size=new System.Drawing.Size(114,24);
            this.cbAutoDelete.TabIndex=8;
            this.cbAutoDelete.Text="Auto delete logs";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize=new System.Drawing.Size(5,13);
            this.ClientSize=new System.Drawing.Size(314,162);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle=System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox=false;
            this.Name="MainForm";
            this.Closing+=new System.ComponentModel.CancelEventHandler(this.MainFormClosing);
            this.tabControl.ResumeLayout(false);
            this.tpMain.ResumeLayout(false);
            this.tpCode.ResumeLayout(false);
            this.tpCode.PerformLayout();
            this.tpBench.ResumeLayout(false);
            this.tpBench.PerformLayout();
            this.tpSegmenter.ResumeLayout(false);
            this.tpSegmenter.PerformLayout();
            this.tpMisc.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        void cbNoKeyPresses(object sender,System.Windows.Forms.KeyPressEventArgs e) {
            e.Handled=true;
        }

        void TbBiasKeyPress(object sender,System.Windows.Forms.KeyPressEventArgs e) {
            if(Array.IndexOf(Numbers,e.KeyChar)==-1) e.Handled=true;
        }

        void CbModv2sCheckedChanged(object sender,System.EventArgs e) {
            cbAlignv2s.Enabled=cbModv2s.Checked;
        }

        void CbModv3sCheckedChanged(object sender,System.EventArgs e) {
            cbAlignv3s.Enabled=cbModv3s.Checked;
        }

        void CbModv4sCheckedChanged(object sender,System.EventArgs e) {
            cbAlignv4s.Enabled=cbModv4s.Checked;
        }

        void CbBenchmarkCheckedChanged(object sender,System.EventArgs e) {
            cbDiscard.Enabled=cbBenchmark.Checked;
            tbBias.Enabled=(cbDiscard.Checked&cbBenchmark.Checked);
            cbLoops.Enabled=cbBenchmark.Checked;
            cbWin98.Enabled=cbBenchmark.Checked;
        }

        void CbDiscardCheckedChanged(object sender,System.EventArgs e) {
            tbBias.Enabled=(cbDiscard.Checked&cbBenchmark.Checked);
        }

        void CbSaveCheckedChanged(object sender,System.EventArgs e) {
            if(!cbSave.Checked) {
                try { File.Delete("ExeOpt.ini"); } catch { }
            }
        }

        void CbBackupsClick(object sender,System.EventArgs e) {
            if(cbBackups.Checked) {
                cbBackups.Checked=false;
                return;
            }
            if(!ShowPrompts||MessageBox.Show("Disabling backups will prevent you from restoring unpatched versions of executables.\r\n"+
                               "Are you sure you wish to do this?","Warning",MessageBoxButtons.YesNo)==
                               DialogResult.Yes) {
                cbBackups.Checked=true;
            }
        }

        void CbPromptsClick(object sender,System.EventArgs e) {
            if(cbPrompts.Checked) {
                cbPrompts.Checked=false;
                return;
            }
            if(!ShowPrompts||MessageBox.Show("Disabling warning prompts may allow files to be overwritten without confirmation.\r\n"+
                               "Are you sure you wish to do this?","Warning",MessageBoxButtons.YesNo)==
                               DialogResult.Yes) {
                cbPrompts.Checked=true;
            }
        }

        void tbBlacklistLeave(object sender,System.EventArgs e) {
            ArrayList lines=new ArrayList(tbBlacklist.Lines);
            for(int i=0;i<lines.Count;i++) {
                string s=(string)lines[i];
                if(s.Length!=8) {
                    lines.RemoveAt(i--);
                } else {
                    for(int a=0;a<8;a++) {
                        if(Array.IndexOf(Hex2,s[a])==-1) {
                            lines.RemoveAt(i--);
                            break;
                        }
                    }
                }
            }
            tbBlacklist.Lines=(string[])lines.ToArray(typeof(string));
        }

        void tbBlacklistKeyPress(object sender,System.Windows.Forms.KeyPressEventArgs e) {
            if(Array.IndexOf(Hex,e.KeyChar)==-1) e.Handled=true;
        }

        void MainFormClosing(object sender,System.ComponentModel.CancelEventArgs e) {
            if(PatchInProgress) {
                e.Cancel=true;
                return;
            }
            if(cbSave.Checked) {
                BinaryWriter bw=new BinaryWriter(File.Open("ExeOpt.ini",FileMode.Create));
                bw.Write(sVersion);
                bw.Write(tbBlacklist.Text);
                bw.Write(cbBenchmark.Checked);
                bw.Write(cbDiscard.Checked);
                bw.Write(cbWin98.Checked);
                bw.Write(tbBias.Text);
                bw.Write(cbModv2s.Checked);
                bw.Write(cbModv3s.Checked);
                bw.Write(cbModv4s.Checked);
                bw.Write(cbAlignv2s.Text);
                bw.Write(cbAlignv3s.Text);
                bw.Write(cbAlignv4s.Text);
                bw.Write(cbBackups.Checked);
                bw.Write(cbLogs.Checked);
                bw.Write(cbPrompts.Checked);
                bw.Write(cbSave.Checked);
                bw.Write(cbLoops.Text);
                bw.Write(cbReadHeader.Checked);
                bw.Write(tbOffset.Text);
                bw.Write(tbCodeSize.Text);
                bw.Write(cbRestrict.Checked);
                bw.Write(tbFirstPatch.Text);
                bw.Write(tbLastPatch.Text);
                if(MorrowindPath==null) {
                    bw.Write("null");
                } else {
                    bw.Write(MorrowindPath);
                }
                bw.Close();
            }
        }

        void BHelpClick(object sender,System.EventArgs e) {
            try {
                System.Diagnostics.Process.Start("readme.chm");
            } catch(Exception ex) {
                MessageBox.Show(ex.Message,"Error");
            }
        }

        void BFolderClick(object sender,System.EventArgs e) {
            if(PatchInProgress) {
                MessageBox.Show("Can only patch one file at a time.","Error");
                return;
            }
            if(OpenFolder.ShowDialog()==DialogResult.OK) {
                ArrayList files=new ArrayList(Directory.GetFiles(OpenFolder.SelectedPath));
                for(int i=0;i<files.Count;i++) {
                    if(Array.IndexOf(ExecutableExtensions,Path.GetExtension((string)files[i]))==-1) {
                        files.RemoveAt(i--);
                    }
                }
                new PatchDialog((string[])files.ToArray(typeof(string)));
            }
        }

        void BMorrowindClick(object sender,System.EventArgs e) {
            if(PatchInProgress) {
                MessageBox.Show("Can only patch one file at a time.","Error");
                return;
            }
            new PatchDialog(MorrowindPath+"\\Morrowind.exe");
        }

        void BFileClick(object sender,System.EventArgs e) {
            if(PatchInProgress) {
                MessageBox.Show("Can only patch one file at a time.","Error");
                return;
            }
            if(OpenFile.ShowDialog()==DialogResult.OK) {
                new PatchDialog(OpenFile.FileName);
            }
        }

        void TbBiasLeave(object sender,System.EventArgs e) {
            try { Convert.ToInt32(tbBias.Text); } catch {
                tbBias.Text="0";
            }
        }

        void BPatchClick(object sender,System.EventArgs e) {
            if(PatchInProgress) {
                MessageBox.Show("Wait for the patch to complete before using this.","Error");
                return;
            }
            if(!MorrowindLatestVersion&&!cbPrompts.Checked) {
                if(MessageBox.Show("You do not appear to have morrowind version 1.6. Trying to apply .patch "+
                        "files to an older version of morrowind may corrupt the executable. Are you sure you "+
                        "wish to proceed?","Question",MessageBoxButtons.YesNo)==DialogResult.No)
                    return;
            }
            OpenPatch.InitialDirectory=Environment.CurrentDirectory;
            if(OpenPatch.ShowDialog()==DialogResult.OK) {
                string s=MorrowindPath+"\\Morrowind.exe";
                Program.FileName=s;
                try {
                    if(!File.Exists(s+".fpu2ssebak")) {
                        File.Copy(s,s+".fpu2ssebak");
                    }
                    Program.MorrowindPatch(OpenPatch.FileName);
                    if(!cbPrompts.Checked) MessageBox.Show("Patch applied","Message");
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message,"Error");
                }
            }
        }

        void HexBoxKeyPress(object sender,System.Windows.Forms.KeyPressEventArgs e) {
            if(Array.IndexOf(Hex,e.KeyChar)==-1) e.Handled=true;
        }

        void cbReadHeaderCheckedChanged(object sender,System.EventArgs e) {
            tbOffset.Enabled=!cbReadHeader.Checked;
            tbCodeSize.Enabled=!cbReadHeader.Checked;
        }

        void cbRestrictCheckedChanged(object sender,System.EventArgs e) {
            tbFirstPatch.Enabled=cbRestrict.Checked;
            tbLastPatch.Enabled=cbRestrict.Checked;
        }

        void NumberBoxKeyPressed(object sender,System.Windows.Forms.KeyPressEventArgs e) {
            if(Array.IndexOf(Numbers2,e.KeyChar)==-1) e.Handled=true;
        }

        void BCreateConfigClick(object sender,System.EventArgs e) {
            if(File.Exists("config.ini")) {
                MessageBox.Show("A config file already exists.","Error");
            } else {
                Program.EmmitConfigFile();
                MessageBox.Show("Saved 'config.ini'.","Message");
            }
        }

        private void lMorrowindVersion_TextChanged(object sender,EventArgs e) {
            bMorrowind.Enabled=MorrowindInstalled;
            bPatch.Enabled=MorrowindInstalled;
            if(!MorrowindInstalled||MorrowindLatestVersion) return;
            //Check morrowinds version
            try {
                FileVersionInfo fvi=FileVersionInfo.GetVersionInfo(MorrowindPath+"\\Morrowind.exe");
                if(fvi.FileMinorPart==6) {
                    MorrowindLatestVersion=true;
                    lMorrowindVersion.Text+="\nMorrowind version: 1.6";
                } else if(fvi.FileMinorPart==7) {
                    MorrowindLatestVersion=true;
                    lMorrowindVersion.Text+="\nMorrowind version: 1.7";
                } else {
                    MorrowindLatestVersion=false;
                }
            } catch {
                MorrowindLatestVersion=false;
            }
        }

        private void bFindInstall_Click(object sender,EventArgs e) {
            string s=OpenFolder.SelectedPath;
            OpenFolder.SelectedPath="\\";
            if(OpenFolder.ShowDialog()==DialogResult.OK) {
                if(File.Exists(OpenFolder.SelectedPath+"\\Morrowind.exe")) {
                    //User has selected a viable path
                    MorrowindInstalled=true;
                    MorrowindPath=OpenFolder.SelectedPath;
                    lMorrowindVersion.Text="Morrowind installation found in:\n   "+MorrowindPath;
                } else {
                    MessageBox.Show("Morrowind.exe not found");
                }
            }
            OpenFolder.SelectedPath=s;
        }

    }
}
#endif
