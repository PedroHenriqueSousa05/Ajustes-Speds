namespace ProjetoSpeds
{
    partial class SpedFiscal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BtnSelSped = new System.Windows.Forms.Button();
            this.CbAjustec100 = new System.Windows.Forms.CheckBox();
            this.CbAjustec190 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.CbRelatorio = new System.Windows.Forms.CheckBox();
            this.CbGerarReg = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CbAjuste0205 = new System.Windows.Forms.CheckBox();
            this.CbAjusteIcmsVl = new System.Windows.Forms.CheckBox();
            this.CbAjusteIE = new System.Windows.Forms.CheckBox();
            this.CbGerarFiscalReg = new System.Windows.Forms.CheckBox();
            this.CbExcluirListadosFiscal = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // BtnSelSped
            // 
            this.BtnSelSped.Location = new System.Drawing.Point(517, 210);
            this.BtnSelSped.Name = "BtnSelSped";
            this.BtnSelSped.Size = new System.Drawing.Size(106, 35);
            this.BtnSelSped.TabIndex = 0;
            this.BtnSelSped.Text = "Ajustar";
            this.BtnSelSped.UseVisualStyleBackColor = true;
            this.BtnSelSped.Click += new System.EventHandler(this.BtnSelSped_Click);
            // 
            // CbAjustec100
            // 
            this.CbAjustec100.AutoSize = true;
            this.CbAjustec100.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbAjustec100.Location = new System.Drawing.Point(24, 126);
            this.CbAjustec100.Name = "CbAjustec100";
            this.CbAjustec100.Size = new System.Drawing.Size(119, 24);
            this.CbAjustec100.TabIndex = 12;
            this.CbAjustec100.Text = "Registro C100";
            this.CbAjustec100.UseVisualStyleBackColor = true;
            // 
            // CbAjustec190
            // 
            this.CbAjustec190.AutoSize = true;
            this.CbAjustec190.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbAjustec190.Location = new System.Drawing.Point(24, 96);
            this.CbAjustec190.Name = "CbAjustec190";
            this.CbAjustec190.Size = new System.Drawing.Size(119, 24);
            this.CbAjustec190.TabIndex = 11;
            this.CbAjustec190.Text = "Registro C190";
            this.CbAjustec190.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(320, 25);
            this.label1.TabIndex = 10;
            this.label1.Text = "Selecione quais ajustes deseja fazer:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(46, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 20);
            this.label2.TabIndex = 13;
            this.label2.Text = "Bloco C";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(387, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 20);
            this.label3.TabIndex = 14;
            this.label3.Text = "Outros";
            // 
            // CbRelatorio
            // 
            this.CbRelatorio.AutoSize = true;
            this.CbRelatorio.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbRelatorio.Location = new System.Drawing.Point(359, 96);
            this.CbRelatorio.Name = "CbRelatorio";
            this.CbRelatorio.Size = new System.Drawing.Size(132, 24);
            this.CbRelatorio.TabIndex = 15;
            this.CbRelatorio.Text = "Gerar Relatório";
            this.CbRelatorio.UseVisualStyleBackColor = true;
            // 
            // CbGerarReg
            // 
            this.CbGerarReg.AutoSize = true;
            this.CbGerarReg.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbGerarReg.Location = new System.Drawing.Point(359, 126);
            this.CbGerarReg.Name = "CbGerarReg";
            this.CbGerarReg.Size = new System.Drawing.Size(191, 24);
            this.CbGerarReg.TabIndex = 16;
            this.CbGerarReg.Text = "Gerar registros faltantes";
            this.CbGerarReg.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(222, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 20);
            this.label4.TabIndex = 17;
            this.label4.Text = "Bloco 0";
            // 
            // CbAjuste0205
            // 
            this.CbAjuste0205.AutoSize = true;
            this.CbAjuste0205.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbAjuste0205.Location = new System.Drawing.Point(192, 96);
            this.CbAjuste0205.Name = "CbAjuste0205";
            this.CbAjuste0205.Size = new System.Drawing.Size(120, 24);
            this.CbAjuste0205.TabIndex = 18;
            this.CbAjuste0205.Text = "Registro 0205";
            this.CbAjuste0205.UseVisualStyleBackColor = true;
            // 
            // CbAjusteIcmsVl
            // 
            this.CbAjusteIcmsVl.AutoSize = true;
            this.CbAjusteIcmsVl.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbAjusteIcmsVl.Location = new System.Drawing.Point(24, 156);
            this.CbAjusteIcmsVl.Name = "CbAjusteIcmsVl";
            this.CbAjusteIcmsVl.Size = new System.Drawing.Size(149, 24);
            this.CbAjusteIcmsVl.TabIndex = 19;
            this.CbAjusteIcmsVl.Text = "Ajuste Valor ICMS";
            this.CbAjusteIcmsVl.UseVisualStyleBackColor = true;
            // 
            // CbAjusteIE
            // 
            this.CbAjusteIE.AutoSize = true;
            this.CbAjusteIE.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbAjusteIE.Location = new System.Drawing.Point(192, 126);
            this.CbAjusteIE.Name = "CbAjusteIE";
            this.CbAjusteIE.Size = new System.Drawing.Size(86, 24);
            this.CbAjusteIE.TabIndex = 20;
            this.CbAjusteIE.Text = "Ajuste IE";
            this.CbAjusteIE.UseVisualStyleBackColor = true;
            // 
            // CbGerarFiscalReg
            // 
            this.CbGerarFiscalReg.AutoSize = true;
            this.CbGerarFiscalReg.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbGerarFiscalReg.Location = new System.Drawing.Point(359, 156);
            this.CbGerarFiscalReg.Name = "CbGerarFiscalReg";
            this.CbGerarFiscalReg.Size = new System.Drawing.Size(248, 24);
            this.CbGerarFiscalReg.TabIndex = 21;
            this.CbGerarFiscalReg.Text = "Gerar registros faltantes p/ fiscal";
            this.CbGerarFiscalReg.UseVisualStyleBackColor = true;
            // 
            // CbExcluirListadosFiscal
            // 
            this.CbExcluirListadosFiscal.AutoSize = true;
            this.CbExcluirListadosFiscal.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.CbExcluirListadosFiscal.Location = new System.Drawing.Point(359, 186);
            this.CbExcluirListadosFiscal.Name = "CbExcluirListadosFiscal";
            this.CbExcluirListadosFiscal.Size = new System.Drawing.Size(132, 24);
            this.CbExcluirListadosFiscal.TabIndex = 22;
            this.CbExcluirListadosFiscal.Text = "Excluir Listados";
            this.CbExcluirListadosFiscal.UseVisualStyleBackColor = true;
            // 
            // SpedFiscal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(635, 257);
            this.Controls.Add(this.CbExcluirListadosFiscal);
            this.Controls.Add(this.CbGerarFiscalReg);
            this.Controls.Add(this.CbAjusteIE);
            this.Controls.Add(this.CbAjusteIcmsVl);
            this.Controls.Add(this.CbAjuste0205);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.CbGerarReg);
            this.Controls.Add(this.CbRelatorio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CbAjustec100);
            this.Controls.Add(this.CbAjustec190);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnSelSped);
            this.Name = "SpedFiscal";
            this.Text = "Ajustes Sped Fiscal";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button BtnSelSped;
        private CheckBox CbAjustec100;
        private CheckBox CbAjustec190;
        private Label label1;
        private Label label2;
        private Label label3;
        private CheckBox CbRelatorio;
        private CheckBox CbGerarReg;
        private Label label4;
        private CheckBox CbAjuste0205;
        private CheckBox CbAjusteIcmsVl;
        private CheckBox CbAjusteIE;
        private CheckBox CbGerarFiscalReg;
        private CheckBox CbExcluirListadosFiscal;
    }
}