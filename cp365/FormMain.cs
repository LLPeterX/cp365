﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cp365
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            
           

        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuConfig_Click(object sender, EventArgs e)
        {
            // Конфигурирование приложения
            FormConfig fconfig = new FormConfig();
            fconfig.Show();
        }

        private void processAFN(object sender, EventArgs e)
        {
            // взять файл из каталога AFN
            // снять с него подпись
            // распаковать в TEMP
            // расширофровать TEMP\*.vrb
            // снять подпись с TEMP\*.xml
            // скопировать *.xml в TO_INV, IN\yyyyMMdd
            // на файлы PNO создать WORK\PB2


        }

        private void processPTK(object sender, EventArgs e)
        {

        }
    }
}
