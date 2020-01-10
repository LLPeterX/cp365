using System;
//using System.Collections.Generic;
using System.Text;
//using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;


namespace cp365
{
    public static class Signature
    {
        public const int SUCCESS = 0;
        public const int ERROR = 1;
        private static bool use_virtual_fdd;
        public static bool isInitialized { get; set; } = false;
        //private const string SIG_PROGRAM = "spki1utl.exe";
        private static string SIG_PROGRAM;
        private static string SIG_PROFILE;
        private static string profilesBaseDirectory = null;
        private const string HKLM_SIGNATURE64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\MDPREI\scs\CurrentVersion";
        private const string HKLM_SIGNATURE32 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\MDPREI\scs\CurrentVersion";
        // см. в реестре значение InstallPath

        private const string HKCU_PROFILE = @"HKEY_CURRENT_USER\Software\MDPREI\spki\Profiles";
        /* внутри значения:
         *  count - число профилей
         *  CurSel - N профиля по умолчанию (1)
         *  Profile/0 - пао умолчанию (не юзать)
         *  Profiles/1/BasePath - путь к профилям
         *  Profiles/1/ProfileName - имя профиля
         *  store/0 (1  т.п.)Ж
         *    store/
        */

        // Инициализация СКАД "Сигнатура"
        // 1) проверяем - есть ли A: ключи на нем
        //    если нет - монтируем ч/з imdisk образ "profile.IMG" на диск А:
        // 2) Проверяем наличие A:\vdkeys
        static public (int, string) Initialize(string profile, bool virtualFDD=false)
        {
            if (isInitialized) return (SUCCESS,null);
            // Тут может быть засада, если мы в конфиге меняем профиль или иные параметры+
            // Поэтому после конфига isInitialized = false
            SIG_PROFILE = profile;
            SIG_PROGRAM = LocateExecutable();
            if(!File.Exists(SIG_PROGRAM)) {
                return (ERROR, "Не могу найти spki1utl.exe");
            }
            use_virtual_fdd = virtualFDD;
            if (!CheckProfile(SIG_PROFILE))
                return (ERROR,"Неверный профиль");
            // Проверяем, что вставлен/смонтирован правильный носитель
            if (!isVdkeysPresent())
            {
                // если включено использование виртуального FDD,
                // то монтируем его на A:
                if (use_virtual_fdd)
                {
                    if (!File.Exists("imdisk.exe"))
                    {
                        return (ERROR, "Не найден файл imdisk.exe");
                    }
                    String imgFileName = SIG_PROFILE + ".img";
                    if (!File.Exists(imgFileName))
                    {
                        return (ERROR, "Не найден файл " + imgFileName);
                    }
                    // запускаем imdisk
                    Process ps = new Process();
                    ps.StartInfo.FileName = "IMDISK.EXE";
                    ps.StartInfo.Arguments = "-a -m A: -f " + imgFileName;
                    ps.StartInfo.UseShellExecute = false;
                    ps.Start();
                    ps.WaitForExit();
                    ps.Dispose();
                }
            }
            // на этом месте у нас уже есть A: - либо реальный, либо образ
            // проверяем существование ключя в gdbm
            // Проверяем наличие A:\vdkeys
            if (isVdkeysPresent())
            {
                isInitialized = true;
                use_virtual_fdd = true;
                return (SUCCESS,null);
            }
            else
            {
                isInitialized = false;
                use_virtual_fdd = false;
                return (ERROR, "Ошибка - на диске A: нет ключей");
            }
        }
        static public void Unload()
        {
            try
            {
                if (use_virtual_fdd)
                {
                    Process ps = new Process();
                    ps.StartInfo.FileName = "IMDISK.EXE";
                    ps.StartInfo.Arguments = "-d -m A:";
                    ps.StartInfo.UseShellExecute = false;
                    ps.Start();
                    ps.WaitForExit();
                    use_virtual_fdd = false;
                    ps.Dispose();
                }
            }
#pragma warning disable CA1031 // Не перехватывать исключения общих типов
            catch
#pragma warning restore CA1031 // Не перехватывать исключения общих типов
            {
            } finally
            {
                isInitialized = false;
            }
        }

        static public (int,string) Sign(String fileName)
        {
            String signedName = fileName + ".sig";
            String args = "-sign -profile " + SIG_PROFILE + " -registry -data " + fileName + " -out " + signedName;
            string result = ExecuteSpki(args);
            // удаляем оригинальный файл и переименовываем signedName в file_name
            if (!revertFile(fileName, signedName))
            {
                return (ERROR, "Не удалось подписать файл " + fileName+"\n"+result);
            }
            return (SUCCESS, result) ;
        }

        static public (int,string) Encrypt(String fileName, string key)
        {
            String encryptedName = fileName + ".vrb";
            String args = "-encrypt -profile " + SIG_PROFILE + " -registry -in " + fileName +
                " -out " + encryptedName + " -reckeyid "+key;
            gzip(fileName);
            string result = ExecuteSpki(args);
            if (!revertFile(fileName, encryptedName))
            {
                return (ERROR,result);

            }
            return (SUCCESS,result);
        }
     
