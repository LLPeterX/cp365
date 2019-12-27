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
            string[] vrbFiles = Directory.GetFiles(tempDir, "*.vrb");
            foreach (string fname in vrbFiles)
            {
                if (Signature.Decrypt(tempDir+"\\"+fname) != 0)
                {
                    // Ошибка - чистим TEMP
                    Util.CleanDirectory(tempDir);
                    return false;
                }
            }
            // снятие ЭЦП с файлов *.xml
            string[] xmlFiles = Directory.GetFiles(tempDir, "*.xml");
            foreach(string fname in xmlFiles)
            {
                Signature.Delsign(tempDir+"\\"+fname);
            }
            // копирование в каталог IN\YYYYMMDD
            string subdir = Config.InDir+"\\"+Util.DateToYMD(DateTime.Now);
            if (Directory.Exists(subdir))
                Directory.CreateDirectory(subdir);
            string invDir = Config.InDir;
            foreach(string fname in xmlFiles)
            {
                File.Copy(tempDir + "\\" + fname, subdir + "\\" + fname);
                if(Directory.Exists(invDir))
                    File.Copy(tempDir + "\\" + fname, invDir + "\\" + fname);
            }
            return true;
        }

    }
}
