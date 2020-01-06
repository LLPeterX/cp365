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
            this.dateFrom.Value = Util.DateFromSQL("20.12.2019 00:00:00");
            this.dateTo.Value = Util.DateFromSQL("20.12.2019 23:59:00");
            //TimeSpan ts1 = new TimeSpan(23, 59, 59);
            //this.dateTo.Value = DateTime.Now + ts1;
        }

        
        private void btnRenew_Click(object sender, EventArgs e)
        {
            //this.dataGrid.Refresh();
            FillDataGrid();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            // обработка помеченных файлов

            if (this.dataGrid.SelectedRows.Count > 0)
            {
                string afnDirectory = Config.AFNDir;
                string errorMessage = "";
                string result = "";
                List<string> afnNames = new List<string>();
                foreach (DataGridViewRow row in this.dataGrid.SelectedRows)
                {
                    MZFile mz = row.DataBoundItem as MZFile;
                    if (mz.IsAFN())
                    {
                        // распаковываем mz в AFN_IN
                        if (!mz.ExtractFile(afnDirectory))
                        {
                            errorMessage += " Ошибка распаковки " + mz.mzName;
                        }
                        else
                        {
                            afnNames.Add(mz.ArjName);
                            result += mz.ArjName + "\n";
                        }
                    }
                }
                if (errorMessage.Length > 1)
                    MessageBox.Show(errorMessage, "Ошибки");
                MessageBox.Show("Следующие файлы помещены в каталог\n" + afnDirectory + ":\n\n" + result, "Распаковка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // обработка файлов AFN:
                this.Close();
            }
        }

        private void FormPTK_Load(object sender, EventArgs e)
        {
            // загрузить файлы
            FillDataGrid();
        }

        private void FillDataGrid()
        {
            //string strDateFrom = "12/20/2019 00:00:00";
            //string strDateTo = "12/20/2019 23:59:00";

            string errorMessage = null;
            //List<MZFile> data = ptk.GetMzFiles(strDateFrom, strDateTo, out errorMessage);
            List<MZFile> data = ptk.GetMzFiles(dateFrom.Value, dateTo.Value, out errorMessage);
            this.dataGrid.DataSource = data;
            this.dataGrid.AutoResizeColumns();
            //this.dataGrid.Refresh();
            if(!String.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка заполнения таблицы",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            this.btnProcess.Enabled = (this.dataGrid.SelectedRows.Count > 0);
            lbCountSelected.Text = this.dataGrid.SelectedRows.Count.ToString();
        }

        private void dateFrom_ValueChanged(object sender, EventArgs e)
        {
            // изменить время на 00:00:00
            this.dateFrom.Value.Add(new TimeSpan(0, 0, 0));
        }

        private void dateTo_ValueChanged(object sender, EventArgs e)
        {
            this.dateFrom.Value.Add(new TimeSpan(23, 59, 0));
        }
    }
}
