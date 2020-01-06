using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            foreach(string fileName in Directory.GetFiles(Config.WorkDir))
            {
                if(xmlInArj>=50)
                {
                    arjCount++;
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
            string err = "";
            foreach (AFNOutFile arj in arjFiles)
            {
                foreach(string xmlFile in arj.xmlFiles)
                {
                    // для каждлго файла - подписать
                    Signature.Sign(xmlFile,out err);
                }
                // шифруем

            }
            
        }
    }
}
