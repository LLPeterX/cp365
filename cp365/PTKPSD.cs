using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
     */
    class PTKPSD
    {
        private string iniPath;
        //private string archPostDir; // не подходит, т.к. файлы зашифрованы ключом ПТК, к которому нет доступа
        private string eloDir;
        private string outPostDir;
        //private string dbPath;
        private IniFile iniFile;
        private string tmpDir;
        private string DSN;
        private string connectionString;

        public PTKPSD(string iniPath)
        {
            iniFile = new IniFile(iniPath);
            this.iniPath = iniPath;
            this.DSN = iniFile.Read("ODBC", "DataBase");
            this.connectionString = "Provider = Microsoft.Jet.OLEDB.4.0; DSN = " + this.DSN+ ";User Id=admin;Password=;";
            this.tmpDir = iniFile.Read("TMP", "Path");
            this.outPostDir = iniFile.Read("OUTPOST", "Path");
            //this.archPostDir = iniFile.Read("ARCHIVESTORE", "Path");
            this.eloDir = GetELODir();

        }

        private string GetELODir()
        {
            string strSQL = "SELECT path_out FROM elo_path WHERE ecp='check'";
            string elo_dir = null;
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                OleDbCommand cmd = new OleDbCommand(strSQL, conn);
                using (OleDbDataReader reader = cmd.ExecuteReader())
                {
                    elo_dir = (String)reader[0];
                }
            }
            return elo_dir;
        }

    }
}
