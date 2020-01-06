using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cp365
{
    class AFNOutFile
    {
        public string arjName;
        public List<string> xmlFiles;

        public AFNOutFile(int seqNo)
        {
            // AFN_7908700_MIFNS00_20200106_00001.arj
            this.arjName = "AFN_" +
              Config.BIK.Substring(2) + "_" +
              "MIFNS00_" +
              DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) + "_" +
              seqNo.ToString("D5") +
              ".ARJ";
            xmlFiles = new List<string>();
        }

        // private constructor
        private AFNOutFile() { }

    }
}
