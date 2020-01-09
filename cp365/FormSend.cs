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
            SetLabelText();
        }

        private string GetNextNumber()
        {
            int currentNumber = Config.SerialNum;
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            if (Config.SerialDate == currentDate)
            {
                currentNumber++;
            }
            else
                currentNumber = 1;
            return currentNumber.ToString();
        }

        private void SetLabelText()
        {
            this.lbInfo.Text = "Порядковый N файла за " + 
                DateTime.Now.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture)+": ";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int seqNumber;
            if (Util.GetCountOfFilesInDirectory(Config.WorkDir) == 0)
                return;
            try
            {
                seqNumber = Convert.ToInt32(this.porNum.Text);
            }
            catch
            {
                MessageBox.Show("Неверный порядковый номер");
                return;
            }
            // чо-то обрабатываем
            AFNOutputProcessor ao = new AFNOutputProcessor();
            string errorMessage = "";
            ao.Process(out errorMessage);
            if(!String.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка");
                this.Close();
                return;
             }
              Config.SerialNum = seqNumber;
              Config.SerialDate = DateTime.Now.ToString("yyyyMMdd");
            
            this.Close();
        }
    }
}
