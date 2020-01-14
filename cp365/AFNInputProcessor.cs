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
    public class AFNInputProcessor : IDisposable
    {
        private string AFNname; // полный путь к файлу AFN
        private string shortAFNname; // короткое имя AFN
        private string tempDir; // отдельная переменная, чтобы лишний раз не пересчитывать Config
        private FormMessage info;

        // конструктор
        public AFNInputProcessor(string afnFileName)
        {
            this.AFNname = afnFileName;
            this.shortAFNname = Path.GetFileName(afnFileName).ToUpper();
            this.tempDir = Config.TempDir;
            info = new FormMessage();
            info.Show();
        }
        public bool Process()
        {
            // выбор файла AFNxxx.arj
            // надо ли снимать ЭЦП, если и так все прекрасно разархивируется?
            // разархивируем AFN
            info.ShowTitle("Обработка " + Path.GetFileName(AFNname));
            info.SetText("Распаковка...");
            info.SetProgressRanges(1);
            Util.CleanDirectory(Config.TempDir);
            Process ps = new Process();
            ps.StartInfo.FileName = "arj32.exe";
            ps.StartInfo.Arguments = "x -y " + AFNname + " " + tempDir;
            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.CreateNoWindow = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.Start();
            ps.WaitForExit();
            ps.Dispose();
            // теперь в tempDir разархивированные файлы - *.vrb и *.xml
            // расшифровать *.vrb в .xml
            info.SetText("Инициализация СКАД \"Сигнатура\"");
            var (resCode, resString) = Signature.Initialize(Config.Profile, Config.UseVirtualFDD);
            if (resCode != Signature.SUCCESS)
            {
                MessageBox.Show(resString, "Ошибка инициализации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Util.CleanDirectory(tempDir);
                return false;
            }
            // получаем список зашифрованных файлов (у них расширение .vrb) и пробуем расшифровать  
            info.SetText("Расшифровка... ");
            string[] vrbFiles = Directory.GetFiles(tempDir, "*.vrb"); // в массиве - полные имена
            info.SetProgressRanges(vrbFiles.Length);
            foreach (string fname in vrbFiles)
            {
                (resCode, resString) = Signature.Decrypt(fname);
                if (resCode != Signature.SUCCESS)
                {
                    // Если ошибка - чистим TEMP и выходим
                    MessageBox.Show(resString, "Ошибка расшифровки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Util.CleanDirectory(tempDir);
                    return false;
                }
                info.UpdateProgress();
            }
            // На выходе после расшифровки получаем набор файлов *.xml
            // снятие ЭЦП с файлов *.xml
            string[] xmlFiles = Directory.GetFiles(tempDir, "*.xml");
            info.SetProgressRanges(xmlFiles.Length);
            info.SetText("Снятие ЭЦП...");
            foreach (string fname in xmlFiles)
            {
                (resCode, resString) = Signature.DeleteSign(fname);
                if(resCode != Signature.SUCCESS)
                {
                    MessageBox.Show(resString, "Ошибка Снятия ЭЦП", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Util.CleanDirectory(tempDir);
                    return false;
                }
                info.UpdateProgress();
            }
            // копирование в каталог IN\YYYYMMDD
            Util.CopyFilesToBackupDirectory(Config.InDir, xmlFiles);
            string invDir = Config.INVDir;
            string strStat = GetStatistic(xmlFiles);
            bool makePB1 = Config.CreatePB1;
            bool makePB2 = Config.CreatePB1 && Config.NoLicense;
            string workDir = Config.WorkDir;
            info.SetText("Формирование PB");
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
                if (makePB2 && CanAnswerPB2(justName.Substring(0, 3))) {
                    PB pb2 = new PB(fname, PBTYPE.ERROR);
                    pb2.Save(workDir);
                }
                info.UpdateProgress();
            }
            info.Close();
            MessageBox.Show(strStat, shortAFNname, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        // Сборка статистики: сколько файлов каких типов было в файле AFN. По первым 3 буквам имени файла xml
        public static string GetStatistic(string[] fileNames)
        {
            Dictionary<String, Int32> stat = new Dictionary<String, Int32>();
            if (fileNames == null || fileNames.Length == 0) return "(Пусто)";
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

        private bool CanAnswerPB2(string fileID)
        {
            return (fileID=="RPO" || fileID=="ROO" || fileID=="TRB" || fileID=="TRG");
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (info != null)
                    info.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
