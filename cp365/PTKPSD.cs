using System;
using System.Collections;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace cp365
{
    /*
     *  Класс для работы с файлом etalon97.mdb из ПТК ПСД
     *  Connection:
     *  Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\ptk\Database\etalon97.mdb
     *  Нас интересует только табица elo_arh_post
     *  и файлы из Post\ELO\ (путь берется селектом из ПТК ПСД)
     *  Внимание! заключительный слэш!
     *  
     *  выборка посылок mz за диапазон:
     *  SELECT     filetype, posttype, dt, filename, pathname, state_, bik, error_, repdate, nkod
           FROM         elo_arh_post
           WHERE     (posttype = 'mz') AND (dt BETWEEN #12/24/2019# AND #12/27/2019#)

      * Точнее:
      
       SELECT     elo_arh_post.filetype, elo_arh_post.posttype, elo_arh_post.dt, elo_arh_post.filename, elo_arh_post.state_, elo_spr_state.name_st, elo_arh_post.bik, 
                      elo_arh_post.error_, elo_spr_err.ErrText, elo_arh_post.nkod
        FROM         ((elo_arh_post INNER JOIN
                      elo_spr_err ON elo_arh_post.error_ = elo_spr_err.ErrCod) INNER JOIN
                      elo_spr_state ON elo_arh_post.state_ = elo_spr_state.kot_st)
        WHERE     (elo_arh_post.posttype = 'mz') AND (elo_arh_post.dt BETWEEN #12/24/2019# AND #12/27/2019#)

        Параметры запроса см. https://stackoverflow.com/questions/37883432/select-data-from-ms-access-database-by-date-in-c-sharp
        Но мы будем юзать тормозной ODBC, созданный для ПТК ПСД
     */
    class PTKPSD
    {
        //private string archPostDir; // не подходит, т.к. файлы зашифрованы ключом ПТК, к которому нет доступа
        public string eloDir;
        private string dbPath;
        private string connectionString;
        public string errorMessage;

        public PTKPSD(string dbName)
        {
            try
            {
                //this.connectionString = "Provider = Microsoft.Jet.OLEDB.4.0; DSN = " + this.DSN+ ";User Id=admin;Password=;";
                this.dbPath = dbName;
                this.connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="+dbPath+
                    ";User Id=admin;Password=;";
                //this.connectionString = "DSN=" + this.DSN;
                //this.tmpDir = iniFile.Read("TMP", "Path");
                //this.outPostDir = iniFile.Read("OUTPOST", "Path");
                this.eloDir = GetELODir();
            } catch (Exception e)
            {
                this.eloDir = null;
                this.errorMessage = e.Message;
            }

        }

        // В ELO только входящие файлы - они нас и интересуют
        private string GetELODir()
        {
            /*string strSQL = "SELECT path_out FROM elo_path WHERE (ecp='check')";
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    OleDbCommand cmd = new OleDbCommand(strSQL, connection);
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        //return reader[0].ToString();
                        return reader[0].ToString().Replace("N:", "C:"); // for local testing
                    }
                }
            } catch (Exception e)
            {
                this.errorMessage = e.Message;
                return null;
            }*/
            return Config.ELODir;
        }

        // Проврерка, что экземпляр создан успешно
        public bool IsSuccess(out string errorMsg)
        {
            if(this.eloDir != null)
            {
                errorMsg = null;
                return true;
            }
            errorMsg = this.errorMessage;
            return false;
        }

        public string GetPackagesDirectory()
        {
            return this.eloDir;
        }

        // параметры - строковые в виде MM/dd/yyyy
        //public List<MZFile> GetMzFiles(string dateFrom, string dateTo, out string errorMessage)
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
            //string strSQL = sqlTemplate.Replace("@1", Util.DateToSQL(dateFrom)).Replace("@2", Util.DateToSQL(dateTo));
            List<MZFile> result = new List<MZFile>();
            errorMessage =null;
            // надо вывести: mzName, fileName, дата/время
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    //OleDbCommand cmd = new OleDbCommand(strSQL, connection);
                    OleDbCommand cmd = new OleDbCommand(sqlTemplate, connection);
                    cmd.Parameters.AddWithValue("@start", dateFrom);
                    cmd.Parameters.AddWithValue("@end", dateTo);
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            string mzfile = reader["filename"].ToString();
                            string fullPath = eloDir + "\\"+mzfile.Substring(0, 12); // с расширением, но отбрасываем лишнее расширение
                            MZFile mz = new MZFile(fullPath);
                            if(mz!=null)
                            {
                                string err = null;
                                if (!mz.IsAFN())
                                {
                                    err = "Без AFN";
                                    mz.valid = false;
                                }
                                mz.mzFileDate = (DateTime)reader["dt"];
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
    }
}
