using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace cp365
{
    public partial class FormMain
    {
        // 1. Выбрать файл AFN из каталога [Config.AFNDir]
        // 2. [снять ЭЦП] - не сделано
        // 3. Разврхивировать в TEMP
        // 4. Расшифровать *.vrb и переименовать в *.xml
        // 5. Теперб всек файлы .xml подписаны. Снять ЭЦП
        // 6. Скопировать в TO_INV и в IN\YYYYMMDD
        public bool DecryptAFN()
        {
            string tempDir = Config.TempDir;
            // выбор файла AFN
            OpenFileDialog openAFN = new OpenFileDialog();
            openAFN.InitialDirectory = Config.AFNDir;
            openAFN.Filter = "ARJ files (*.arj)|*.arj";
            openAFN.FilterIndex = 0;
            openAFN.RestoreDirectory = true;
            if (openAFN.ShowDialog() != DialogResult.OK)
                return false;
            //            this.lbInfo.Text = "Обработка...";
            //            this.lbInfo.Visible = true;
            ShowProcess(true);
            
                string AFNname = Path.GetFullPath(openAFN.FileName);
                // надо ли снимать ЭЦП, если и так все прекрасно разархивируется?
                // разархивируем AFN
                Util.CleanDirectory(Config.TempDir);
                Process ps = new Process();
                ps.StartInfo.FileName = "arj32.exe";
                ps.StartInfo.Arguments = "x -y " + AFNname + " " + tempDir;
                ps.StartInfo.UseShellExecute = false;
                ps.Start();
                ps.WaitForExit();
                // теперь в TEMP разархивированные файлы - *.vrb и *.xml
                // расшифровать *.vrb в .xml
                Signature.Initialize();
                string[] vrbFiles = Directory.GetFiles(tempDir, "*.vrb"); // в массиве полные имена
                foreach (string fname in vrbFiles)
                {
                    if (Signature.Decrypt(fname) != 0)
                    {
                        // Если ошибка - чистим TEMP и выходим
                        Util.CleanDirectory(tempDir);
                        return false;
                    }
                }
                // снятие ЭЦП с файлов *.xml
                string[] xmlFiles = Directory.GetFiles(tempDir, "*.xml");
                foreach (string fname in xmlFiles)
                {
                    Signature.Delsign(fname);
                }
                // копирование в каталог IN\YYYYMMDD
                string subdir = Config.InDir + "\\" + Util.DateToYMD(DateTime.Now);
                if (!Directory.Exists(subdir))
                    Directory.CreateDirectory(subdir);
                string invDir = Config.INVDir;
                string strStat = GetStatistic(xmlFiles);
                foreach (string fname in xmlFiles) //fname = fullpath файла из каталога TEMP
                {
                    string outName = subdir + "\\" + Path.GetFileName(fname);
                    File.Copy(fname, outName, true);
                    outName = invDir + "\\" + Path.GetFileName(fname);
                    if (Directory.Exists(invDir))
                        File.Copy(fname, outName, true);
                }
                MessageBox.Show(strStat, "Статистика", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
        }

        private string  GetStatistic(string[] fileNames)
        {
            Dictionary<String,Int32> stat = new Dictionary<String, Int32>();
            foreach(string fname in fileNames)
            {
                string fileName = Path.GetFileName(fname).ToUpper();
                string id = fileName.Substring(0, 3);
                if (stat.ContainsKey(id))
                    stat[id]++;
                else
                    stat[id] = 1;
                
            }
            StringBuilder sb = new StringBuilder();
            foreach(string key in stat.Keys)
            {
                sb.Append(key);
                sb.Append(": ");
                sb.Append(stat[key].ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private void ShowProcess(bool active)
        {
            if(active)
            {
                this.lbInfo.Visible = true;
                this.lbInfo.Text = "Обработка...";
                this.lbInfo.Left = (this.Width - lbInfo.Width) / 2;
            } else
            {
                this.lbInfo.Visible = false;
            }
            this.Refresh();
        }

    }
}
