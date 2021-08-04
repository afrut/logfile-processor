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
using System.IO;

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
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.label1.Text = "File:";
            var now = DateTime.Now;
            string startTime = now.AddHours(-24).ToString("yyyyMMdd_HHmmss");
            string endTime = now.ToString("yyyyMMdd_HHmmss");
            this.textBox4.Text = startTime;
            this.textBox5.Text = endTime;

            // TESTING ONLY
            this.textBox1.Text = @"D:\src\cs-templates\SampleLoggingClient\bin\Debug\netcoreapp3.1\SampleLoggingClient.log";
            this.textBox6.Text = "Sum .* is \\d*\r\n----------\r\nPrevious random";

            // Event handlers.
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckChanged);
            this.button2.Click += new System.EventHandler(button2_Click);
            this.button3.Click += new System.EventHandler(button3_Click);
            this.button4.Click += new System.EventHandler(button4_Click);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            // Set initial directory.
            if (Directory.Exists(this.textBox1.Text))
                dialog.InitialDirectory = this.textBox1.Text;
            else if (File.Exists(this.textBox1.Text))
                dialog.InitialDirectory = Directory.GetParent(this.textBox1.Text).FullName;

            // Check if selecting a directory or a file.            
            if (checkBox1.Checked) dialog.IsFolderPicker = false;
            else
            {
                dialog.IsFolderPicker = true;
            }

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                this.textBox1.Text = dialog.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            // Set initial directory.
            if (Directory.Exists(this.textBox1.Text))
                dialog.InitialDirectory = this.textBox1.Text;
            else if (File.Exists(this.textBox1.Text))
                dialog.InitialDirectory = Directory.GetParent(this.textBox1.Text).FullName;

            dialog.IsFolderPicker = false;
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (dialog.FileNames.Count() > 0)
                {
                    // Modify directory.
                    string filename = dialog.FileNames.First();
                    this.textBox1.Text = Directory.GetParent(filename).ToString() + "\\";

                    // Get all filenames.
                    StringBuilder filenames = new StringBuilder();
                    foreach(String s in dialog.FileNames)
                        filenames.Append(Path.GetFileName(s) + ",");
                    filenames.Length = filenames.Length - 1;
                    this.textBox2.Text = filenames.ToString();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            // Set initial directory.
            if (Directory.Exists(this.textBox3.Text))
                dialog.InitialDirectory = this.textBox3.Text;
            else if (File.Exists(this.textBox3.Text))
                dialog.InitialDirectory = Directory.GetParent(this.textBox3.Text).FullName;
            else if (Directory.Exists(this.textBox1.Text))
                dialog.InitialDirectory = this.textBox1.Text;
            else if (File.Exists(this.textBox1.Text))
                dialog.InitialDirectory = Directory.GetParent(this.textBox1.Text).FullName;
            dialog.IsFolderPicker = false;
            dialog.DefaultFileName = "output.txt";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                this.textBox3.Text = dialog.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "LogfileProcessor.exe";

                StringBuilder args = new StringBuilder();

                if (this.checkBox1.Checked)
                {
                    // Tailing a single file.
                    args.Append("-Tail ");
                    if (this.textBox1.Text.Trim().Length > 0)
                        args.Append($"-Files \"{this.textBox1.Text}\" ");
                }
                else
                {
                    // Processing multiple selected files.
                    if (this.textBox2.Text.Trim().Length > 0)
                        args.Append($"-Files ");
                    foreach(string filename in this.textBox2.Text.Split(new string[] {","}, StringSplitOptions.None))
                        args.Append(this.textBox1.Text + filename + " ");
                }

                // Check if output file is specified.
                if (this.textBox3.Text.Trim().Length > 0)
                    args.Append($"-Output {this.textBox3.Text.Trim()} ");

                // Check if start and end times are specified.
                if (this.textBox4.Text.Trim().Length > 0)
                    args.Append($"-StartTime {this.textBox4.Text.Trim()} ");
                if (this.textBox5.Text.Trim().Length > 0)
                    args.Append($"-EndTime {this.textBox5.Text.Trim()} ");

                // Adding regex patterns.
                if (this.textBox6.Text.Trim().Length > 0)
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
                this.button2.Enabled = false;
                this.button3.Enabled = false;
                this.label1.Text = "File:";
            }
            else
            {
                this.textBox2.Enabled = true;
                this.textBox3.Enabled = true;
                this.textBox4.Enabled = true;
                this.textBox5.Enabled = true;
                this.button2.Enabled = true;
                this.button3.Enabled = true;
                this.label1.Text = "Directory:";
            }
        }
    }
}