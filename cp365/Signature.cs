using System;
//using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;

namespace cp365
{
    public static class Signature
    {
        private static bool use_virtual_fdd = Config.UseVirtualFDD;
        private static bool isInitialized = false;
        private const string SIG_PROGRAM = "spki1utl.exe";
        private static string SIG_PROFILE;
        private static string profilesBaseDirectory = null;
        private const string HKLM_SIGNATURE64 = @"HKEY_LOCAL_MACHINE\\SOFTWARE\\MDPREI\\scsref\\CurrentVersion";
        private const string HKLM_SIGNATURE32 = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\MDPREI\scs\CurrentVersion";
        // в реестре значение InstallPath

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
        static public bool Initialize()
        {
            if (isInitialized) return true; // success

            SIG_PROFILE = Config.Profile;
            CheckProfile(SIG_PROFILE);
            // Проверяем, что вставлен/смонтирован правильный носитель
            if (!isVdkeysPresent())
            {
                // поверяем imdisk
                if (!File.Exists("imdisk.exe") && Config.UseVirtualFDD)
                {
                    MessageBox.Show("Не найден файл imdisk.exe");
                    return false;
                }
                String imgFileName = SIG_PROFILE + ".img";
                if (!File.Exists(imgFileName))
                {
                    MessageBox.Show("Не найден файл " + imgFileName);
                    return false;
                }
                // запускаем imdisk
                Process ps = new Process();
                ps.StartInfo.FileName = "IMDISK.EXE";
                ps.StartInfo.Arguments = "-a -m A: -f " + imgFileName;
                ps.StartInfo.UseShellExecute = false;
                ps.Start();
                ps.WaitForExit();
            }
            // проверяем существование ключя в gdbm
            // Проверяем наличие A:\vdkeys
            if (isVdkeysPresent())
            {
                isInitialized = true;
                use_virtual_fdd = true;
                return true;
            }
            else
            {
                MessageBox.Show("Ошибка - на диске A: нет ключей");
                isInitialized = false;
                use_virtual_fdd = false;
                return false;
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
                }
            }
            catch
            {
                MessageBox.Show("Предупреждение: диск A: не отключен");
            }
        }

        static public int Sign(String file_name, out string result)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            String signedName = file_name + ".sig";
            String args = "-sign -profile " + SIG_PROFILE + " -registry -data " + file_name + " -out " + signedName;
            ExecuteSpki(args, out result);
            // удаляем оригинальный файл и переименовываем signedName в file_name
            if (!revertFile(file_name, signedName))
            {
                MessageBox.Show("Не удалось подписать файл " + file_name);
                return 1;
            }
            return 0;
        }

        static public int Encrypt(String file_name, string key, out string result)
        {
            String encryptedName = file_name + ".vrb";
            String args = "-encrypt -profile " + SIG_PROFILE + " -registry -in " + file_name +
                " -out " + encryptedName + " -reckeyid "+key;
            gzip(file_name);
            ExecuteSpki(args, out result);
            if (!revertFile(file_name, encryptedName))
            {
                MessageBox.Show("Не удалось зашифровать файл " + file_name);
                return 1;

            }
            return (0);
        }

        // расшифровка файла .vrb в .xml (дальнейшее преобразование будет далее)
        // 1) расшифроываем в .gz
        // 2) распаковываем .gz в .xml
        static public int Decrypt(String file_name)
        {
            string new_name = file_name.ToUpper().Replace(".VRB", ".gz");
            String args = "-decrypt -profile " + SIG_PROFILE + " -registry -in " + file_name + " -out " + new_name;
            string result;
            ExecuteSpki(args, out result);
            if (IsSuccess(result))
            {
                ungzip(new_name,".xml");
                File.Delete(file_name);

                return 0;
            }
            else
            {
                MessageBox.Show("Не удалось расшифровать файл\n" + file_name + "\nОшибка:\n" + result);
                return 1;
            }
        }

        // снятие ЭЦП: сначала .xml->.dec, потом переименовать обратно
        static public int DeleteSign(String srcFileName)
        {
            String dstFileName = srcFileName + ".dec";
            String args = "-verify -delete 1 -profile " + SIG_PROFILE + " -registry -in " + srcFileName + " -out " + dstFileName;

            string result = null;
            ExecuteSpki(args, out result);
            return revertFile(srcFileName, dstFileName) ? 0 : 1;

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
        private static bool isVdkeysPresent()
        {
            return Directory.Exists("A:\\vdkeys");
        }

        private static void ExecuteSpki(string arguments, out string result)
        {
            result = null;
            //Application.UseWaitCursor = true; // один хер не помгает
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
            profile = profile.Trim();
            if (String.IsNullOrEmpty(profile) || profile.Length < 3) return false;
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

        // Проверить наличие ключа в профиле
        // 1. открываем base_dir\profile\Local.gdbm
        // 2. Ищем ключ в содержимом файла тупо по сопадению подстрок
        public static bool CheckKey(string profile, string key)
        {
            if (key.Length != 12) return false;
            string pathGDBM = profilesBaseDirectory + "\\" + profile + "\\Local.gdbm";
            try
            {
                string gdbm = File.ReadAllText(pathGDBM);
                bool result = gdbm.Contains(key);
                return result;
                //return File.ReadAllText(pathGDBM).Contains(key);
            } catch // файла нет или ошибка
            {
                return false;
            }
        }
       

    }


}