        // расшифровка файла .vrb в .xml (дальнейшее преобразование будет далее)
        // 1) расшифроываем в .gz
        // 2) распаковываем .gz в .xml
        static public (int,string) Decrypt(String fileName)
        {
            string new_name = fileName.ToUpper().Replace(".VRB", ".gz");
            String args = "-decrypt -profile " + SIG_PROFILE + " -registry -in " + fileName + " -out " + new_name;
            string result = ExecuteSpki(args);
            if (IsSuccess(result))
            {
                ungzip(new_name,".xml");
                File.Delete(fileName);
                return (SUCCESS, result);
            }
            else
            {
                return (ERROR,result);
            }
        }

        // снятие ЭЦП: сначала .xml->.dec, потом переименовать обратно
        static public (int, string) DeleteSign(String srcFileName)
        {
            String dstFileName = srcFileName + ".dec";
            String args = "-verify -delete 1 -profile " + SIG_PROFILE + " -registry -in " + srcFileName + " -out " + dstFileName;

            string result = ExecuteSpki(args);
            return revertFile(srcFileName, dstFileName) ? (SUCCESS,result) : (ERROR,result);

        }

        static public int gzip(String filename)
        {
            String new_name = filename + ".gz";
            FileStream inStream = File.OpenRead(filename);
            FileStream outStream = File.OpenWrite(new_name);
            GZipStream compressionStream = new GZipStream(outStream, CompressionMode.Compress);
            inStream.CopyTo(compressionStream);
            compressionStream.Close();
            outStream.Close();
            inStream.Close();
            return revertFile(filename, new_name) ? 0 : 1;

        }

        // нииже хуйня. код лучше: https://docs.microsoft.com/ru-ru/dotnet/api/system.io.compression.gzipstream?view=netframework-4.8
        // Распаковать файл VRB в dec (.gz получен после decrypt vrb)
        // файлы .dec потом используютcя в Delsign()
        static public void ungzip(string zipName, string newExtension)
        {
            String xmlName = zipName.Replace(".gz", newExtension);
            using (FileStream gzipFileStream = new FileStream(zipName,FileMode.Open))
            {
                using (FileStream decompressedFileStream = File.Create(xmlName))
                {
                    using (GZipStream decompressionStream = new GZipStream(gzipFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
            // Если xml создан, то удаляем уже ненужный .gz
            if(File.Exists(xmlName))
            {
                File.Delete(zipName);
            }
        }

        // переместить файл newFile в oldFile
        static private bool revertFile(String oldFile, String newFile)
        {
            if (oldFile != null && newFile != null)
            {
                if (File.Exists(newFile))
                {
                    File.Delete(oldFile);
                    File.Move(newFile, oldFile);
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }
        // существует ли диск ("A:" или "B:")
        private static bool isVdkeysPresent() => Directory.Exists("A:\\vdkeys");
        

        private static string ExecuteSpki(string arguments)
        {
            string result;
            Process ps = new Process();
            ps.StartInfo.FileName = SIG_PROGRAM; // spki1utl.exe
            ps.StartInfo.Arguments = arguments;
            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.RedirectStandardError = true;
            ps.StartInfo.CreateNoWindow = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ps.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("CP866");
            ps.Start();
            result = ps.StandardError.ReadToEnd();
            ps.WaitForExit();
            ps.Dispose();
            return result;
        }

        private static bool IsSuccess(string result)
        {
            if (result.Contains("(00000000)"))
                return true;
            return false;
        }

        // Проверить наличие профиля. Для этого смотрим в реестре
        public static bool CheckProfile(string profile)
        {
            if (String.IsNullOrEmpty(profile)) return false;
            Int32 profilesCount = (Int32)Registry.GetValue(HKCU_PROFILE, "count", 0);
            profilesBaseDirectory = (String)Registry.GetValue(HKCU_PROFILE, "BasePath",null);
            if (profilesCount == 0)
                return false;
            // читаем профили
            for(int profileNum=0; profileNum<profilesCount; profileNum++)
            {
                // читаем разделы реестра "HKEY_CURRENT_USER\Software\MDPREI\spki\Profiles\0" и т.д.
                string profileName = (String)Registry.GetValue(HKCU_PROFILE + "\\" + profileNum.ToString(), "ProfileName", "");
                if(!String.IsNullOrEmpty(profileName) && profileName==profile)
                {
                    return true;
                }
            }
            return false;
        }

        // Проверить наличие ключа в списке сертификатов
        // 1. открываем {profiles_dir}\{profile}\Local.gdbm
        // 2. Ищем ключ в содержимом файла тупо по сопадению подстрок
        public static bool CheckKey(string profile, string key)
        {
            if (key.Length != 12) return false;
            string gdbmFile = profilesBaseDirectory + "\\" + profile + "\\Local.gdbm";
            try
            {
                return File.ReadAllText(gdbmFile).Contains(key);
            } catch // файла нет или ключ не найден в файле
            {
                return false;
            }
        }

        private static string LocateExecutable()
        {
            string pathToSpki = Registry.GetValue(HKLM_SIGNATURE32, "InstallPath", "").ToString();
            if(String.IsNullOrEmpty(pathToSpki))
            {
                pathToSpki = Registry.GetValue(HKLM_SIGNATURE64, "InstallPath", "").ToString();
            }
            if (String.IsNullOrEmpty(pathToSpki)) // путь не найден
            {
                pathToSpki = AppDomain.CurrentDomain.BaseDirectory;
                if (!pathToSpki.EndsWith("\\"))
                    pathToSpki += "\\";
            }
            return pathToSpki + "spki1utl.exe";
        }
       

    }


}

