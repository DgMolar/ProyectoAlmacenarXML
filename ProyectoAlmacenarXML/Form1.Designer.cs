namespace GuardadordeXML
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button buttonSelectFile;
        private System.Windows.Forms.Button buttonProcessFile;
        private System.Windows.Forms.TextBox textBoxFilePath;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.buttonSelectFile = new System.Windows.Forms.Button();
            this.buttonProcessFile = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();

            // 
            // buttonSelectFile
            // 
            this.buttonSelectFile.Location = new System.Drawing.Point(50, 50);
            this.buttonSelectFile.Name = "buttonSelectFile";
            this.buttonSelectFile.Size = new System.Drawing.Size(100, 23);
            this.buttonSelectFile.TabIndex = 0;
            this.buttonSelectFile.Text = "Seleccionar XML";
            this.buttonSelectFile.UseVisualStyleBackColor = true;
            this.buttonSelectFile.Click += new System.EventHandler(this.buttonSelectFile_Click);

            // 
            // buttonProcessFile
            // 
            this.buttonProcessFile.Location = new System.Drawing.Point(200, 50);
            this.buttonProcessFile.Name = "buttonProcessFile";
            this.buttonProcessFile.Size = new System.Drawing.Size(100, 23);
            this.buttonProcessFile.TabIndex = 1;
            this.buttonProcessFile.Text = "Guardar XML";
            this.buttonProcessFile.UseVisualStyleBackColor = true;
            this.buttonProcessFile.Click += new System.EventHandler(this.buttonProcessFile_Click);

            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(50, 100);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(250, 20);
            this.textBoxFilePath.TabIndex = 2;

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 200);
            this.Controls.Add(this.buttonSelectFile);
            this.Controls.Add(this.buttonProcessFile);
            this.Controls.Add(this.textBoxFilePath);
            this.Name = "Form1";
            this.Text = "Guardar XML";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
