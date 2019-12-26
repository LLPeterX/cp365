﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace cp365
{
    public static class Util
    {
        public static string DateToYMD(DateTime d)
        {
            if (d == null)
                return DateTime.Now.ToString("yyyyMMdd");
            return d.ToString("yyyyMMdd");
            
        }

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

        // удалить все файлы из зданного каталога
        public static void CleanDirectory(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            foreach(string fileName in files)
            {
                File.Delete(fileName);
            }
        }

    }
}
