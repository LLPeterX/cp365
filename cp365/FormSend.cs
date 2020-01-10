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
    public partial class FormSend : Form
    {
        public FormSend()
        {
            InitializeComponent();
            this.porNum.Text = GetNextNumber();
            this.lbInfo.Text = "Порядковый N файла за " + Util.DateToYMD(DateTime.Now) + ":";
        }

        private string GetNextNumber()
        {
            int currentNumber = Config.SerialNum;
            string currentDate = Util.DateToYMD(DateTime.Now);
            if (Config.SerialDate == currentDate)
            {
                currentNumber++;
            }
            else
                currentNumber = 1;
            return currentNumber.ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int seqNumber;
            try
            {
                seqNumber = Convert.ToInt32(this.porNum.Text);
            }
            catch 
            {
                MessageBox.Show("Неверный порядковый номер");
                return;
            }
            if(seqNumber <=0 || seqNumber>99999)
            {
                MessageBox.Show("Неверный порядковый номер");
                return;
            }
            // чо-то обрабатываем
            AFNOutputProcessor ao = new AFNOutputProcessor(seqNumber);
            string errorMessage = "";
            ao.Process(out errorMessage);
            MessageBox.Show(errorMessage);
            if (ao.IsSuccess)
            {
                Config.SerialNum = seqNumber+ao.arjCount;
                Config.SerialDate = Util.DateToYMD(DateTime.Now);
            }
            ao.Dispose();
            this.Close();
        }
    }
}
