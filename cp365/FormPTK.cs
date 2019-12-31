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
    // см. https://www.bestprog.net/ru/2015/12/22/002-%D0%B2%D1%8B%D0%B2%D0%BE%D0%B4-%D1%82%D0%B0%D0%B1%D0%BB%D0%B8%D1%86%D1%8B-%D0%B1%D0%B0%D0%B7%D1%8B-%D0%B4%D0%B0%D0%BD%D0%BD%D1%8B%D1%85-microsoft-access-%D0%B2-%D0%BA%D0%BE%D0%BC%D0%BF%D0%BE/
    // (работа с адаптерами)
    public partial class FormPTK : Form
    {
        PTKPSD ptk;
        public FormPTK()
        {
            InitializeComponent();
            ptk = new PTKPSD(Config.PTKDatabase);
            if(ptk.eloDir==null)
            {
                MessageBox.Show("Не удалось получить доступ к ПТК ПСД\n" + ptk.errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        
        private void btnRenew_Click(object sender, EventArgs e)
        {
            this.dataGrid.Refresh();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            // обработка помеченных файлов
            this.Close();
        }

        private void FormPTK_Load(object sender, EventArgs e)
        {
            // загрузить файлы
            FillDataGrid();
        }

        private void FillDataGrid()
        {
            string strDateFrom = "12/20/2019 00:00:00";
            string strDateTo = "12/20/2019 23:59:00";
            string errorMessage = null;
            List<MZFile> data = ptk.GetMzFiles(strDateFrom, strDateTo, out errorMessage);
            this.dataGrid.DataSource = data;
            this.dataGrid.Refresh();
            if(errorMessage.Length>10)
            {
                MessageBox.Show(errorMessage, "Ошибка",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
