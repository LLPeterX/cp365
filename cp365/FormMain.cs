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
    public partial class FormMain : Form
    {
        public FormMain()
        {
            if (!Config.CheckFiles())
                Application.Exit();

            InitializeComponent();
            
           

        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuConfig_Click(object sender, EventArgs e)
        {
            // Конфигурирование приложения
            FormConfig fconfig = new FormConfig();
            fconfig.Show();
        }

        private void processAFN(object sender, EventArgs e)
        {
            DecryptAFN();
            ShowProcess(false);

        }

        private void processPTK(object sender, EventArgs e)
        {

        }
    }
}
