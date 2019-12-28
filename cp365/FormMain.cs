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
            EnableOrDisableMenuItems();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuConfig_Click(object sender, EventArgs e)
        {
            // Конфигурирование приложения
            FormConfig fconfig = new FormConfig();
            fconfig.ShowDialog();
            EnableOrDisableMenuItems(); // почему эта хрень не выполняется?
        }

        private void processAFN(object sender, EventArgs e)
        {
            DecryptAFN(null); // предполагается выбор файла вручную
            ShowProcess(false);

        }

        private void EnableOrDisableMenuItems()
        {
            this.menuProcessPTK.Enabled = Config.UsePTK;
        }

    }
}
