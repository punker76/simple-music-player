using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SimpleMusicPlayer.Core
{
    /// <summary>
    /// A Slider which provides a way to modify the 
    /// auto tooltip text by using a format string.
    /// </summary>
    public class FormattedSlider : Slider
    {
        private ToolTip _autoToolTip;

        /// <summary>
        /// Gets/sets a format string used to modify the auto tooltip's content.
        /// Note: This format string must contain exactly one placeholder value,
        /// which is used to hold the tooltip's original content.
        /// </summary>
        public string AutoToolTipFormat { get; set; }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            this.FormatAutoToolTipContent();
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            this.FormatAutoToolTipContent();
        }

        private void FormatAutoToolTipContent()
        {
            if (!string.IsNullOrEmpty(this.AutoToolTipFormat))
            {
                this.AutoToolTip.Content = string.Format(this.AutoToolTipFormat, this.Value);
            }
        }

        private ToolTip AutoToolTip
        {
            get
            {
                if (this._autoToolTip == null)
                {
                    var field = typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
                    this._autoToolTip = field.GetValue(this) as ToolTip;
                }
                return this._autoToolTip;
            }
        }
    }
}