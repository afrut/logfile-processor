using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogfileProcessorUI
{
    public partial class FormEmptyTextBox : Form
    {
        public FormEmptyTextBox()
        {
            InitializeComponent();
        }

        public FormEmptyTextBox(string title, string str)
        {
            InitializeComponent();
            this.Text = title;
            this.textBox1.Text = str;
        }
    }
}
