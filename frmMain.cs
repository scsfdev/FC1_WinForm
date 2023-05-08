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


            txtCOM.Text = "3";
        }



        private void txtCOM_KeyPress(object sender, KeyPressEventArgs e)
        {
            string strNum = "0123456789";
            if (!strNum.Contains(e.KeyChar) && e.KeyChar != 8)
                e.Handled = true;
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFolder();
        }


        private void btnClear_Click(object sender, EventArgs e)
        {
            picBox.Image = null;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCOM.Text))
                return;

            try
            {
                comPort = int.Parse(txtCOM.Text);

                if (comPort <= 0)
                    return;
                
                btnConnect.Enabled = false;
                btnDisconnect.Visible = btnReadyRead.Visible = btnManualCapture.Visible = btnClear.Visible = true;

                ReadyFC1();          
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (pr.Stop())
                picBox.Image = null;

            btnConnect.Enabled = true;
            btnDisconnect.Visible = btnReadyRead.Visible = btnManualCapture.Visible = btnClear.Visible = false;
        }


        private void btnReadyRead_Click(object sender, EventArgs e)
        {
            ReadyFC1();

            // Set scanneer in Ready mode to scan MRZ or any code.
            pr.ExecuteCommand("R", out string response, 1000);
        }

        private void btnManualCapture_Click(object sender, EventArgs e)
        {
            ReadyFC1();
            pr.ExecuteCommand("R", out string response, 1000);
            pr.StartCapture();
        }

        private void ReadyFC1()
        {
            pr.Start(comPort);
            // Immediately go into Standby Mode to disable any code reading.
            pr.ExecuteCommand("Z", out string response, 1000);
        }

        private void OpenFolder()
        {
            var runExplorer = new System.Diagnostics.ProcessStartInfo();
            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = imgDir;
            System.Diagnostics.Process.Start(runExplorer);
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
                e.ScanData = null;
            }


            if (e.Data.Image != null)
            {
                // There is an image, save it.
                string imgFileName = Path.Combine(imgDir, "FC1_" + DateTime.Now.ToString("yyyymmdd_hhmmss") + ".bmp");
                Image img = e.Data.Image;
                img.Save(imgFileName);
                picBox.Image = img;
            }

            e.Data = null;
            e.ScanData = null;

            pr.Stop();
          
            return true;
        }        
    }
}
