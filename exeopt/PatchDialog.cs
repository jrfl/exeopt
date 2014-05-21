// PatchDialog.cs
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
using System.Windows.Forms;
using System.Threading;
using BZip=SharpZipLib.BZip2;
using System.IO;

namespace Patcher {

    public class PatchDialog : System.Windows.Forms.Form {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Label ConsoleText;
        private System.Windows.Forms.Timer PatchTimer;
        private System.Windows.Forms.Button bCancel;

        private static Thread PatchThread;
        private static string file;
        private static string[] files;
        public static MainForm mainform;
        private static int BatchPosition;

        /// <summary>
        /// Need this to prevent cross threading errors when trying to disable cancel button from the patch thread
        /// </summary>
        private volatile bool DisableCancel=false;

        public PatchDialog(string FileName) : this(new string[] { FileName }) { }
        public PatchDialog(string[] FileName) {
            if(PatchThread!=null) return;
            file=FileName[0];
            files=FileName;
            Console.message="Preparing for patch";
            SetupOptions();
            InitializeComponent();
            BatchPosition=1;
            MainForm.PatchInProgress=true;
            PatchThread=new Thread(new ThreadStart(Patch));
            Show();
            Activate();
            PatchThread.Start();
            PatchTimer.Start();
        }

        public static void SetupOptions() {
            Program.FileName=file;
            Program.CreateLog=!mainform.cbLogs.Checked;
            Program.Benchmark=mainform.cbBenchmark.Checked;
            Program.IgnoreBenchmark=!mainform.cbDiscard.Checked;
            Program.Benchmark98Fix=mainform.cbWin98.Checked;
            Program.DissallowedAddresses=mainform.tbBlacklist.Lines;
            Program.Align[0]=Convert.ToByte(mainform.cbAlignv2s.Text);
            Program.Align[1]=Convert.ToByte(mainform.cbAlignv3s.Text);
            Program.Align[2]=Convert.ToByte(mainform.cbAlignv4s.Text);
            Program.Mod[0]=mainform.cbModv2s.Checked;
            Program.Mod[1]=mainform.cbModv3s.Checked;
            Program.Mod[2]=mainform.cbModv4s.Checked;
            Program.AutoGetCode=mainform.cbReadHeader.Checked;
            Program.CodeOffset=Convert.ToUInt32(mainform.tbOffset.Text,16);
            Program.CodeLength=Convert.ToUInt32(mainform.tbCodeSize.Text,16);
            Program.Restrict=mainform.cbRestrict.Checked;
            Program.FirstPatch=Convert.ToInt32(mainform.tbFirstPatch.Text);
            Program.LastPatch=Convert.ToInt32(mainform.tbLastPatch.Text);
            Patcher.Bias=TimeSpan.FromMilliseconds(Convert.ToDouble(mainform.tbBias.Text));
            int LoopTimes=Convert.ToInt32(mainform.cbLoops.Text.Replace(",",""));
            for(int i=0;i<4;i++) {
                Patcher.LoopCode[i]=(byte)((LoopTimes%Math.Pow(256,i+1))/Math.Pow(256,i));
            }
        }

        public void Patch() {
            try {
                if(!File.Exists(file)) return;
                string FileDirectory=Path.GetDirectoryName(file);
                //Check if the file is readonly or locked
                if((File.GetAttributes(file)&FileAttributes.ReadOnly)>0) {
                    if(MessageBox.Show("File is readonly. Remove readonly attribute?","Error",
                                       MessageBoxButtons.YesNo)==DialogResult.Yes) {
                        File.SetAttributes(file,File.GetAttributes(file)^FileAttributes.ReadOnly);
                    } else return;
                }
                FileStream fi;
                try {
                    fi=File.Open(file,FileMode.Open);
                    fi.Close();
                } catch(Exception ex) {
                    if(ex is ThreadAbortException) throw;
                    MessageBox.Show("File seems to be locked by another process. Unable to patch.","Error");
                    return;
                }
                //Search for a backup file and offer to restore it
                if(File.Exists(file+".fpu2ssebak")) {
                    switch(MessageBox.Show(file+".fpu2ssebak exists. Restore this backup?",
                                           "Restore backup",MessageBoxButtons.YesNoCancel)) {
                        case DialogResult.Yes:
                            File.Delete(file);
                            File.Move(file+".fpu2ssebak",file);
                            return;
                        case DialogResult.Cancel:
                            return;
                    }
                } else {
                    File.Copy(file,file+".fpu2ssebak");
                }

                //Search for any other files which would cause problems.
                string[] FileNames=new string[] {"nasm.exe","ndisasm.exe","asmdriver.exe","code","dcode.txt",
                "out","out.txt","sse.exe","fpu.exe","log.txt"};
                foreach(string str in FileNames) {
                    if(File.Exists(str)) {
                        if(MainForm.mainform.cbPrompts.Checked||MessageBox.Show("File '"+str+"' already exists. Overwrite?",
                            "Warning",MessageBoxButtons.YesNo)==DialogResult.Yes) {
                            try {
                                File.Delete(str);
                            } catch {
                                MessageBox.Show("Error deleting file. Try restarting","Error");
                                return;
                            }
                        } else return;
                    }
                }
                //Decompress the files packed inside files.cs
                FileData.Files.WriteFiles();

                Program.Patch();
                DisableCancel=true;

                //Display or ignore log as appropriate
                if(!mainform.cbPrompts.Checked) {
                    if(mainform.cbLogs.Checked) {
                        MessageBox.Show("Patching complete.","Finished");
                    } else {
                        if(MessageBox.Show("Patching complete. Show log?","Finshed",MessageBoxButtons.YesNo)==DialogResult.Yes) {
                            System.Diagnostics.Process.Start(FileDirectory+"\\log.txt");
                        }
                        if(mainform.cbAutoDelete.Checked) {
                            try {
                                File.Delete(FileDirectory+"\\log.txt");
                            } catch { }
                        }
                    }
                }
            } catch(ThreadAbortException) {
                return;
            } catch(Exception ex) {
                MessageBox.Show(ex.Message,"Error");
                return;
            }
        }

