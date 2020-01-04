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
            //this.ptkdb.Text = Config.PTKiniFile;
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
            this.eloDir.Text = Config.ELODir;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string warnings = "";
            if (CheckDirectory(this.workdir.Text.Trim()))
                Config.WorkDir = StripEndSlash(this.workdir.Text);
            else return;
            if (CheckDirectory(this.tempdir.Text.Trim()))
                Config.TempDir = StripEndSlash(this.tempdir.Text);
            else return;
            if (CheckDirectory(this.indir.Text.Trim()))
                Config.InDir = StripEndSlash(this.indir.Text);
            else return;
            if (CheckDirectory(this.outdir.Text.Trim()))
                Config.OutDir = this.outdir.Text.Trim();
            else return;
            if (CheckDirectory(this.afndir.Text.Trim()))
                Config.AFNDir = StripEndSlash(this.afndir.Text);
            else return;
            Config.INVDir = this.invdir.Text; // может быть пустой - тогда не копировать
            Config.XSDDir = this.xsddir.Text; // может быть пустой - тогда не проверять
            if (File.Exists(this.ptkdb.Text))
                Config.PTKDatabase = this.ptkdb.Text; // может быть пустой - тогда без ПТК ПСД
            else
            {
                warnings += "Файла " + this.ptkdb.Text + "не существует\n";
                Config.PTKDatabase = "";
                Config.UsePTK = false;
            }
            if (this.bik.Text.Length != 9 || !this.bik.Text.StartsWith("04"))
            {
                MessageBox.Show("Неверный БИК", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Config.BIK = this.bik.Text;
            if (this.fil.Text.Length != 4)
            {
                //MessageBox.Show("N филиала должен быть 4 цифры");
                warnings += "N филиала должен быть 4 цифры. Установлено 0000";
                Config.Filial = "0000";
            }
            else
            {
                Config.Filial = this.fil.Text;
            }
            // здесь надо проверить профиль и ключ
           if (Signature.CheckProfile(this.profile.Text))
                {
                  Config.Profile = this.profile.Text.Trim();
                } else
                {
                     MessageBox.Show("Неверный профиль СКАД \"Сигнатура\":" + this.profile.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }
            // проверяеи ключ
            if (Signature.CheckKey(this.profile.Text.Trim(), this.fnskey.Text))
            {
                Config.FNSKey = this.fnskey.Text;
            }
            else
            {
                MessageBox.Show("Ключ ФНС " + this.fnskey.Text+"\nне найден в хранилище сертификатов", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Config.SerialNum == 0)
                Config.SerialNum = 1;
            if (this.virtualFDD.Checked)
            {
                if (!File.Exists("imdisk.exe"))
                {
                    //MessageBox.Show("Включено использвание виртуального флоппи-диска\nно отсутствует IMDISK.EXE\nИспользование отключено", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    warnings+= "\nВключено использвание виртуального флоппи-диска\nно отсутствует IMDISK.EXE\nИспользование отключено\n";
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
                    warnings += "\nНе найден файл " + this.ptkdb.Text + "\nИспользование ПТК ПСД отключено\n";
                    //MessageBox.Show("Включено использвание ПТК ПСД\nно отсутствует файл "+ this.ptkini.Text+"\n"+
                    //    "Использование ПДК ПСД отключено", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Config.UsePTK = false;
                }
                else
                {
                    Config.UsePTK = true;
                }
            } else
            {
                Config.UsePTK = false;
            }
            // проверка наличия файлов XSD в каталоге
            // недоделано!
            Config.UseXSD = this.checkXSD.Checked && Directory.Exists(this.xsddir.Text); // пока не используется
            Config.CreatePB1 = this.makePB1.Checked;
            Config.NoLicense = this.noLicense.Checked; // на файлы PNO, RPO автоматически выдаем PB2
            if (this.tel.Text.Length < 2 || this.dolgn.Text.Length < 2 || this.family.Text.Length<2)
            {
                MessageBox.Show("Не заполнены все поля представителя банка\n(Фамилия, должность, телефон)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Config.TelOtpr = this.tel.Text;
            Config.DolOtpr = this.dolgn.Text;
            Config.FamOtpr = this.family.Text;
            if (!Directory.Exists(this.eloDir.Text))
            {
                MessageBox.Show("Неверный каталог посылок ПТК ПСД");
                return;
            }
            Config.ELODir = this.eloDir.Text.Trim();
            if (warnings.Length > 2)
                MessageBox.Show(warnings, "Ошибки кофигурации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Проверить существование каталога directory
        // Если каталог не существует, спросить о создании (Yes/No)
        //   если No, то выйти с false
        //   если Yes, создать и присвоить config_dir значение этого каталога
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

        private string StripEndSlash(string s)
        {
            string result = s.Trim();
            if (result.EndsWith("\\"))
                return result.Substring(0, result.Length - 1);
            return result;
        }
    }
}
