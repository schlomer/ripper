namespace RipAdmin
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.miAction = new System.Windows.Forms.ToolStripDropDownButton();
            this.miActionConnectRipper = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.miActionExit = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tvConnections = new System.Windows.Forms.TreeView();
            this.tvConnectionsImageList = new System.Windows.Forms.ImageList(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.recordsTextBox = new System.Windows.Forms.TextBox();
            this.cmRecords = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miRecordsGetRecords = new System.Windows.Forms.ToolStripMenuItem();
            this.mainToolStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.cmRecords.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.statusStrip1.Location = new System.Drawing.Point(0, 698);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1039, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAction});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mainToolStrip.Size = new System.Drawing.Size(1039, 25);
            this.mainToolStrip.TabIndex = 2;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // miAction
            // 
            this.miAction.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.miAction.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miActionConnectRipper,
            this.exitToolStripMenuItem1,
            this.miActionExit});
            this.miAction.Image = global::RipAdmin.Properties.Resources.Action_16x;
            this.miAction.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.miAction.Name = "miAction";
            this.miAction.Size = new System.Drawing.Size(55, 22);
            this.miAction.Text = "&Action";
            // 
            // miActionConnectRipper
            // 
            this.miActionConnectRipper.Image = global::RipAdmin.Properties.Resources.ConnectFilled_grey_16x;
            this.miActionConnectRipper.Name = "miActionConnectRipper";
            this.miActionConnectRipper.Size = new System.Drawing.Size(165, 22);
            this.miActionConnectRipper.Text = "&Connect Ripper...";
            this.miActionConnectRipper.Click += new System.EventHandler(this.miActionConnectRipper_Click);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(162, 6);
            // 
            // miActionExit
            // 
            this.miActionExit.Image = global::RipAdmin.Properties.Resources.Exit_16x;
            this.miActionExit.Name = "miActionExit";
            this.miActionExit.Size = new System.Drawing.Size(165, 22);
            this.miActionExit.Text = "E&xit";
            this.miActionExit.Click += new System.EventHandler(this.miActionExit_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tvConnections);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 673);
            this.panel1.TabIndex = 3;
            // 
            // tvConnections
            // 
            this.tvConnections.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tvConnections.ContextMenuStrip = this.cmRecords;
            this.tvConnections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvConnections.ImageIndex = 0;
            this.tvConnections.ImageList = this.tvConnectionsImageList;
            this.tvConnections.Location = new System.Drawing.Point(0, 19);
            this.tvConnections.Name = "tvConnections";
            this.tvConnections.SelectedImageIndex = 0;
            this.tvConnections.Size = new System.Drawing.Size(200, 654);
            this.tvConnections.TabIndex = 1;
            this.tvConnections.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvConnections_NodeMouseClick);
            // 
            // tvConnectionsImageList
            // 
            this.tvConnectionsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("tvConnectionsImageList.ImageStream")));
            this.tvConnectionsImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.tvConnectionsImageList.Images.SetKeyName(0, "DatabaseRun_16x.png");
            this.tvConnectionsImageList.Images.SetKeyName(1, "DatabaseGroup_16x.png");
            this.tvConnectionsImageList.Images.SetKeyName(2, "DatabaseTableGroup_16x.png");
            this.tvConnectionsImageList.Images.SetKeyName(3, "Datalist_16x.png");
            this.tvConnectionsImageList.Images.SetKeyName(4, "Partition_16x.png");
            this.tvConnectionsImageList.Images.SetKeyName(5, "Guid_16x.png");
            this.tvConnectionsImageList.Images.SetKeyName(6, "Indexer_16x.png");
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 19);
            this.panel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connections";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.Control;
            this.splitter1.Location = new System.Drawing.Point(200, 25);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(5, 673);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // recordsTextBox
            // 
            this.recordsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recordsTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recordsTextBox.Location = new System.Drawing.Point(205, 25);
            this.recordsTextBox.Multiline = true;
            this.recordsTextBox.Name = "recordsTextBox";
            this.recordsTextBox.ReadOnly = true;
            this.recordsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.recordsTextBox.Size = new System.Drawing.Size(834, 673);
            this.recordsTextBox.TabIndex = 5;
            this.recordsTextBox.WordWrap = false;
            // 
            // cmRecords
            // 
            this.cmRecords.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miRecordsGetRecords});
            this.cmRecords.Name = "cmRecords";
            this.cmRecords.ShowImageMargin = false;
            this.cmRecords.Size = new System.Drawing.Size(113, 26);
            this.cmRecords.Opening += new System.ComponentModel.CancelEventHandler(this.cmRecords_Opening);
            // 
            // miRecordsGetRecords
            // 
            this.miRecordsGetRecords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.miRecordsGetRecords.Name = "miRecordsGetRecords";
            this.miRecordsGetRecords.Size = new System.Drawing.Size(180, 22);
            this.miRecordsGetRecords.Text = "Get Records";
            this.miRecordsGetRecords.Click += new System.EventHandler(this.miRecordsGetRecords_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1039, 720);
            this.Controls.Add(this.recordsTextBox);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainForm";
            this.Text = "Ripper Admin";
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.cmRecords.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripDropDownButton miAction;
        private System.Windows.Forms.ToolStripMenuItem miActionConnectRipper;
        private System.Windows.Forms.ToolStripSeparator exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem miActionExit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView tvConnections;
        private System.Windows.Forms.ImageList tvConnectionsImageList;
        private System.Windows.Forms.TextBox recordsTextBox;
        private System.Windows.Forms.ContextMenuStrip cmRecords;
        private System.Windows.Forms.ToolStripMenuItem miRecordsGetRecords;
    }
}