        #region Windows Forms Designer generated code
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent() {
            this.components=new System.ComponentModel.Container();
            this.bCancel=new System.Windows.Forms.Button();
            this.PatchTimer=new System.Windows.Forms.Timer(this.components);
            this.ConsoleText=new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bCancel
            // 
            this.bCancel.Location=new System.Drawing.Point(104,40);
            this.bCancel.Name="bCancel";
            this.bCancel.TabIndex=0;
            this.bCancel.Text="Cancel";
            this.bCancel.Click+=new System.EventHandler(this.BCancelClick);
            // 
            // PatchTimer
            // 
            this.PatchTimer.Tick+=new System.EventHandler(this.PatchTimerTick);
            // 
            // ConsoleText
            // 
            this.ConsoleText.Location=new System.Drawing.Point(8,8);
            this.ConsoleText.Name="ConsoleText";
            this.ConsoleText.Size=new System.Drawing.Size(264,24);
            this.ConsoleText.TabIndex=1;
            // 
            // PatchDialog
            // 
            this.AutoScaleBaseSize=new System.Drawing.Size(5,13);
            this.ClientSize=new System.Drawing.Size(282,74);
            this.ControlBox=false;
            this.Controls.Add(this.ConsoleText);
            this.Controls.Add(this.bCancel);
            this.FormBorderStyle=System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name="PatchDialog";
            this.ShowInTaskbar=false;
            this.Text="Patch in progress";
            this.TopMost=true;
            this.Closing+=new System.ComponentModel.CancelEventHandler(this.PatchDialogClosing);
            this.ResumeLayout(false);
        }
        #endregion

        void PatchTimerTick(object sender,System.EventArgs e) {
            if(DisableCancel&&bCancel.Enabled) bCancel.Enabled=false;
            if(Console.message!=""&&ConsoleText.Text!=Console.message) {
                ConsoleText.Text=Console.message;
            }
            if(PatchThread.ThreadState==ThreadState.Stopped) {
                if(BatchPosition==files.Length) {
                    CleanUp();
                    Close();
                } else {
                    file=files[BatchPosition++];
                    Console.message="Begining patch number "+BatchPosition.ToString();
                    ConsoleText.Text=Console.message;
                    SemiCleanup();
                    SetupOptions();
                    PatchThread=new Thread(new ThreadStart(Patch));
                    PatchThread.Start();
                }
            }
        }

        void BCancelClick(object sender,System.EventArgs e) {
            try {
                PatchThread.Abort();
                PatchThread.Join(10000);
            } catch { }
            if(File.Exists(file+".fpu2ssebak")) {
                try {
                    File.Delete(file);
                    File.Copy(file+".fpu2ssebak",file);
                } catch {
                    MessageBox.Show("The patcher seems to have the file locked open. Backup was not restored.","Error");
                }
            }
            CleanUp();
            Close();
        }

        private void SemiCleanup() {
            try { File.Delete("sse.exe"); } catch { }
            try { File.Delete("fpu.exe"); } catch { }
            try { File.Delete("dcode.txt"); } catch { }
            try { File.Delete("code"); } catch { }
            try { File.Delete("out"); } catch { }
            try { File.Delete("out.txt"); } catch { }
            try { File.Delete("nasm.exe"); } catch { }
            try { File.Delete("ndisasm.exe"); } catch { }
            try { File.Delete("asmdriver.exe"); } catch { }
        }

        private void CleanUp() {
            PatchTimer.Enabled=false;
            Console.message="Cleaning up";
            try { File.Delete("sse.exe"); } catch { }
            try { File.Delete("fpu.exe"); } catch { }
            try { File.Delete("dcode.txt"); } catch { }
            try { File.Delete("code"); } catch { }
            try { File.Delete("out"); } catch { }
            try { File.Delete("out.txt"); } catch { }
            try { File.Delete("nasm.exe"); } catch { }
            try { File.Delete("ndisasm.exe"); } catch { }
            try { File.Delete("asmdriver.exe"); } catch { }
            MainForm.PatchInProgress=false;
        }

        void PatchDialogClosing(object sender,System.ComponentModel.CancelEventArgs e) {
            if(MainForm.PatchInProgress) {
                e.Cancel=true;
            } else {
                GC.Collect();
                PatchThread=null;
                PatchTimer.Enabled=false;
            }
        }

    }
}
#endif
