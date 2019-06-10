namespace EmguCvWinforms
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.OriginalImage = new Emgu.CV.UI.ImageBox();
            this.FormImage = new Emgu.CV.UI.ImageBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ContourImage = new Emgu.CV.UI.ImageBox();
            this.DetectionMethod = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.OriginalImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FormImage)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ContourImage)).BeginInit();
            this.SuspendLayout();
            // 
            // OriginalImage
            // 
            this.OriginalImage.Location = new System.Drawing.Point(4, 4);
            this.OriginalImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.OriginalImage.Name = "OriginalImage";
            this.OriginalImage.Size = new System.Drawing.Size(533, 369);
            this.OriginalImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.OriginalImage.TabIndex = 2;
            this.OriginalImage.TabStop = false;
            // 
            // FormImage
            // 
            this.FormImage.Location = new System.Drawing.Point(4, 381);
            this.FormImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FormImage.Name = "FormImage";
            this.FormImage.Size = new System.Drawing.Size(533, 369);
            this.FormImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.FormImage.TabIndex = 2;
            this.FormImage.TabStop = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.OriginalImage);
            this.flowLayoutPanel1.Controls.Add(this.FormImage);
            this.flowLayoutPanel1.Controls.Add(this.ContourImage);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(16, 49);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(863, 1201);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // ContourImage
            // 
            this.ContourImage.Location = new System.Drawing.Point(4, 758);
            this.ContourImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ContourImage.Name = "ContourImage";
            this.ContourImage.Size = new System.Drawing.Size(533, 369);
            this.ContourImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ContourImage.TabIndex = 2;
            this.ContourImage.TabStop = false;
            // 
            // DetectionMethod
            // 
            this.DetectionMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DetectionMethod.FormattingEnabled = true;
            this.DetectionMethod.Items.AddRange(new object[] {
            "Method 1",
            "Method 2"});
            this.DetectionMethod.Location = new System.Drawing.Point(17, 16);
            this.DetectionMethod.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DetectionMethod.Name = "DetectionMethod";
            this.DetectionMethod.Size = new System.Drawing.Size(160, 24);
            this.DetectionMethod.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 1265);
            this.Controls.Add(this.DetectionMethod);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.OriginalImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FormImage)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ContourImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Emgu.CV.UI.ImageBox OriginalImage;
        private Emgu.CV.UI.ImageBox FormImage;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Emgu.CV.UI.ImageBox ContourImage;
        private System.Windows.Forms.ComboBox DetectionMethod;
    }
}

