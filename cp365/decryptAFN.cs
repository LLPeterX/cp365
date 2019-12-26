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
            // разархивируем AFN
            Util.CleanDirectory(Config.TempDir);
            Process ps = new Process();
            ps.StartInfo.FileName = "arj32.exe";
            ps.StartInfo.Arguments = "x -y " + AFNname + " " + tempDir;
            ps.StartInfo.UseShellExecute = false;
            ps.Start();
            ps.WaitForExit();
            // теперь в TEMP разархифированные файлы - *.vrb и *.xml
            // Снять подпись со всех
            string[] allFiles = Directory.GetFiles(tempDir, "*.*");
            foreach(string fname in allFiles) {
                Signature.Delsign(fname);
            }
            // расшифровать *.vrb в .xml
            string[] vrbFiles = Directory.GetFiles(tempDir, "*.vrb");
            foreach (string fname in allFiles)
            {
                Signature.Decrypt(fname);
            }


            // распаковка файлов
            return true;
        }

    }
}
