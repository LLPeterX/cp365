using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace cp365
{
    /*
     * Класс для формирования ответных файлов PB1 или PB2
     */
    public class PB
    {
        private string xmlText;
        private string pbFileName;
        private string sourceName;
        private PBTYPE pbType;
        private string pbErrCode;
        private string pbMessage;
        public PB(string fileName, PBTYPE type, string errorCode="01", string errorMessage="")
        {
            this.sourceName = Path.GetFileName(fileName).ToUpper();
            this.pbType = type;
            this.pbErrCode = errorCode;
            this.pbMessage = errorMessage;
            this.pbFileName = "PB" + type.ToString("d") + "_" + this.sourceName;
            switch(type)
            {
                case PBTYPE.GOOD:
                    xmlText = "<?xml version=\"1.0\" encoding=\"windows-1251\"?>"+
       "<Файл ИдЭС=\""+Util.GUID()+"\" ТипИнф=\"ПОДБНПРИНТ\" ВерсПрог=\"cp365 1.0\""+
       " ТелОтпр=\""+Config.TelOtpr+"\" ДолжнОтпр=\""+Config.DolOtpr+"\" ФамОтпр=\"" + Config.FamOtpr+"\" ВерсФорм=\""+Config.VersForm+
       "\">"+
       "<ПОДБНПРИНТ ИмяФайла=\""+fileName + "\" ДатаВремяПроверки = \"" + Util.XMLDate(DateTime.Now) + "\">" +
       " <Результат КодРезПроверки=\"01\"/>" +
       "</ПОДБНПРИНТ>" +
       "</Файл>";
                    break;
                case PBTYPE.ERROR:
                    xmlText = "<?xml version=\"1.0\" encoding=\"windows-1251\"?>" +
       "<Файл ИдЭС=\"" + Util.GUID() + "\" ТипИнф=\"ПОДБНПРИНТ\" ВерсПрог=\"cp365 1.0\"" +
       " ТелОтпр=\"" + Config.TelOtpr + "\" ДолжнОтпр=\"" + Config.DolOtpr + "\" ФамОтпр=\"" + Config.FamOtpr + "\" ВерсФорм=\"" + Config.VersForm +
       "\">" +
       "<ПОДБНПРИНТ ИмяФайла=\"" + fileName + "\" ДатаВремяПроверки = \"" + Util.XMLDate(DateTime.Now) + "\">" +
       " <Результат КодРезПроверки=\""+errorCode+"\" Пояснение=\""+errorMessage+"\"/>" +
       "</ПОДБНПРИНТ>" +
       "</Файл>";
                    break;

            }
        }
        override public string ToString()
        {
            return this.xmlText;
        }

        public void Save(String workdir)
        {
            string outFileName = workdir + "\\" + this.pbFileName;
            File.WriteAllText(outFileName, this.xmlText, Encoding.GetEncoding(1251));
        }
    }

   

    public enum PBTYPE
    {
        GOOD = 1,
        ERROR = 2
    }
}
