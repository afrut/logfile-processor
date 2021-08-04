using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

namespace LogfileProcessorUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Initial values.
            this.checkBox1.Checked = true;
            this.textBox2.Enabled = false;
            this.textBox3.Enabled = false;
            this.textBox4.Enabled = false;
            this.textBox5.Enabled = false;
            this.label1.Text = "File:";

            // TESTING ONLY
            this.textBox1.Text = @"D:\src\cs-templates\SampleLoggingClient\bin\Debug\netcoreapp3.1\SampleLoggingClient.log";
            this.textBox6.Text = "Sum .* is \\d*\r\n----------\r\nPrevious random";

            // Event handlers.
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckChanged);
            this.button4.Click += new System.EventHandler(button4_Click);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (checkBox1.Checked) dialog.IsFolderPicker = false;
            else dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.textBox1.Text = dialog.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "LogfileProcessor.exe";

                StringBuilder args = new StringBuilder();
                if (this.checkBox1.Checked) args.Append("-Tail ");
                if (this.textBox1.Text.Length > 0) args.Append($"-File \"{this.textBox1.Text}\" ");
                if (this.textBox6.Text.Length > 0)
                {
                    string[] patterns = this.textBox6.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    if (patterns.Length > 0) args.Append("-Patterns ");
                    foreach (String pattern in patterns) args.Append($"\"{pattern}\" ");
                }
                args.Length = args.Length - 1;
                process.StartInfo.Arguments = args.ToString();
                process.Start();
                process.WaitForExit();
            }
        }

        private void checkBox1_CheckChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                this.textBox2.Enabled = false;
                this.textBox3.Enabled = false;
                this.textBox4.Enabled = false;
                this.textBox5.Enabled = false;
                this.label1.Text = "File:";
            }
            else
            {
                this.textBox2.Enabled = true;
                this.textBox3.Enabled = true;
                this.textBox4.Enabled = true;
                this.textBox5.Enabled = true;
                this.label1.Text = "Directory:";
            }
        }
    }
}
