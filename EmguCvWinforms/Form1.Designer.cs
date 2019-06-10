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
            this.ContourImage = new Emgu.CV.UI.ImageBox();
            this.DetectionMethod = new System.Windows.Forms.ComboBox();
            this.FormImage = new Emgu.CV.UI.ImageBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Resolution = new System.Windows.Forms.ComboBox();
            this.Cameras = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.ContourImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FormImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContourImage
            // 
            this.ContourImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContourImage.Location = new System.Drawing.Point(0, 0);
            this.ContourImage.Name = "ContourImage";
            this.ContourImage.Size = new System.Drawing.Size(538, 976);
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
            this.DetectionMethod.Location = new System.Drawing.Point(13, 13);
            this.DetectionMethod.Name = "DetectionMethod";
            this.DetectionMethod.Size = new System.Drawing.Size(121, 21);
            this.DetectionMethod.TabIndex = 4;
            // 
            // FormImage
            // 
            this.FormImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormImage.Location = new System.Drawing.Point(0, 0);
            this.FormImage.Name = "FormImage";
            this.FormImage.Size = new System.Drawing.Size(1074, 976);
            this.FormImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.FormImage.TabIndex = 2;
            this.FormImage.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.splitContainer1.Location = new System.Drawing.Point(15, 40);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.ContourImage);
            this.splitContainer1.Panel1MinSize = 300;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel2.Controls.Add(this.FormImage);
            this.splitContainer1.Panel2MinSize = 300;
            this.splitContainer1.Size = new System.Drawing.Size(1616, 976);
            this.splitContainer1.SplitterDistance = 538;
            this.splitContainer1.TabIndex = 5;
            // 
            // Resolution
            // 
            this.Resolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Resolution.FormattingEnabled = true;
            this.Resolution.Items.AddRange(new object[] {
            "640 x 480",
            "1280 x 720",
            "1920 x 1080"});
            this.Resolution.Location = new System.Drawing.Point(141, 13);
            this.Resolution.Name = "Resolution";
            this.Resolution.Size = new System.Drawing.Size(121, 21);
            this.Resolution.TabIndex = 6;
            // 
            // Cameras
            // 
            this.Cameras.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Cameras.FormattingEnabled = true;
            this.Cameras.Location = new System.Drawing.Point(269, 13);
            this.Cameras.Name = "Cameras";
            this.Cameras.Size = new System.Drawing.Size(201, 21);
            this.Cameras.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1643, 1028);
            this.Controls.Add(this.Cameras);
            this.Controls.Add(this.Resolution);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.DetectionMethod);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.ContourImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FormImage)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Emgu.CV.UI.ImageBox ContourImage;
        private System.Windows.Forms.ComboBox DetectionMethod;
        private Emgu.CV.UI.ImageBox FormImage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ComboBox Resolution;
        private System.Windows.Forms.ComboBox Cameras;
    }
}

