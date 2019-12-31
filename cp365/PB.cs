using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

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
        private string filNumber;
        public PB(string srcFileName, PBTYPE type, string errorCode="01", string errorMessage="", string fil="0000")
        {
            this.sourceName = Path.GetFileNameWithoutExtension(srcFileName).ToUpper();
            this.pbType = type;
            this.pbErrCode = errorCode;
            this.pbMessage = errorMessage;
            this.filNumber = fil;
            if(pbType == PBTYPE.GOOD)
              this.pbFileName = "PB1" + "_" + this.sourceName+".xml";
            else
                this.pbFileName = "PB2" + "_" + this.sourceName+"_"+this.filNumber+".xml";
            /*
            switch(type)
            {
                case PBTYPE.GOOD:
                    xmlText = "<?xml version=\"1.0\" encoding=\"windows-1251\"?>"+
       "<Файл ИдЭС=\""+Util.GUID()+"\" ТипИнф=\"ПОДБНПРИНТ\" ВерсПрог=\"cp365 1.0\""+
       " ТелОтпр=\""+Config.TelOtpr+"\" ДолжнОтпр=\""+Config.DolOtpr+"\" ФамОтпр=\"" + Config.FamOtpr+"\" ВерсФорм=\""+Config.FORM_VERSION+
       "\">"+
       "<ПОДБНПРИНТ ИмяФайла=\""+srcFileName + "\" ДатаВремяПроверки = \"" + Util.XMLDate(DateTime.Now) + "\">" +
       " <Результат КодРезПроверки=\"01\"/>" +
       "</ПОДБНПРИНТ>" +
       "</Файл>";
                    break;
                case PBTYPE.ERROR:
                    xmlText = "<?xml version=\"1.0\" encoding=\"windows-1251\"?>" +
       "<Файл ИдЭС=\"" + Util.GUID() + "\" ТипИнф=\"ПОДБНПРИНТ\" ВерсПрог=\""+Config.ProgramVersion()+
       "\" ТелОтпр=\"" + Config.TelOtpr + "\" ДолжнОтпр=\"" + Config.DolOtpr + "\" ФамОтпр=\"" + Config.FamOtpr + "\" ВерсФорм=\"" + Config.FORM_VERSION +
       "\">" +
       "<ПОДБНПРИНТ ИмяФайла=\"" + srcFileName + "\" ДатаВремяПроверки = \"" + Util.XMLDate(DateTime.Now) + "\">" +
       " <Результат КодРезПроверки=\""+errorCode+"\" Пояснение=\""+errorMessage+"\"/>" +
       "</ПОДБНПРИНТ>" +
       "</Файл>";
                    break;

            }
            */
            if (type == PBTYPE.GOOD)
                this.xmlText = GetPBContent(srcFileName);
            else
                this.xmlText = GetPBContent(srcFileName, errorCode, errorMessage);
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

        // вариант через XML
        public string GetPBContent(string srcFileName, string errorCode = "01", string errorMessage = "У банка отозвана лицензия")
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration xmlDecl = xml.CreateXmlDeclaration("1.0", "windows-1251", null);
            xml.AppendChild(xmlDecl);
            XmlElement root = xml.CreateElement("Файл");
            xml.AppendChild(root);
            //root.GetAttribute("xmlns", "urn:cbr-365Р:stop: v.3.00"); // это не используется, убрано в 440-П
            root.SetAttribute("ИдЭС", Util.GUID());
            root.SetAttribute("ТипИнф", "ПОДБНПРИНТ");
            root.SetAttribute("ВерсПрог", Config.ProgramVersion());
            root.SetAttribute("ТелОтпр", Config.TelOtpr);
            root.SetAttribute("ДолжнОтпр", Config.DolOtpr);
            root.SetAttribute("ФамОтпр", Config.FamOtpr);
            root.SetAttribute("ВерсФорм", Config.FORM_VERSION);
            XmlElement body = xml.CreateElement("ПОДБНПРИНТ");
            root.AppendChild(body);
            body.SetAttribute("ИмяФайла", this.sourceName);
            body.SetAttribute("ДатаВремяПроверки", Util.XMLDateTime(DateTime.Now));
            XmlElement result = xml.CreateElement("Результат");
            body.AppendChild(result);
            result.SetAttribute("КодРезПроверки", errorCode);
            if(errorCode != "01")
            {
                result.SetAttribute("Пояснение", errorMessage);
            }
            return xml.InnerXml;
        }
    }

    public enum PBTYPE
    {
        GOOD = 1,
        ERROR = 2
    }
}
