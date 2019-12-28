using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace cp365
{
    public partial class FormMain
    {
        public void processPTK(object sender, EventArgs e)
        {

            PTKPSD ptk;
            if (Config.UsePTK && File.Exists(Config.PTKDatabase))
            {
                ptk = new PTKPSD(Config.PTKDatabase);
                string errorMessage;
                if(!ptk.IsSuccess(out errorMessage))
                {
                    MessageBox.Show("Не удалось получить доступ к ПТК ПСД\n" + errorMessage, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } else {
                return;
            }
            // Получить список файлов mz*
            
        }


    }
}
