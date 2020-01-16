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
            ptk = new PTKPSD(Config.PTKiniFile);
            this.dateFrom.Value = CreateDate(DateTime.Now, true); 
            this.dateTo.Value = CreateDate(DateTime.Now, false);
            this.dateFrom_ValueChanged(null, null);
            this.dateTo_ValueChanged(null, null);
        }

        
        private void btnRenew_Click(object sender, EventArgs e)
        {
            FillDataGrid();
            this.dataGrid.Focus();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// По выделенным строкам: взять посылку из PTK\Post\Store, снять ЭЦП, распаковать в AFN_IN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProcess_Click(object sender, EventArgs e)
        {
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
                            result += mz.ArjName + Environment.NewLine;
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
                        AFNInputProcessor prc = new AFNInputProcessor(fullPath);
                        prc.Process();
                        prc.Dispose();
                    }
                    MessageBox.Show("Готово");
                }
                this.Close(); // закрываем форму
            }
        }

        private void FormPTK_Load(object sender, EventArgs e)
        {
            FillDataGrid();
        }

        /// <summary>
        /// Заполнение таблицы перечнем посылок имени посылки mz и имени ARJ-архива в нем
        /// </summary>
        private void FillDataGrid()
        {

            string errorMessage;
            List<MZFile> data = ptk.GetMzFiles(dateFrom.Value, dateTo.Value, out errorMessage);
            if (data == null || !String.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка заполнения таблицы", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.dataGrid.DataSource = data;
            this.dataGrid.Columns["mzName"].HeaderText = "Файл ПТК";
            this.dataGrid.Columns["mzFileDate"].HeaderText = "Дата";
            this.dataGrid.Columns["mzErr"].HeaderText = "Ошибка";
            this.dataGrid.Columns["ArjName"].HeaderText = "Имя файла ФНС";
            this.dataGrid.AutoResizeColumns();
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            this.btnProcess.Enabled = (this.dataGrid.SelectedRows.Count > 0);
            lbCountSelected.Text = this.dataGrid.SelectedRows.Count.ToString();
        }

        private void dateFrom_ValueChanged(object sender, EventArgs e)
        {
            // изменить время в дате на 00:00:00
            this.dateFrom.Value.Add(new TimeSpan(0, 0, 0));
        }

        private void dateTo_ValueChanged(object sender, EventArgs e)
        {
            // изменить время в дате на 23:590:00
            this.dateFrom.Value.Add(new TimeSpan(23, 59, 0));
        }
        /// <summary>
        /// Создает новую дату на основе существующей (dt)
        /// если startDate = true, то время заменяется на 00:00:00, иначе - на 23:59:00
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// копирует значение поля dateTo в dateFrom и заменяет время на 23:59:00
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'c' || e.KeyChar=='C')
            {
                DateTime dtNew = this.dateFrom.Value;
                dtNew = CreateDate(dtNew, false);
                this.dateTo.Value = dtNew;
            }
        }
    }
}
