using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ChordingCoding
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
                TickStyle = TickStyle.None, AutoSize = false, Height = 24, Maximum = 100, Minimum = 0
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
