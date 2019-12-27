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
            this.ptkdb.Text = Config.PTKDatabase;
            this.bik.Text = Config.BIK;
            this.fil.Text = Config.Filial;
            this.profile.Text = Config.Profile;
            this.fnskey.Text = Config.FNSKey;
            this.lastNum.Text = Config.SerialNum.ToString();
            this.lastDate.Text = Util.DateToYMD(Config.SerialDate);
            this.virtualFDD.Checked = Config.UseVirtualFDD;
            this.usePTK.Checked = Config.UsePTK;
            this.checkXSD.Checked = Config.UseXSD;
            this.makePB1.Checked = Config.CreatePB1;
            this.noLicense.Checked = Config.NoLicense;
            this.dolgn.Text = Config.DolOtpr;
            this.family.Text = Config.FamOtpr;
            this.tel.Text = Config.TelOtpr;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (CheckDirectory(this.workdir.Text.Trim()))
                Config.WorkDir = this.workdir.Text.Trim();
            else return;
            if (CheckDirectory(this.tempdir.Text.Trim()))
                Config.TempDir = this.tempdir.Text.Trim();
            else return;
            if (CheckDirectory(this.indir.Text.Trim()))
                Config.InDir = this.indir.Text.Trim();
            else return;
            if (CheckDirectory(this.outdir.Text.Trim()))
                Config.OutDir = this.outdir.Text.Trim();
            else return;
            if (CheckDirectory(this.afndir.Text.Trim()))
                Config.AFNDir = this.afndir.Text.Trim();
            else return;
            Config.INVDir = this.invdir.Text; // может быть пустой - тогда не копировать
            Config.XSDDir = this.xsddir.Text; // может быть пустой - тогда не проверять
            Config.PTKDatabase = this.ptkdb.Text; // может быть пустой - тогда без ПТК ПСД
            if (this.bik.Text.Length != 9 || !this.bik.Text.StartsWith("04"))
            {
                MessageBox.Show("Неверный БИК", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Config.BIK = this.bik.Text;
            try
            {
                int tmp = Convert.ToInt32(this.fil.Text);
                Config.Filial = this.fil.Text;
            } catch
            {
                Config.Filial = "0";
            }
            //Config.Profile = this.profile.Text.Trim();
            //Config.FNSKey = this.fnskey.Text;
            // здесь надо проверить профиль и ключ
                if (Signature.CheckProfile(this.profile.Text))
                {
                  Config.Profile = this.profile.Text.Trim();
                } else
                {
                     MessageBox.Show("Неверный профиль СКАД Сигнатура:" + this.profile.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            // проверяеи ключ
            if (Signature.CheckKey(this.profile.Text.Trim(), this.fnskey.Text))
            {
                Config.FNSKey = this.fnskey.Text;
            }
            else
            {
                MessageBox.Show("Неверный ключ ФНС:" + this.fnskey.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Config.SerialNum == 0)
                Config.SerialNum = 1;
            //Config.SerialNum = Convert.ToInt32(this.lastNum.Text);
            //Config.SerialDate = Util.DateFromYMD(this.lastDate.Text);
            if (this.virtualFDD.Checked)
            {
                if (!File.Exists("imdisk.exe"))
                {
                    MessageBox.Show("Включено использвание виртуального флоппи-диска\nно отсутствует IMDISK.EXE\nИспользование отключено", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Config.UseVirtualFDD = false;
                }
                else
                {
                    Config.UseVirtualFDD = true;
                }
            }
            if (this.usePTK.Checked)
            {
                if (!File.Exists(this.ptkdb.Text))
                {
                    MessageBox.Show("Включено использвание ПТК ПСД\nно отсутствует файл "+ this.ptkdb.Text+"\n"+
                        "Использование ПДК ПСД отключено", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Config.UsePTK = false;
                }
                else
                {
                    Config.UsePTK = true;
                }
            }
            // проверка наличия файлов XSD в каталоге
            // недоделано!
            Config.UseXSD = this.checkXSD.Checked && Directory.Exists(this.xsddir.Text);
            Config.CreatePB1 = this.makePB1.Checked;
            Config.NoLicense = this.noLicense.Checked;
            Config.TelOtpr = this.tel.Text;
            Config.DolOtpr = this.dolgn.Text;
            Config.FamOtpr = this.family.Text;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Проверить существование каталога directory
        // Если каталог не существует, спросить о создании (Yes/No)
        //   если No, то выйти с false
        //   если Yes, создать и происвоить config_dir значение этого каталога
        // Если каталог существует, config_dir = directory
        private bool CheckDirectory(string directory)
        {
            if(!Directory.Exists(directory))
            {
                if (MessageBox.Show("Каталог " + directory + " не существует\nСоздать?", "Отсутствует каталог", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Directory.CreateDirectory(directory);
                    return true;
                }
                else return false;
            } else
            {
                return true;
            }
        }
    }
}
