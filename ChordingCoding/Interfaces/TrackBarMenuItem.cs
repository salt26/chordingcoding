/*
MIT License

Copyright (c) 2019 salt26

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
using System;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ChordingCoding.UI
{
    /// <summary>
    /// 슬라이더(TrackBar)가 포함될 수 있는 우클릭 메뉴(ContextMenuStrip) 컴포넌트입니다.
    /// </summary>
    [System.ComponentModel.DesignerCategory("code")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip |
                                   ToolStripItemDesignerAvailability.ContextMenuStrip)]
    public class TrackBarMenuItem : ToolStripControlHost
    {
        public TrackBar TrackBar { get { return (TrackBar)Control; } }
        public TrackBarMenuItem() : base(CreateControl()) { }

        private static TrackBar CreateControl()
        {
            var t = new TrackBar()
            {
                TickStyle = TickStyle.None, AutoSize = false, Height = 24, Maximum = 20, Minimum = 0, LargeChange = 2
            };
            return t;
        }

        public int Value
        {
            get { return TrackBar.Value; }
            set { TrackBar.Value = value; }
        }

        /// <summary>
        /// Attach to events we want to re-wrap
        /// </summary>
        /// <param name="control"></param>
        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            TrackBar trackBar = control as TrackBar;
            trackBar.ValueChanged += new EventHandler(trackBar_ValueChanged);
        }
        /// <summary>
        /// Detach from events.
        /// </summary>
        /// <param name="control"></param>
        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            TrackBar trackBar = control as TrackBar;
            trackBar.ValueChanged -= new EventHandler(trackBar_ValueChanged);
        }
        /// <summary>
        /// Routing for event
        /// TrackBar.ValueChanged -> TrackBarMenuItem.ValueChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void trackBar_ValueChanged(object sender, EventArgs e)
        {
            // when the trackbar value changes, fire an event.
            if (this.ValueChanged != null)
            {
                ValueChanged(sender, e);
            }
        }
        // add an event that is subscribable from the designer.
        public event EventHandler ValueChanged;
    }
}
