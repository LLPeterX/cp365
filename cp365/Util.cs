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
            DateTime defaultDate = DateTime.Today;
            if (String.IsNullOrEmpty(str) || str.Length != 8) return defaultDate;
            try
            {
                try { 
                    DateTime newDate = DateTime.ParseExact(str, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    return newDate;
                } catch {
                    return defaultDate;
                }
            }
            catch
            {
                return defaultDate;
            }
        }

        // удалить все файлы из заданного каталога
        public static void CleanDirectory(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            foreach(string fileName in files)
            {
                File.Delete(fileName);
            }
        }

        public static string GUID() => Guid.NewGuid().ToString().ToUpper();
        

        public static string XMLDate(DateTime d) => d.ToString("yyyy-MM-dd");
        
        public static string XMLDateTime(DateTime d) => d.ToString("s");

        public static string DateToSQL(DateTime d) => d.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        public static DateTime DateFromSQL(string s) => String.IsNullOrEmpty(s) ? DateTime.Now : DateTime.ParseExact(s, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

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
            string backupDirectory = mainDirectory + "\\" + Util.DateToYMD(DateTime.Now) + "\\";
            if (!Directory.Exists(backupDirectory))
                Directory.CreateDirectory(backupDirectory);
            foreach(string fileName in sourceFiles)
            {
                string destinationFileName = backupDirectory + Path.GetFileName(fileName);
                File.Copy(fileName, destinationFileName,true);
            }
        }
    }
}
