using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace cp365
{
    public static class Util
    {
        // Преобразорвание DateTime в строку YYYYMMDD
        public static string DateToYMD(DateTime d) => d.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
     
        // Преобразование даты в виде YYYYMMDD в DateTime
        public static DateTime DateFromYMD(string str)
        {
            if (String.IsNullOrEmpty(str) || str.Length != 8) return DateTime.Now;
            try { 
                return DateTime.ParseExact(str, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
           } catch {
                return DateTime.Now;
             }
        }

        // удалить все файлы из заданного каталога
        public static void CleanDirectory(string dir)
        {
            foreach(string fileName in Directory.GetFiles(dir))
            {
                File.Delete(fileName);
            }
        }

        public static string GUID() => Guid.NewGuid().ToString().ToUpper();
        
        public static string XMLDateTime(DateTime d) => d.ToString("s");

        public static string DateToSQL(DateTime d) => d.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        //public static DateTime DateFromSQL(string s) => String.IsNullOrEmpty(s) ? DateTime.Now : DateTime.ParseExact(s, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        public static int GetCountOfFilesInDirectory(string directory) => Directory.GetFiles(directory).Length;

        public static void CopyFilesToBackupDirectory(string mainDirectory, string[] files = null)
        {
            string[] sourceFiles;
            if(files == null)
            {
                sourceFiles = Directory.GetFiles(mainDirectory);
            } else
            {
                sourceFiles = files;
            }
            string backupDirectory = mainDirectory + "\\" + DateToYMD(DateTime.Now);
            if (!Directory.Exists(backupDirectory))
                Directory.CreateDirectory(backupDirectory);
            foreach(string fileName in sourceFiles)
            {
                string destinationFileName = backupDirectory + "\\"+Path.GetFileName(fileName);
                File.Copy(fileName, destinationFileName,true);
            }
        }
    }
}
