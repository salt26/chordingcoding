/*
MIT License

Copyright (c) 2019 Dantae An

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
namespace ChordingCoding.UI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.themeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noteResolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fourthNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eighthNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sixteenthNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thirtysecondNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.immediateNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.musicalModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.majorModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minorModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sentimentAwarenessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.opacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.volumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAccompanimentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useReverbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.recordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackBarMenuItem2 = new ChordingCoding.UI.TrackBarMenuItem();
            this.trackBarMenuItem1 = new ChordingCoding.UI.TrackBarMenuItem();
            this.trackBarMenuItem3 = new ChordingCoding.UI.TrackBarMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.themeToolStripMenuItem,
            this.noteResolutionToolStripMenuItem,
            this.musicalModeToolStripMenuItem,
            this.sentimentAwarenessToolStripMenuItem,
            this.toolStripSeparator2,
            this.opacityToolStripMenuItem,
            this.volumeToolStripMenuItem,
            this.autoAccompanimentToolStripMenuItem,
            this.useReverbToolStripMenuItem,
            this.toolStripSeparator1,
            this.recordToolStripMenuItem,
            this.resetToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.contextMenuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(191, 280);
            // 
            // themeToolStripMenuItem
            // 
            this.themeToolStripMenuItem.Name = "themeToolStripMenuItem";
            this.themeToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.themeToolStripMenuItem.Text = "테마";
            // 
            // noteResolutionToolStripMenuItem
            // 
            this.noteResolutionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fourthNoteToolStripMenuItem,
            this.eighthNoteToolStripMenuItem,
            this.sixteenthNoteToolStripMenuItem,
            this.thirtysecondNoteToolStripMenuItem,
            this.immediateNoteToolStripMenuItem});
            this.noteResolutionToolStripMenuItem.Name = "noteResolutionToolStripMenuItem";
            this.noteResolutionToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.noteResolutionToolStripMenuItem.Text = "단위 리듬";
            // 
            // fourthNoteToolStripMenuItem
            // 
            this.fourthNoteToolStripMenuItem.Name = "fourthNoteToolStripMenuItem";
            this.fourthNoteToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.fourthNoteToolStripMenuItem.Text = "4분음표";
            this.fourthNoteToolStripMenuItem.Click += new System.EventHandler(this.fourthNoteToolStripMenuItem_Click);
            // 
            // eighthNoteToolStripMenuItem
            // 
            this.eighthNoteToolStripMenuItem.Name = "eighthNoteToolStripMenuItem";
            this.eighthNoteToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.eighthNoteToolStripMenuItem.Text = "8분음표";
            this.eighthNoteToolStripMenuItem.Click += new System.EventHandler(this.eighthNoteToolStripMenuItem_Click);
            // 
            // sixteenthNoteToolStripMenuItem
            // 
            this.sixteenthNoteToolStripMenuItem.Name = "sixteenthNoteToolStripMenuItem";
            this.sixteenthNoteToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.sixteenthNoteToolStripMenuItem.Text = "16분음표";
            this.sixteenthNoteToolStripMenuItem.Click += new System.EventHandler(this.sixteenthNoteToolStripMenuItem_Click);
            // 
            // thirtysecondNoteToolStripMenuItem
            // 
            this.thirtysecondNoteToolStripMenuItem.Name = "thirtysecondNoteToolStripMenuItem";
            this.thirtysecondNoteToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.thirtysecondNoteToolStripMenuItem.Text = "32분음표";
            this.thirtysecondNoteToolStripMenuItem.Click += new System.EventHandler(this.thirtysecondNotetoolStripMenuItem_Click);
            // 
            // immediateNoteToolStripMenuItem
            // 
            this.immediateNoteToolStripMenuItem.Name = "immediateNoteToolStripMenuItem";
            this.immediateNoteToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.immediateNoteToolStripMenuItem.Text = "없음 (즉시 반응)";
            this.immediateNoteToolStripMenuItem.Click += new System.EventHandler(this.immediateNoteToolStripMenuItem_Click);
            // 
            // musicalModeToolStripMenuItem
            // 
            this.musicalModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoModeToolStripMenuItem,
            this.majorModeToolStripMenuItem,
            this.minorModeToolStripMenuItem});
            this.musicalModeToolStripMenuItem.Name = "musicalModeToolStripMenuItem";
            this.musicalModeToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.musicalModeToolStripMenuItem.Text = "선법";
            // 
            // autoModeToolStripMenuItem
            // 
            this.autoModeToolStripMenuItem.Name = "autoModeToolStripMenuItem";
            this.autoModeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.autoModeToolStripMenuItem.Text = "자동";
            this.autoModeToolStripMenuItem.Click += new System.EventHandler(this.autoModeToolStripMenuItem_Click);
            // 
            // majorModeToolStripMenuItem
            // 
            this.majorModeToolStripMenuItem.Name = "majorModeToolStripMenuItem";
            this.majorModeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.majorModeToolStripMenuItem.Text = "밝은 음악 선호";
            this.majorModeToolStripMenuItem.Click += new System.EventHandler(this.majorModeToolStripMenuItem_Click);
            // 
            // minorModeToolStripMenuItem
            // 
            this.minorModeToolStripMenuItem.Name = "minorModeToolStripMenuItem";
            this.minorModeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.minorModeToolStripMenuItem.Text = "어두운 음악 선호";
            this.minorModeToolStripMenuItem.Click += new System.EventHandler(this.minorModeToolStripMenuItem_Click);
            // 
            // sentimentAwarenessToolStripMenuItem
            // 
            this.sentimentAwarenessToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sentimentAwarenessToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackBarMenuItem2});
            this.sentimentAwarenessToolStripMenuItem.Name = "sentimentAwarenessToolStripMenuItem";
            this.sentimentAwarenessToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.sentimentAwarenessToolStripMenuItem.Text = "감성 인식 수준";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(187, 6);
            // 
            // opacityToolStripMenuItem
            // 
            this.opacityToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.opacityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackBarMenuItem1});
            this.opacityToolStripMenuItem.Name = "opacityToolStripMenuItem";
            this.opacityToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.opacityToolStripMenuItem.Text = "불투명도";
            // 
            // volumeToolStripMenuItem
            // 
            this.volumeToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.volumeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackBarMenuItem3});
            this.volumeToolStripMenuItem.Name = "volumeToolStripMenuItem";
            this.volumeToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.volumeToolStripMenuItem.Text = "음량";
            // 
            // autoAccompanimentToolStripMenuItem
            // 
            this.autoAccompanimentToolStripMenuItem.Name = "autoAccompanimentToolStripMenuItem";
            this.autoAccompanimentToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.autoAccompanimentToolStripMenuItem.Text = "자동 반주";
            this.autoAccompanimentToolStripMenuItem.Click += new System.EventHandler(this.autoAccompanimentToolStripMenuItem_Click);
            // 
            // useReverbToolStripMenuItem
            // 
            this.useReverbToolStripMenuItem.Name = "useReverbToolStripMenuItem";
            this.useReverbToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.useReverbToolStripMenuItem.Text = "반향";
            this.useReverbToolStripMenuItem.Click += new System.EventHandler(this.useReverbToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(187, 6);
            // 
            // recordToolStripMenuItem
            // 
            this.recordToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.recordToolStripMenuItem.Name = "recordToolStripMenuItem";
            this.recordToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.recordToolStripMenuItem.Text = "녹음 [F12]";
            this.recordToolStripMenuItem.Click += new System.EventHandler(this.recordToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.quitToolStripMenuItem.Text = "종료                       ";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // trackBarMenuItem2
            // 
            this.trackBarMenuItem2.BackColor = System.Drawing.Color.White;
            this.trackBarMenuItem2.Name = "trackBarMenuItem2";
            this.trackBarMenuItem2.Size = new System.Drawing.Size(104, 24);
            this.trackBarMenuItem2.Text = "trackBarMenuItem2";
            this.trackBarMenuItem2.Value = 6;
            this.trackBarMenuItem2.ValueChanged += new System.EventHandler(this.trackBarMenuItem2_ValueChanged);
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
            // trackBarMenuItem3
            // 
            this.trackBarMenuItem3.BackColor = System.Drawing.Color.White;
            this.trackBarMenuItem3.Name = "trackBarMenuItem3";
            this.trackBarMenuItem3.Size = new System.Drawing.Size(104, 24);
            this.trackBarMenuItem3.Text = "trackBarMenuItem3";
            this.trackBarMenuItem3.Value = 10;
            this.trackBarMenuItem3.ValueChanged += new System.EventHandler(this.trackBarMenuItem3_ValueChanged);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
            this.resetToolStripMenuItem.Text = "모든 설정 초기화";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(247, 202);
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
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
        private System.Windows.Forms.ToolStripMenuItem opacityToolStripMenuItem;
        private ChordingCoding.UI.TrackBarMenuItem trackBarMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem sentimentAwarenessToolStripMenuItem;
        private ChordingCoding.UI.TrackBarMenuItem trackBarMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem volumeToolStripMenuItem;
        private ChordingCoding.UI.TrackBarMenuItem trackBarMenuItem3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem themeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem noteResolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fourthNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eighthNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sixteenthNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thirtysecondNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem immediateNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAccompanimentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useReverbToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem musicalModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem majorModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minorModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
    }
}

