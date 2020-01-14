using System;
using System.Collections;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.Pkcs;


namespace cp365
{
   
    class PTKPSD
    {
        public string errorMessage;
        // файлы из INI
        private string ODBC_DSN; // имя ODBC
        private string tmpDir;
        private string archPostDir; // Post\Store\
        // other private members
        private string connectionString;
        private IniFile ini;

        public PTKPSD(string iniFileName)
        {
            try
            {
                ini = new IniFile(iniFileName);
                this.ODBC_DSN = ini.Read("ODBC", "DataBase");
                this.tmpDir = ini.Read("TMP", "Path");
                this.archPostDir = ini.Read("ARCHIVESTORE", "Path");
                if (String.IsNullOrEmpty(ODBC_DSN) || String.IsNullOrEmpty(tmpDir) || String.IsNullOrEmpty(archPostDir))
                    throw new Exception("Неверный файл " + iniFileName);
                
                //this.connectionString = "Provider = Microsoft.Jet.OLEDB.4.0; DSN = " + this.ODBC_DSN+ ";User Id=admin;Password=;";
                //this.connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="+dbPath+
                    //";User Id=admin;Password=;";
                this.connectionString = "DSN=" + this.ODBC_DSN;
            } catch (Exception e)
            {
                this.errorMessage = e.Message;
            }

        }


        public List<MZFile> GetMzFiles(DateTime dateFrom, DateTime dateTo, out string errorMessage)
        {

            string sqlTemplate =
                "SELECT elo_arh_post.filename as filename, elo_arh_post.dt as dt, "+
                "elo_arh_post.state_, elo_spr_state.name_st, " +
                "elo_arh_post.error_, elo_spr_err.ErrText "+
        "FROM((elo_arh_post "+
          "INNER JOIN elo_spr_err ON elo_arh_post.error_ = elo_spr_err.ErrCod) "+
          "INNER JOIN elo_spr_state ON elo_arh_post.state_ = elo_spr_state.kot_st) "+
        "WHERE(elo_arh_post.posttype = 'mz') AND (elo_arh_post.filetype = 'ИЭС2') "+
              //              " AND (elo_arh_post.dt BETWEEN #@1# AND #@2#) "+
              " AND (elo_arh_post.dt BETWEEN @start AND @end) " +
              "ORDER BY elo_arh_post.dt";
            // 0:filetype, 1:dt, 2:filename, 3:state, 4:stateText, 5: err, 6:err_text
            // даты в виде "#mm/dd/YYYY#"
            string strSQL = sqlTemplate.Replace("@start", "#"+Util.DateToSQL(dateFrom)+"#").Replace("@end", "#"+Util.DateToSQL(dateTo)+"#");
            List<MZFile> result = new List<MZFile>();
            errorMessage =null;
            // надо вывести: mzName, fileName, дата/время
            try
            {
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    connection.Open();
                    // Я не победил DateTime в параметрах, поэтому через строки
#pragma warning disable CA2000 // Ликвидировать объекты перед потерей области
                    OdbcCommand cmd = new OdbcCommand(strSQL, connection);
#pragma warning restore CA2000 // Ликвидировать объекты перед потерей области
                    using (OdbcDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            string mzfile = reader["filename"].ToString(); // полное имя файла mz (напр. "mzr01_08.700.100111")
                            DateTime mzDate = (DateTime)reader["dt"];
                            string mzDecodedName = DecodeMZ(mzfile, mzDate); // may be null
                            MZFile mz = new MZFile(mzDecodedName);
                            if(mz!=null)
                            {
                                string err = null;
                                if (!File.Exists(mzDecodedName))
                                {
                                    err = "Не найден";
                                }
                                else
                                {
                                    if (!mz.IsAFN())
                                    {
                                        err = "Без AFN";
                                        mz.valid = false;
                                    }
                                }
                                mz.mzFileDate = mzDate;
                                mz.mzErr = err;
                                result.Add(mz);
                            } else
                            {
                                errorMessage+="Ошибка обработки "+mzfile+"\n";
                            }
                        }
                        
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                errorMessage += e.Message;
                return null;
            }
        }

        // Снять ЭЦП с файла и поместить его в каталог TEMP ПТК ПСД
        private string DecodeMZ(string fileName, DateTime fileDate)
        {
            //string subdir = string.Format("%4d\\%02d\\%02d", dt.Year, dt.Month, dt.Day); // YYYY\MM\DD
            string subdir = string.Format("{0}\\{1:D2}\\{2:D2}\\", fileDate.Year, fileDate.Month, fileDate.Day); // YYYY\MM\DD\
            string fullInPath = this.archPostDir + subdir + fileName;
            string outName = this.tmpDir + fileName.Substring(0, 12);
            if (!File.Exists(fullInPath))
                return null;
            byte[] fileContent = File.ReadAllBytes(fullInPath);
            SignedCms cms = new SignedCms();
            cms.Decode(fileContent);
            File.WriteAllBytes(outName, cms.ContentInfo.Content);
            return outName;
        }
    }
}
