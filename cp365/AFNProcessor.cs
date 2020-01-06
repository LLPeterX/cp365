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
    public class AFNProcessor
    {
        private string AFNname; // полный путь к файлу AFN
        private string shortAFNname; // короткое имя AFN
     // конструктор
        public AFNProcessor(string afn_name)
        {
            this.AFNname = afn_name;
            this.shortAFNname = Path.GetFileName(afn_name).ToUpper();
        }
        public bool Decrypt() // afnFileName - полный путь к AFN
        {
            string tempDir = Config.TempDir; // отдельная переменная, чтобы лишний раз не перечитывать файл .ini
            // выбор файла AFNxxx.arj
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
                if(!Signature.Initialize())
            {
                Util.CleanDirectory(tempDir);
                return false;
            }
            // получаем список зашифрованных файлов (у них расширение .vrb) и пробуем расшифровать  
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
                    Signature.DeleteSign(fname);
                }
                // копирование в каталог IN\YYYYMMDD
                string subdir = Config.InDir + "\\" + Util.DateToYMD(DateTime.Now);
                if (!Directory.Exists(subdir))
                    Directory.CreateDirectory(subdir);
                string invDir = Config.INVDir;
                string strStat = GetStatistic(xmlFiles);
            bool makePB1 = Config.CreatePB1;
            string workDir = Config.WorkDir;
            foreach (string fname in xmlFiles) //fname = fullpath файла из каталога TEMP
            {
                string justName = Path.GetFileName(fname).ToUpper();
                string outName = subdir + "\\" + justName;
                File.Copy(fname, outName, true);
                outName = invDir + "\\" + justName;
                if (Directory.Exists(invDir))
                    File.Copy(fname, outName, true);
                // создать файлы полтверждения PB1
                if (makePB1 && justName.Substring(0, 3) != "KWT")
                {
                    PB pb1 = new PB(fname, PBTYPE.GOOD);
                    pb1.Save(workDir);
                }
             }
             MessageBox.Show(strStat, shortAFNname, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        // Сборка статистики: сколько файлов каких тиапов в AFN
        public string  GetStatistic(string[] fileNames)
        {
            Dictionary<String,Int32> stat = new Dictionary<String, Int32>();
            foreach(string fname in fileNames)
            {
                string id = this.shortAFNname.Substring(0, 3);
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
    }
}
