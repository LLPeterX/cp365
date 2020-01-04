using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Deployment.Compression;
using Microsoft.Deployment.Compression.Cab;

namespace cp365
{
    class MZFile
    {
        public string mzName { get; } // имя файла MZ
        private string mzFullPath; // полное имя файла
        public string fileName; // имя файла внутри MZ
        public DateTime mzFileDate { get; set; } // дата создания mz
        public string mzErrState {get; set;}
        

        public string ArjName { 
            get
            {
                return this.fileName;
            } 
        }

        // конструктор
        public MZFile(string mz_path)
        {
            this.mzFullPath = mz_path;
            this.mzName = Path.GetFileName(this.mzFullPath).ToLower();
            getFileInCAB(); // получить значение fileName в CAB-архиве
        }

        // ---- getters ---
        public string GetFile()
        {
            return this.fileName;
        }

        // -------- private methods ------
        private void getFileInCAB()
        {
            try
            {
                using (CabEngine engine = new CabEngine())
                {
                    IList<string> files;
                    using (Stream cabStream = File.OpenRead(mzFullPath))
                    {
                        files = engine.GetFiles(new ArchiveFileStreamContext(mzFullPath), null);
                    }
                    this.fileName = files[0];
                }
            }
            catch
            {
                this.fileName =  null;
                this.mzErrState = "Ошибка!";
            }
        }

        public bool ExctractFile(string directory)
        {
            bool result = false;
            try
            {

                CabInfo cab = new CabInfo(this.mzFullPath);
                cab.Unpack(directory);
                if (File.Exists(directory + "\\" + ArjName))
                {
                    result = true;
                }
                else result = false;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool FileExists(string directory)
        {
            return File.Exists(mzFullPath);
        }
    }
}
