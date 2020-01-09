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
    public class AFNInputProcessor
    {
        private string AFNname; // полный путь к файлу AFN
        private string shortAFNname; // короткое имя AFN
        private string tempDir; // отдельная переменная, чтобы лишний раз не пересчитывать Config
        private FormMessage info;

        // конструктор
        public AFNInputProcessor(string afn_name)
        {
            this.AFNname = afn_name;
            this.shortAFNname = Path.GetFileName(afn_name).ToUpper();
            this.tempDir = Config.TempDir;
            info = new FormMessage();
            info.ShowTitle("Обработка входящих файлов");
            info.Show();
        }
        public bool Decrypt()
        {
            // выбор файла AFNxxx.arj
            // надо ли снимать ЭЦП, если и так все прекрасно разархивируется?
            // разархивируем AFN
            info.ShowInfo("Распаковка " + AFNname);
            info.SetProgressRanges(1);
            Util.CleanDirectory(Config.TempDir);
            Process ps = new Process();
            ps.StartInfo.FileName = "arj32.exe";
            ps.StartInfo.Arguments = "x -y " + AFNname + " " + tempDir;
            ps.StartInfo.UseShellExecute = false;
            ps.Start();
            ps.WaitForExit();
            // теперь в tempDir разархивированные файлы - *.vrb и *.xml
            // расшифровать *.vrb в .xml
            info.ShowInfo("Инициализация Сигнатуры");
            if (!Signature.Initialize())
            {
                Util.CleanDirectory(tempDir);
                return false;
            }
            // получаем список зашифрованных файлов (у них расширение .vrb) и пробуем расшифровать  
            info.ShowInfo("Расшифровка " + AFNname);
            string[] vrbFiles = Directory.GetFiles(tempDir, "*.vrb"); // в массиве - полные имена
            foreach (string fname in vrbFiles)
            {
                if (Signature.Decrypt(fname) != 0)
                {
                    // Если ошибка - чистим TEMP и выходим
                    Util.CleanDirectory(tempDir);
                    return false;
                }
            }
            // На выходе после расшифровки получаем набор файлов *.xml
            // снятие ЭЦП с файлов *.xml
            string[] xmlFiles = Directory.GetFiles(tempDir, "*.xml");
            info.SetProgressRanges(xmlFiles.Length);
            info.ShowInfo("Снятие ЭЦП...");
            foreach (string fname in xmlFiles)
            {
                Signature.DeleteSign(fname);
                info.UpdateProgress();
            }
            // копирование в каталог IN\YYYYMMDD
            Util.CopyFilesToBackupDirectory(Config.InDir, xmlFiles);
            string invDir = Config.INVDir;
            string strStat = GetStatistic(xmlFiles);
            bool makePB1 = Config.CreatePB1;
            string workDir = Config.WorkDir;
            info.ShowInfo("Формирование PB");
            info.SetProgressRanges(xmlFiles.Length);
            foreach (string fname in xmlFiles) //fname = fullpath файла из каталога TEMP
            {
                string justName = Path.GetFileName(fname).ToUpper();
                string outName = invDir + "\\" + justName;
                if (Directory.Exists(invDir) && justName.Substring(0, 3) != "KWT")
                    File.Copy(fname, outName, true);
                if (makePB1 && justName.Substring(0, 3) != "KWT")
                {
                    PB pb1 = new PB(fname, PBTYPE.SUCCESS);
                    pb1.Save(workDir);
                }
                info.UpdateProgress();
            }
            info.Close();
            MessageBox.Show(strStat, shortAFNname, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        // Сборка статистики: сколько файлов каких типов было в файле AFN. Тип - первым 3 буквам имени файла xml
        public string GetStatistic(string[] fileNames)
        {
            Dictionary<String, Int32> stat = new Dictionary<String, Int32>();
            foreach (string xmlFileName in fileNames)
            {
                string xmlFileID = Path.GetFileName(xmlFileName).ToUpper().Substring(0, 3);
                if (stat.ContainsKey(xmlFileID))
                    stat[xmlFileID]++;
                else
                    stat[xmlFileID] = 1;

            }
            StringBuilder sb = new StringBuilder();
            foreach (string key in stat.Keys)
            {
                sb.Append(key);
                sb.Append(": ");
                sb.Append(stat[key].ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            if (info != null)
                info.Close();
        }
    }
}
