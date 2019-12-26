using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace cp365
{
    public partial class FormConfig : Form
    {
        public FormConfig()
        {
            InitializeComponent();
            this.workdir.Text = Config.WorkDir;
            this.tempdir.Text = Config.TempDir;
            this.indir.Text = Config.InDir;
            this.outdir.Text = Config.OutDir;
            this.afndir.Text = Config.AFNDir;
            this.invdir.Text = Config.INVDir;
            this.xsddir.Text = Config.XSDDir;
            this.ptkini.Text = Config.PTKPath;
            this.bik.Text = Config.BIK;
            this.fil.Text = Config.Filial;
            this.profile.Text = Config.Profile;
            this.fnskey.Text = Config.FNSKey;
            this.lastNum.Text = Config.SerialNum.ToString();
            this.lastDate.Text = Util.DateToYMD(Config.SerialDate);
            this.virtualFDD.Checked = Config.UseVirtualFDD;
            this.usePTK.Checked = Config.UsePTK;
            this.checkXSD.Checked = Config.UseXSD; 

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Config.WorkDir = this.workdir.Text.Trim(); 
            Config.TempDir = this.tempdir.Text.Trim();
            Config.InDir = this.indir.Text.Trim(); ;
            Config.OutDir = this.outdir.Text.Trim(); 
            Config.AFNDir = this.afndir.Text.Trim();
            Config.INVDir = this.invdir.Text;
            Config.XSDDir = this.xsddir.Text;
            Config.PTKPath = this.ptkini.Text;
            Config.BIK = this.bik.Text;
            try
            {
                int tmp = Convert.ToInt32(this.fil.Text);
                Config.Filial = this.fil.Text;
            } catch
            {
                Config.Filial = "0";
            }
            Config.Profile = this.profile.Text.Trim();
            Config.FNSKey = this.fnskey.Text;
            Config.SerialNum = Convert.ToInt32(this.lastNum.Text);
            Config.SerialDate = Util.DateFromYMD(this.lastDate.Text);
            Config.UseVirtualFDD = this.virtualFDD.Checked;
            Config.UsePTK = this.usePTK.Checked && File.Exists(this.ptkini.Text);
            Config.UseXSD = this.checkXSD.Checked && Directory.Exists(this.xsddir.Text);

            Config.CreateDirectories(); // создаем необходимые каталоги

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
