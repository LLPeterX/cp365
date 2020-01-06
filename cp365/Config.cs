using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace cp365
{
    // выбрать одно из:
    // - IniFile https://stackoverflow.com/questions/217902/reading-writing-an-ini-file
    // - IniManager http://plssite.ru/csharp/csharp_ini_files_article.html
    // и т.д. и т.п.
    public static class Config
    {
        private static IniFile iniFile = null;
        private const string DEFAULT_DIR = @".\";
        private const string CONFIG_FILENAME = @".\cp365.ini";
        public const string PROGRAM_NAME = "cp365";
        public const string PROGRAM_VERSION = "1.0";
        public const string FORM_VERSION = "3.00";
        // private members

        public static string TempDir
        {
            get
            {
                return GetValue("Paths", "TEMP", Path.GetFullPath(DEFAULT_DIR+"TEMP"));
            }
            set
            {
                SetValue("Paths", "TEMP", value);
            }
        }
        public static string WorkDir
        {
            get
            {
                return GetValue("Paths", "WORK", Path.GetFullPath(DEFAULT_DIR + "WORK"));
            }
            set
            {
                SetValue("Paths", "WORK", value);
            }
        }
        public static string InDir
        {
            get
            {
                return GetValue("Paths", "IN", Path.GetFullPath(DEFAULT_DIR +"IN"));
            }
            set
            {
                SetValue("Paths", "IN", value);
            }
        }
        public static string OutDir
        {
            get
            {
                return GetValue("Paths", "OUT", Path.GetFullPath(DEFAULT_DIR + "OUT"));
            }
            set
            {
                SetValue("Paths", "OUT", value);
            }
        }
        public static string AFNDir
        {
            get
            {
                return GetValue("Paths", "AFN_FILES", Path.GetFullPath(DEFAULT_DIR +"AFN_IN"));
            }
            set
            {
                SetValue("Paths", "AFN_FILES", value);
            }
        }
        public static string INVDir
        {
            get
            {
                return GetValue("Paths", "TO_INV", Path.GetFullPath(DEFAULT_DIR +"TO_INV"));
            }
            set
            {
                SetValue("Paths", "TO_INV", value);
            }
        }
        public static string XSDDir
        {
            get
            {
                return GetValue("Paths", "XSD", "");
            }
            set
            {
                SetValue("Paths", "XSD", value);
            }
        }
        public static string PTKDatabase
        {
            get
            {
                return GetValue("PTK", "Database", "");
            }
            set
            {
                SetValue("PTK", "Database", value);
            }
        }
        public static string ELODir
        {
            get
            {
                return GetValue("PTK", "ELO Directory", "");
            }
            set
            {
                SetValue("PTK", "ELO Directory", value);
            }
        }
        //public static string PTKiniFile
        //{
        //    get
        //    {
        //        return GetValue("PTK", "INI file", "");
        //    }
        //    set
        //    {
        //        SetValue("PTK", "INI file", value);
        //    }
        //}
        public static string BIK
        {
            get
            {
                return GetValue("Bank", "BIK", "");
            }
            set
            {
                SetValue("Bank", "BIK", value);
            }
        }
        public static string Filial
        {
            get
            {
                return GetValue("Bank", "FilialNumber", "0");
            }
            set
            {
                SetValue("Bank", "FilialNumber", value);
            }
        }
        public static string TelOtpr
        {
            get
            {
                return GetValue("Bank", "Phone", "");
            }
            set
            {
                SetValue("Bank", "Phone", value);
            }
        }
        public static string DolOtpr
        {
            get
            {
                return GetValue("Bank", "Sender State", "");
            }
            set
            {
                SetValue("Bank", "Sender State", value);
            }
        }
        public static string FamOtpr
        {
            get
            {
                return GetValue("Bank", "Sender Family", "");
            }
            set
            {
                SetValue("Bank", "Sender Family", value);
            }
        }
        public static bool UsePTK
        {
            get
            {
                if (!File.Exists(PTKDatabase))
                    return false;
                if (GetValue("PTK", "UsePTK", "0") == "0")
                    return false;
                return true;
            }
            set
            {
                SetValue("PTK", "UsePTK", value && File.Exists(PTKDatabase) ? "1" : "0");
            }
        }
        public static bool UseVirtualFDD
        {
            get
            {
                if (GetValue("Other", "UseVirtialFDD", "0") == "1")
                    return true;
                return false;
            }
            set
            {
                SetValue("Other", "UseVirtialFDD", value ? "1" : "0");
            }
        }
        public static string Profile
        {
            get
            {
                return GetValue("Crypto", "Profile", "IAS");
            }
            set
            {
                SetValue("Crypto", "Profile", value);
            }
        }
        public static string FNSKey
        {
            get
            {
                return GetValue("Crypto", "FNS key", "");
            }
            set
            {
                SetValue("Crypto", "FNS key", value);
            }
        }

        // прочие переменные
        public static int SerialNum
        {
            get
            {
                return Convert.ToInt32(GetValue("NoModify", "Serial Number", "0"));
            }
            set
            {
                SetValue("NoModify", "Serial Number", value.ToString());
            }
        }
        public static string SerialDate
        {
            get
            {
                //return Util.DateFromYMD(GetValue("NoModify", "Serial Date", ""));
                string strToday = DateTime.Now.ToString("yyyyMMdd");
                return GetValue("NoModify", "Serial Date", strToday);
            }
            set
            {
                //SetValue("NoModify", "Serial Date", Util.DateToYMD(value));
                SetValue("NoModify", "Serial Date", value);
            }
        }

        public static bool UseXSD
        {
            get
            {
                if (GetValue("Other", "Check via XSD", "0") == "1")
                    return true;
                return false;
            }
            set
            {
                SetValue("Other", "Check via XSD", value ? "1" : "0");
            }
        }
        public static bool CreatePB1
        {
            get
            {
                if (GetValue("Other", "Create PB1", "0") == "1")
                    return true;
                return false;
            }
            set
            {
                SetValue("Other", "Create PB1", value ? "1" : "0");
            }
        }
        public static bool NoLicense
        {
            get
            {
                if (GetValue("Other", "No License", "0") == "1")
                    return true;
                return false;
            }
            set
            {
                SetValue("Other", "No License", value ? "1" : "0");
            }
        }
        // --------------- методы ------------------------------------
        // Получить значение (строковое) из ini-файла
        private static string GetValue(string section, string key, string default_value) { 
            if(iniFile == null)
            {
                iniFile = new IniFile(CONFIG_FILENAME);
            }
            if (iniFile.KeyExists(key, section))
                return iniFile.Read(key, section).Trim();
            else return default_value;
        }
        // Получить значение (целое) из ini-файла
        private static int GetValue(string section, string key, int  default_value)
        {
            int retValue = 0;
            if (iniFile == null)
            {
                iniFile = new IniFile(CONFIG_FILENAME);
            }
            string value = GetValue(section, key, "0");
            try
            {
                retValue = Convert.ToInt32(value);
            } catch
            {
                retValue = default_value;
            }
            return retValue;
        }

        // Запомнить указанное значение в ini-файле
        private static void SetValue(string section, string key, string value)
        {
            if (iniFile == null)
            {
                iniFile = new IniFile(CONFIG_FILENAME);
            }
            iniFile.Write(key, value.Trim(), section);
        }

        private static void SetValue(string section, string key, int value)
        {
            if (iniFile == null)
            {
                iniFile = new IniFile(CONFIG_FILENAME);
            }
            iniFile.Write(key, value.ToString(), section);
        }

        // Проверить наличие нужных внешних EXE-файлов (arj, сигнатура, imdisk)
          public static bool CheckFiles()
        {
            if(!File.Exists("ARJ32.exe"))
            {
                MessageBox.Show("Нет файла arj32.exe");
                return false;
            }
            // ниже спорно. Можно бы взять из Signature, но т.к. м.б. x32 и x64 гемор определять где находится spki1utl.exe
            // так что лучше скопировать spki1utl.exe в рабочий каталог.
            // В будущем переделаю
            if (!File.Exists("spki1utl.exe"))
            {
                MessageBox.Show("Нет файла spki1utl.exe");
                return false;
            }
            // не очень. В конфиге исп-е imdisk м.б. указано позже
            if(Config.UseVirtualFDD && !File.Exists("imdisk.exe"))
            {
                MessageBox.Show("Указано использование виртуального FDD\nНо нет файла IMDISK.EXE");
                return false;
            }
            
            return true;

        }

        public static string ProgramVersion()
        {
            return PROGRAM_NAME + " " + PROGRAM_VERSION;
        }

    } // config  


}
