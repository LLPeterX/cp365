﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace cp365
{
    class AFNOutputProcessor
    {
        private string workDir;
        private List<AFNOutFile> arjFiles;
        public bool IsSuccess = true;
        private int arjNumber; // порядковый номер файла ARJ за тек.дату
        public int arjCount; // общее число файлов ARJ
        private FormMessage info;
        
        public AFNOutputProcessor(int porNo)
        {
            this.workDir = Config.WorkDir;
            this.arjFiles = new List<AFNOutFile>();
            this.arjNumber = porNo; // начальный N архива
            this.arjCount = 1; // кол-во сформированных ARJ-файлов
            info = new FormMessage();
            info.ShowTitle("Подготовка файлов");
            info.Show();
        }

        public void Process(out string errorMessage)
        {
            // Обработка файлов в каталоге WORK
            info.ShowInfo("Инициализация Сигнатуры");
            info.SetProgressRanges(1);
            if (!Signature.Initialize())
            {
                errorMessage = "Ошибка инициализации СКАД \"Сигнатура\"";
                IsSuccess = false;
                info.Close();
                return;
            }
            errorMessage = null;
            int xmlInArj = 0;
            Util.CopyFilesToBackupDirectory(workDir); // backup to WORK\yyymmdd\
            AFNOutFile aout = new AFNOutFile(arjNumber);
            arjFiles.Add(aout);
            string err = "";
            info.SetProgressRanges(Directory.GetFiles(workDir).Length);
            foreach (string fileName in Directory.GetFiles(workDir))
            {
                if(xmlInArj>=50)
                {
                    ++arjCount;
                    xmlInArj = 0;
                    aout = new AFNOutFile(++this.arjNumber);
                    arjFiles.Add(aout);
                }
                aout.xmlFiles.Add(fileName);
                xmlInArj++;

            }
            // теперь у нас есть массив arjFiles с файлами xml
            // PBx подписываем, остальное - шифруем
            //   P.S. arjName - без пути, в arj.xmlFiles - полные пути
            info.ShowInfo("Подпись и шифрование файлов...");
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
                            IsSuccess = false;
                        } 
                        else
                            xmlFile.ToUpper().Replace(".XML", ".VRB");
                    }
                    info.UpdateProgress();
                }
            }
            if (!String.IsNullOrEmpty(errorMessage) || !IsSuccess)
                return;
            // теперь в WORK у нас смесь .xml и .vrb
            // их надо упаковать в arj и поместить файлы в OUT
            // файлы скидываем в files.lst и списском фрхивируем
            string outDir = Config.OutDir;
            errorMessage = "";
            string[] arjBkp = new string[arjCount];
            int arjNo = 0;
            info.ShowInfo("Архивирование....");
            info.SetProgressRanges(arjCount);
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
                if (File.Exists(outName))
                    File.Delete(outName);
                arjBkp[arjNo++] = outName;
                Process ps = new Process();
                ps.StartInfo.FileName = "arj32.exe";
                ps.StartInfo.Arguments = "a -e " + outName + " !files.lst";
                ps.StartInfo.UseShellExecute = false;
                ps.Start();
                ps.WaitForExit();
                if(File.Exists(outName))
                {
                    if (Signature.Sign(outName, out err) != 0)
                    {
                        errorMessage += err;
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
                    sb.Append('\n');
                }
                errorMessage = sb.ToString();
            }
            info.Close();
            try
            {
                File.Delete("files.lst");
            }
            catch { }
        }

        public void Dispose()
        {
            if (this.info != null)
                info.Close();

        }

        
    }
    // TODO: проверить формирование файлов с BOS,BV и т.п. - которые следует шифровать
}
