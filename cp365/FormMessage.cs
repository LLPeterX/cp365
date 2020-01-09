using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cp365
{
    public partial class FormMessage : Form
    {
        public string title;
        public string text;
        public FormMessage()
        {
            InitializeComponent();
        }

        public void ShowInfo(string text)
        {
            this.label1.Text = text;
            this.Refresh();
        }

        public void ShowTitle(string text)
        {
            this.Text = text;
            this.Refresh();
        }

        public void SetProgressRanges(int maxNum)
        {
            this.progressBar1.Maximum = maxNum;
            this.progressBar1.Value = 1;
            this.progressBar1.Minimum = 1;
            this.progressBar1.Step = 1;
        }

        public void UpdateProgress()
        {
            this.progressBar1.PerformStep();
        }
    }
}
