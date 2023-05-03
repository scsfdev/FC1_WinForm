using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DNWA.PassportAssist;
using System.IO;

namespace FC1_WinForm
{
    public partial class frmMain : Form
    {
        PassportReader pr;
        string imgDir = "";
        string title = "FC1 Demo";

        int comPort = 0;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pr != null)
                pr.Stop();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            pr = new PassportReader();
            pr.DataReceived += new PassportReader.DataReceivedHandler(Pr_DataReceived);
            pr.ShowProgress += new PassportReader.ShowProgressHandler(Pr_ShowProgress);
            pr.HideProgress += new PassportReader.HideProgressHandler(Pr_HideProgress);

            imgDir = Properties.Settings.Default.IMAGE_DIR;

            if (!Directory.Exists(imgDir))
                Directory.CreateDirectory(imgDir);


            txtCOM.Text = "7";
        }


        private string GetVersion()
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetEntryAssembly();
            return asm.GetName().Version.Major.ToString() + "." + asm.GetName().Version.Minor.ToString() + "." + asm.GetName().Version.Revision.ToString();
        }

        private void OpenFolder()
        {
            var runExplorer = new System.Diagnostics.ProcessStartInfo();
            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = imgDir;
            System.Diagnostics.Process.Start(runExplorer);
        }

        private void Start_Capture()
        {
            pr.StartCapture();
        }



        private void txtCOM_KeyPress(object sender, KeyPressEventArgs e)
        {
            string strNum = "0123456789";
            if (!strNum.Contains(e.KeyChar) && e.KeyChar!= 8)
                e.Handled = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCOM.Text))
                return;

            comPort = int.Parse(txtCOM.Text);

            if (comPort <= 0)
                return;

            pr.Start(comPort);
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnCapture.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (pr.Stop())
                picBox.Image = null;

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnCapture.Enabled = false;
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            Start_Capture();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            picBox.Image = null;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFolder();
        }


        private bool Pr_HideProgress()
        {
            //Console.WriteLine("Hide progress.");
            return true;
        }

        private bool Pr_ShowProgress()
        {
            //Console.WriteLine("Show progress.");
            return true;
        }

        private bool Pr_DataReceived(PassportReaderEventArg e)
        {
            if (!string.IsNullOrEmpty(e.Data.PassportNo))
            {
                // Passport scanning.
                string passportInfo = "";

                passportInfo = "Name: " + e.Data.Name + Environment.NewLine +
                               "Date of Birth: " + e.Data.DateOfBirth + Environment.NewLine +
                               "Nationality: " + e.Data.Nationality + Environment.NewLine +
                               "Passport Type: " + e.Data.Type + Environment.NewLine +
                               "Passport No: " + e.Data.PassportNo.Replace("<", "") + Environment.NewLine +
                               "Date of Expiry: " + e.Data.DateOfExpiry + Environment.NewLine +
                               "Issuing Country: " + e.Data.IssuingCountry;

                MessageBox.Show(passportInfo, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else if (!string.IsNullOrEmpty(e.ScanData))
            {
                // Barcode scanning.
                Console.WriteLine("Barcode:" + e.ScanData);
                MessageBox.Show(e.ScanData, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.ScanData = "";
            }

            if (e.Data.Image != null)
            {
                // There is an image, save it.

                string imgFileName = Path.Combine(imgDir, "FC1_" + DateTime.Now.ToString("yyyymmdd_hhmmss") + ".bmp");
                Image img = e.Data.Image;
                img.Save(imgFileName);
                picBox.Image = img;
            }

            return true;
        }

        
    }
}
