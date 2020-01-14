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
        public string mzName { get; } // корткое имя файла MZ
        private string mzFullPath; // полное имя файла (PTK\TEMP\mz....)
        public string fileName; // имя файла внутри MZ
        public DateTime mzFileDate { get; set; } // дата создания mz
        public string mzErr {get; set;} // сообщение об ошибке (если есть)
        public bool valid; // файл правильный (существует, в нем есть AFN_MIFNS00*.ARJ)
        

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
            if(mz_path == null)
            {
                this.mzErr = "Не найден";
                this.mzName = "";
                this.fileName = "";
                this.valid = false;

            }
            this.mzName = Path.GetFileName(this.mzFullPath).ToLower();
            getFileInCAB(); // получить значение fileName в CAB-архиве
            if(!FileExists())
            {
                this.mzErr = "Файл не найден";
                this.valid = false;
            }
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
#pragma warning disable CA1031 // Не перехватывать исключения общих типов
            catch
#pragma warning restore CA1031 // Не перехватывать исключения общих типов
            {
                this.fileName =  null;
                this.mzErr = "Ошибка получения файла из MZ!";
                this.valid = false;
            }
        }
        public bool ExtractFile(string directory)
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
#pragma warning disable CA1031 // Не перехватывать исключения общих типов
            catch
#pragma warning restore CA1031 // Не перехватывать исключения общих типов
            {
                result = false;
            }
            return result;
        }

        public bool FileExists()
        {
            if (File.Exists(mzFullPath))
            {
                this.valid = true;
                return true;
            }
            else
            {
                this.valid = false;
                return false;
            }
        }

        public bool IsAFN()
        {
            if (String.IsNullOrEmpty(fileName)) return false;
            return this.fileName.ToUpper().StartsWith("AFN")
                && this.fileName.ToUpper().EndsWith("ARJ");
        }

    }
}
