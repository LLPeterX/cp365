using System;
using System.Collections.Generic;
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
        private const string SIG_INSALL_PATH = "HKEY_LOCAL_MACHINE\\SOFTWARE\\MDPREI\\scsref\\CurrentVersion";
        /*
         * scs/InstallPath - это будет путь к spki1utl.exe
         */
        private const string SIG_PROF_SECTION = "HKEY_CURRENT_USER\\Software\\MDPREI\\spki";
        /* внутри значения:
         *  Profiles/count - число профилей
         *  Profiles/CurSel - N профиля по умолчанию (1)
         *  Profile/0 - пао умолчанию (не юзать)
         *  Profiles/1/BasePath - путь к профилям
         *  Profiles/1/ProfileName - имя профиля
         *  store/0 (1  т.п.)Ж
         *    store/
        */  

        // Инициализация 
        static public int Initialize()
        {
            if (isInitialized) return 0;

            SIG_PROFILE = Config.Profile;
            // Проверяем, что вставлен/смонтирован правильный носитель
            if (!isVdkeysPresent())
            {
                // поверяем imdisk
                if (!File.Exists("imdisk.exe"))
                {
                    MessageBox.Show("Не найден файл imdisk.exe");
                    return 1;
                }
                String imgFileName = SIG_PROFILE + ".img";
                if (!File.Exists(imgFileName))
                {
                    MessageBox.Show("Не найден файл " + imgFileName);
                    return 1;
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
                return 0;
            }
            else
            {
                MessageBox.Show("Ошибка: на диске A: нет ключей");
                isInitialized = false;
                use_virtual_fdd = false;
            }
            return 1;
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
            /*
            if(IsSuccess(result))
            {
                MessageBox.Show("Успешно");
            } else
            {
                MessageBox.Show("Ошибка");
            }
            */
            // удаляем оригинальный файл и переименовываем signedName в file_name
            if (!revertFile(file_name, signedName))
            {
                MessageBox.Show("Не удалось подписать файл " + file_name);
                return 1;
            }
            return 0;
        }

        static public int Encrypt(String file_name, out string result)
        {
            String encryptedName = file_name + ".vrb";
            String args = "-encrypt -profile " + SIG_PROFILE + " -in " + file_name +
                " -out " + encryptedName + " -reclist .\\reclist.txt";
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
        static public int Decrypt(String file_name)
        {
            String args = "-decrypt -profile " + SIG_PROFILE + " -registry -in " + file_name + " -out " + file_name + ".gz";
            string result = null;
            ExecuteSpki(args, out result);
            return revertFile(file_name, file_name + ".xml") ? 0 : 1;

        }

        static public int Delsign(String file_name)
        {
            String cleanName = file_name + ".dec";
            String args = "-verify -delete 1 -profile " + SIG_PROFILE + " -registry -in " + file_name +
                "-out " + cleanName;

            Process ps = new Process();
            ps.StartInfo.FileName = SIG_PROGRAM;
            ps.StartInfo.Arguments = args;
            ps.StartInfo.UseShellExecute = false;
            ps.Start();
            ps.WaitForExit();
            return revertFile(file_name, cleanName) ? 0 : 1;

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
        static public void ungzip(String filename)
        {
            String new_name = filename.Replace(".gz", ".xml");
            //FileStream gzStream = File.OpenRead(filename); // gzip file
            //FileStream outStream = File.OpenWrite(new_name); // result
            using (FileStream gzipFileStream = new FileStream(filename,FileMode.Open))
            {
                using (FileStream decompressedFileStream = File.Create(new_name))
                {
                    using (GZipStream decompressionStream = new GZipStream(gzipFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
            //File.Delete(filename);
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
            Process ps = new Process();
            ps.StartInfo.FileName = SIG_PROGRAM; // spki1utl.exe
            ps.StartInfo.Arguments = arguments;
            ps.StartInfo.UseShellExecute = false;
            ps.StartInfo.RedirectStandardError = true;
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

        public bool CheckProfileAndKey(string profile, string key)
        {
            Registry.GetValue
          
            return false;

        }
        

    }


}
