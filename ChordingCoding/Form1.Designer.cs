namespace ChordingCoding
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.테마ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.가을산책ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.비오는날ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.별헤는밤ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.불투명도ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackBarMenuItem1 = new ChordingCoding.TrackBarMenuItem();
            this.음량ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackBarMenuItem2 = new ChordingCoding.TrackBarMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.종료ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "트레이 아이콘을 마우스로 클릭하면 메뉴가 열립니다.";
            this.notifyIcon1.BalloonTipTitle = "ChordingCoding";
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ChordingCoding";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.테마ToolStripMenuItem,
            this.toolStripSeparator2,
            this.불투명도ToolStripMenuItem,
            this.음량ToolStripMenuItem,
            this.toolStripSeparator1,
            this.종료ToolStripMenuItem});
            this.contextMenuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(194, 140);
            // 
            // 테마ToolStripMenuItem
            // 
            this.테마ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.가을산책ToolStripMenuItem,
            this.비오는날ToolStripMenuItem,
            this.별헤는밤ToolStripMenuItem});
            this.테마ToolStripMenuItem.Name = "테마ToolStripMenuItem";
            this.테마ToolStripMenuItem.Size = new System.Drawing.Size(193, 24);
            this.테마ToolStripMenuItem.Text = "테마";
            // 
            // 가을산책ToolStripMenuItem
            // 
            this.가을산책ToolStripMenuItem.Name = "가을산책ToolStripMenuItem";
            this.가을산책ToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.가을산책ToolStripMenuItem.Text = "가을 산책";
            this.가을산책ToolStripMenuItem.Click += new System.EventHandler(this.가을산책ToolStripMenuItem_Click);
            // 
            // 비오는날ToolStripMenuItem
            // 
            this.비오는날ToolStripMenuItem.Name = "비오는날ToolStripMenuItem";
            this.비오는날ToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.비오는날ToolStripMenuItem.Text = "비 오는 날";
            this.비오는날ToolStripMenuItem.Click += new System.EventHandler(this.비오는날ToolStripMenuItem_Click);
            // 
            // 별헤는밤ToolStripMenuItem
            // 
            this.별헤는밤ToolStripMenuItem.Name = "별헤는밤ToolStripMenuItem";
            this.별헤는밤ToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.별헤는밤ToolStripMenuItem.Text = "별 헤는 밤";
            this.별헤는밤ToolStripMenuItem.Click += new System.EventHandler(this.별헤는밤ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // 불투명도ToolStripMenuItem
            // 
            this.불투명도ToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.불투명도ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackBarMenuItem1});
            this.불투명도ToolStripMenuItem.Name = "불투명도ToolStripMenuItem";
            this.불투명도ToolStripMenuItem.Size = new System.Drawing.Size(193, 24);
            this.불투명도ToolStripMenuItem.Text = "불투명도";
            // 
            // trackBarMenuItem1
            // 
            this.trackBarMenuItem1.BackColor = System.Drawing.Color.White;
            this.trackBarMenuItem1.Name = "trackBarMenuItem1";
            this.trackBarMenuItem1.Size = new System.Drawing.Size(104, 24);
            this.trackBarMenuItem1.Text = "trackBarMenuItem1";
            this.trackBarMenuItem1.Value = 0;
            this.trackBarMenuItem1.ValueChanged += new System.EventHandler(this.trackBarMenuItem1_ValueChanged);
            // 
            // 음량ToolStripMenuItem
            // 
            this.음량ToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.음량ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackBarMenuItem2});
            this.음량ToolStripMenuItem.Name = "음량ToolStripMenuItem";
            this.음량ToolStripMenuItem.Size = new System.Drawing.Size(193, 24);
            this.음량ToolStripMenuItem.Text = "음량";
            // 
            // trackBarMenuItem2
            // 
            this.trackBarMenuItem2.BackColor = System.Drawing.Color.White;
            this.trackBarMenuItem2.Name = "trackBarMenuItem2";
            this.trackBarMenuItem2.Size = new System.Drawing.Size(104, 24);
            this.trackBarMenuItem2.Text = "trackBarMenuItem2";
            this.trackBarMenuItem2.Value = 10;
            this.trackBarMenuItem2.ValueChanged += new System.EventHandler(this.trackBarMenuItem2_ValueChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(190, 6);
            // 
            // 종료ToolStripMenuItem
            // 
            this.종료ToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.종료ToolStripMenuItem.Name = "종료ToolStripMenuItem";
            this.종료ToolStripMenuItem.Size = new System.Drawing.Size(193, 24);
            this.종료ToolStripMenuItem.Text = "종료                 ";
            this.종료ToolStripMenuItem.Click += new System.EventHandler(this.종료ToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Opacity = 0.2D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "ChordingCoding";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.White;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 불투명도ToolStripMenuItem;
        private TrackBarMenuItem trackBarMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 음량ToolStripMenuItem;
        private TrackBarMenuItem trackBarMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 종료ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 테마ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 가을산책ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 비오는날ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 별헤는밤ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

