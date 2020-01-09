using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace cp365
{
    class AFNOutputProcessor
    {
        private string workDir;
        private List<AFNOutFile> arjFiles;

        
        public AFNOutputProcessor()
        {
            this.workDir = Config.WorkDir;
            this.arjFiles = new List<AFNOutFile>();
        }

        public void Process(out string errorMessage)
        {
            // Обработка файлов в каталоге WORK
            Signature.Initialize();
            errorMessage = null;
            int arjCount = 1;
            int xmlInArj = 0;
            AFNOutFile aout = new AFNOutFile(arjCount);
            arjFiles.Add(aout);
            string err = "";
            foreach(string fileName in Directory.GetFiles(Config.WorkDir))
            {
                if(xmlInArj>=50)
                {
                    ++arjCount;
                    xmlInArj = 0;
                    aout = new AFNOutFile(arjCount);
                    arjFiles.Add(aout);
                }
                aout.xmlFiles.Add(fileName);
                xmlInArj++;

            }
            // теперь у нас есть массив arjFiles с файлами xml
            // PBx подписываем, остальное - шифруем
            //   P.S. arjName - без пути, в arj.xmlFiles - полные пути
            foreach (AFNOutFile arj in arjFiles)
            {
                foreach(string xmlFile in arj.xmlFiles)
                {
                    // для кажд.файла - подписать
                    Signature.Sign(xmlFile,out err);
                    string shortXmlName = Path.GetFileName(xmlFile).ToUpper();
                    if (shortXmlName.Substring(0, 2) != "PB")
                    {
                        if (Signature.Encrypt(xmlFile, out err) != 0)
                        {
                            errorMessage += err;
                        } 
                        else
                            xmlFile.ToUpper().Replace(".XML", ".VRB");
                    }

                }
            }
            if (!String.IsNullOrEmpty(errorMessage))
                return;
            // теперь в WORK у нас смесь .xml и .vrb
            // их надо упаковать в arj и поместить файлы в OUT
            // файлы скидываем в lst.txt
            string outDir = Config.OutDir;
            errorMessage = "";
            foreach (AFNOutFile arj in arjFiles)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string xmlFile in  arj.xmlFiles)
                {
                    sb.Append(xmlFile);
                    sb.Append('\n');
                }
                File.WriteAllText("files.lst", sb.ToString());
                string outName = outDir + "\\" + arj.arjName;
                Process ps = new Process();
                ps.StartInfo.FileName = "arj32.exe";
                ps.StartInfo.Arguments = "a -e " + outName + " !files.lst";
                ps.StartInfo.UseShellExecute = false;
                ps.Start();
                ps.WaitForExit();
                if(File.Exists(outName))
                {

                    if(Signature.Sign(outName, out err)!=0)
                        errorMessage += err;
                }
            }

        }
    }
}
