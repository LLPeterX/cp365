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
            // ниже в датах заменяем время на 00:00:00 или 23:59:00
            this.dateFrom.Value = CreateDate(DateTime.Now, true); 
            this.dateTo.Value = CreateDate(DateTime.Now, false);
        }

        
        private void btnRenew_Click(object sender, EventArgs e)
        {
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
                if (afnNames.Count>0 && MessageBox.Show("Следующие файлы помещены в каталог\n" + afnDirectory + ":\n\n" + result + "\nОбработать?",
                    "Распаковка", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach(string afnFile in afnNames)
                    {
                        string fullPath = afnDirectory+"\\"+afnFile;
                        AFNProcessor prc = new AFNProcessor(fullPath);
                        prc.Decrypt();
                    }
                    MessageBox.Show("Готово");
                }
                this.Close(); // закрываем форму
            }
        }

        private void FormPTK_Load(object sender, EventArgs e)
        {
            // загрузить файлы
            FillDataGrid();
        }

        private void FillDataGrid()
        {

            string errorMessage = null;
            List<MZFile> data = ptk.GetMzFiles(dateFrom.Value, dateTo.Value, out errorMessage);
            this.dataGrid.DataSource = data;
            this.dataGrid.Columns["mzName"].HeaderText = "Файл ПТК";
            this.dataGrid.Columns["mzFileDate"].HeaderText = "Дата";
            this.dataGrid.Columns["mzErr"].HeaderText = "Ошибка";
            this.dataGrid.Columns["ArjName"].HeaderText = "Имя файла ФНС";
            this.dataGrid.AutoResizeColumns();
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

        private DateTime CreateDate(DateTime dt, bool startDate)
        {
            if(startDate)
            {
                return new DateTime(dt.Year,
        dt.Month,
        dt.Day,
        0,
        0,
        0,
        0,
        dt.Kind);
            } else
            {
                return new DateTime(dt.Year,
        dt.Month,
        dt.Day,
        23,
        59,
        0,
        0,
        dt.Kind);
            }
        }
    }
}
