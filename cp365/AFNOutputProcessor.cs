using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace cp365
{
    class AFNOutputProcessor : IDisposable
    {
        private string workDir;
        private List<AFNOutFile> arjFiles;
        public bool IsSuccess = true;
        private int arjNumber; // порядковый номер файла ARJ за тек.дату
        public int arjFilesCount; // общее число файлов ARJ
        private FormMessage info;
        
        public AFNOutputProcessor(int porNo)
        {
            this.workDir = Config.WorkDir;
            this.arjFiles = new List<AFNOutFile>();
            this.arjNumber = porNo; // начальный N архива
            this.arjFilesCount = 1; // кол-во сформированных ARJ-файлов
            info = new FormMessage();
            info.SetTitle("Подготовка файлов");
            info.Show();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Укажите CultureInfo", Justification = "<Ожидание>")]
        public void Process(out string errorMessage)
        {
            // Обработка файлов в каталоге WORK
            info.SetText("Инициализация Сигнатуры");
            info.SetProgressRanges(1);
            var (resCode, resString) = Signature.Initialize(Config.Profile, Config.UseVirtualFDD);
            if (resCode != Signature.SUCCESS)
            {
                errorMessage = "Ошибка инициализации СКАД \"Сигнатура\""+Environment.NewLine+resString;
                IsSuccess = false;
                Signature.isInitialized = false;
                this.Dispose();
                return;
            }
            errorMessage = null;
            int numberFilesInArj = 0;
            Util.CopyFilesToBackupDirectory(workDir); // copy WORK\*.* -> WORK\yyymmdd\
            AFNOutFile afnOutFile = new AFNOutFile(arjNumber);
            arjFiles.Add(afnOutFile);
            info.SetProgressRanges(Directory.GetFiles(workDir).Length);
            foreach (string fileName in Directory.GetFiles(workDir))
            {
                if(numberFilesInArj>=50)
                {
                    ++arjFilesCount;
                    numberFilesInArj = 0;
                    afnOutFile = new AFNOutFile(++this.arjNumber);
                    arjFiles.Add(afnOutFile);
                }
                afnOutFile.xmlFiles.Add(fileName);
                numberFilesInArj++;

            }
            // теперь у нас есть массив arjFiles (*.arj), в которые надо поместить файлы *.xml
            // PBx подписываем, остальное - шифруем
            //   P.S. arjName - без пути, в arj.xmlFiles - полные пути
            info.SetText("Подпись и шифрование файлов...");
            foreach (AFNOutFile arj in arjFiles)
            {
                foreach(string xmlFile in arj.xmlFiles)
                {
                    // для кажд.файла - подписать
                    (resCode, resString) = Signature.Sign(xmlFile);
                    if(resCode!=Signature.SUCCESS)
                    {
                        break;
                    }
                    string shortXmlName = Path.GetFileName(xmlFile).ToUpper();
                    if (shortXmlName.Substring(0, 2) != "PB")
                    {
                        (resCode, resString) = Signature.Encrypt(xmlFile,Config.FNSKey);
                        if (resCode != Signature.SUCCESS)
                        {
                            errorMessage += resString;
                            IsSuccess = false;
                            break;
                        } 
                        else
                            _ = xmlFile.ToUpper().Replace(".XML", ".VRB");
                    }
                    info.UpdateProgress();
                }
            }
            if (!String.IsNullOrEmpty(errorMessage) || !IsSuccess)
                return;
            // теперь в WORK у нас смесь .xml и .vrb
            // их надо упаковать в arj и поместить файлы в OUT
            // файлы скидываем в files.lst и списком архивируем
            string outDir = Config.OutDir;
            errorMessage = "";
            string[] arjBkp = new string[arjFilesCount];
            int arjNo = 0;
            info.SetText("Архивирование....");
            info.SetProgressRanges(arjFilesCount);
            foreach (AFNOutFile arj in arjFiles)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string xmlFile in  arj.xmlFiles)
                {
                    sb.Append(xmlFile);
                    sb.Append(Environment.NewLine);
                }
                File.WriteAllText("files.lst", sb.ToString());
                string outName = outDir + "\\" + arj.arjName;
                if (File.Exists(outName))
                    File.Delete(outName);
                arjBkp[arjNo++] = outName;
                Process ps = new Process();
                ps.StartInfo.FileName = "arj32.exe";
                ps.StartInfo.Arguments = "a -e " + outName + " !files.lst";
                ps.StartInfo.UseShellExecute = false;
                ps.Start();
                ps.WaitForExit();
                ps.Dispose();
                if(File.Exists(outName))
                {
                    (resCode, resString) = Signature.Sign(outName);
                    if (resCode != Signature.SUCCESS)
                    {
                        errorMessage += resString+"\n";
                        IsSuccess = false;
                        break;
                    }
                }
                info.UpdateProgress();
            }
            if (IsSuccess)
            {
                Util.CleanDirectory(Config.WorkDir);
                // стоит ли делать backup arj-файлов?
                Util.CopyFilesToBackupDirectory(Config.WorkDir, arjBkp);
                // формируем сообщение
                StringBuilder sb = new StringBuilder();
                sb.Append("Подготовлены файлы:\n");
                foreach(AFNOutFile o in arjFiles)
                {
                    sb.Append(o.arjName);
                    sb.Append(Environment.NewLine);
                }
                errorMessage = sb.ToString();
            }
           
        }

        public void Dispose()
        {
            if (this.info != null)
                info.Close();
            try
            {
                File.Delete("files.lst");
            }
#pragma warning disable CA1031 // Не перехватывать исключения общих типов
            catch { }
#pragma warning restore CA1031 // Не перехватывать исключения общих типов

        }


    }
}
