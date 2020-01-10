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
            if (Signature.isInitialized)
                Signature.Unload();
            Application.Exit();
        }

        private void menuConfig_Click(object sender, EventArgs e)
        {
            // Конфигурирование приложения
            FormConfig fconfig = new FormConfig();
            fconfig.ShowDialog();
            EnableOrDisableMenuItems(); // почему эта хрень не выполняется?
            fconfig.Dispose();
        }

        private void processAFN(object sender, EventArgs e)
        {

            OpenFileDialog dlg = new OpenFileDialog
            {
                InitialDirectory = Config.AFNDir,
                Filter = "ARJ files (*.arj)|*.arj",
                FilterIndex = 0,
                RestoreDirectory = true
            };
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            // Может, писать в протокол?
            AFNInputProcessor ap = new AFNInputProcessor(dlg.FileName);
            ap.Process();
            ap.Dispose();
            dlg.Dispose();
        }

        private void EnableOrDisableMenuItems()
        {
            this.menuProcessPTK.Enabled = Config.UsePTK;
        }

        private void ProcessPackagesFromPTK(object sender, EventArgs e)
        {
            FormPTK ptkForm = new FormPTK();
            ptkForm.ShowDialog();
            ptkForm.Dispose();
        }

        private void исходящиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // проверить, что чт-то есть для отправки
            if (Util.GetCountOfFilesInDirectory(Config.WorkDir) == 0)
                return;
            FormSend fs = new FormSend();
            fs.ShowDialog();
            fs.Dispose();
        }
    }
}
