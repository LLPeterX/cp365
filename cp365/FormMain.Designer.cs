namespace cp365
{
    partial class FormMain
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.входящиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProcessAFN = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProcessPTK = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExitIn = new System.Windows.Forms.ToolStripMenuItem();
            this.исходящиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.входящиеToolStripMenuItem,
            this.исходящиеToolStripMenuItem,
            this.menuConfig,
            this.menuExit});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(800, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // входящиеToolStripMenuItem
            // 
            this.входящиеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuProcessAFN,
            this.menuProcessPTK,
            this.menuExitIn});
            this.входящиеToolStripMenuItem.Name = "входящиеToolStripMenuItem";
            this.входящиеToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.входящиеToolStripMenuItem.Text = "Входящие";
            // 
            // menuProcessAFN
            // 
            this.menuProcessAFN.Name = "menuProcessAFN";
            this.menuProcessAFN.Size = new System.Drawing.Size(194, 22);
            this.menuProcessAFN.Text = "Обработка файла AFN";
            this.menuProcessAFN.Click += new System.EventHandler(this.processAFN);
            // 
            // menuProcessPTK
            // 
            this.menuProcessPTK.Name = "menuProcessPTK";
            this.menuProcessPTK.Size = new System.Drawing.Size(194, 22);
            this.menuProcessPTK.Text = "Взять файл из ПТК ПСД";
            this.menuProcessPTK.Click += new System.EventHandler(this.processPTK);
            // 
            // menuExitIn
            // 
            this.menuExitIn.Name = "menuExitIn";
            this.menuExitIn.Size = new System.Drawing.Size(194, 22);
            this.menuExitIn.Text = "Выход";
            this.menuExitIn.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // исходящиеToolStripMenuItem
            // 
            this.исходящиеToolStripMenuItem.Name = "исходящиеToolStripMenuItem";
            this.исходящиеToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.исходящиеToolStripMenuItem.Text = "Исходящие";
            // 
            // menuConfig
            // 
            this.menuConfig.Name = "menuConfig";
            this.menuConfig.Size = new System.Drawing.Size(73, 20);
            this.menuConfig.Text = "Настройка";
            this.menuConfig.Click += new System.EventHandler(this.menuConfig_Click);
            // 
            // menuExit
            // 
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(52, 20);
            this.menuExit.Text = "Выход";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mainMenu);
            this.MainMenuStrip = this.mainMenu;
            this.Name = "FormMain";
            this.Text = "Обработка сообщений 365-П (440-П)";
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem входящиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem исходящиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuConfig;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ToolStripMenuItem menuProcessAFN;
        private System.Windows.Forms.ToolStripMenuItem menuProcessPTK;
        private System.Windows.Forms.ToolStripMenuItem menuExitIn;
    }
}

