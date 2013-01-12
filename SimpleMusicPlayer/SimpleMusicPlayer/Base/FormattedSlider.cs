using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SimpleMusicPlayer.Base
{
  /// <summary>
  /// A Slider which provides a way to modify the 
  /// auto tooltip text by using a format string.
  /// </summary>
  public class FormattedSlider : Slider
  {
    private ToolTip _autoToolTip;

    public FormattedSlider() {
      this.Loaded += (sender, args) => this.UpdateToolTip();
    }

    private void UpdateToolTip() {
      if (this.Thumb == null) {
        return;
      }
      if (this.Thumb.ToolTip == null) {
        this.Thumb.ToolTip = new ToolTip();
      }
      ((ToolTip)this.Thumb.ToolTip).Content = this.ThumbToolTipText;
      ToolTipService.SetPlacement((ToolTip)this.Thumb.ToolTip, this.ThumbToolTipPlacement);
    }

    /// <summary>
    /// Gets/sets a format string used to modify the auto tooltip's content.
    /// Note: This format string must contain exactly one placeholder value,
    /// which is used to hold the tooltip's original content.
    /// </summary>
    public string AutoToolTipFormat { get; set; }

    protected override void OnThumbDragStarted(DragStartedEventArgs e) {
      base.OnThumbDragStarted(e);
      this.FormatAutoToolTipContent();
    }

    protected override void OnThumbDragDelta(DragDeltaEventArgs e) {
      base.OnThumbDragDelta(e);
      this.FormatAutoToolTipContent();
    }

    private void FormatAutoToolTipContent() {
      if (!string.IsNullOrEmpty(this.AutoToolTipFormat)) {
        this.AutoToolTip.Content = string.Format(this.AutoToolTipFormat, this.AutoToolTip.Content);
      }
    }

    private ToolTip AutoToolTip {
      get {
        if (this._autoToolTip == null) {
          var field = typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
          this._autoToolTip = field.GetValue(this) as ToolTip;
        }
        return this._autoToolTip;
      }
    }

    private Thumb thumb;

    public Thumb Thumb {
      get { return this.thumb ?? (this.thumb = this.GetThumb(this) as Thumb); }
    }

    private DependencyObject GetThumb(DependencyObject root) {
      if (root is Thumb) {
        return root;
      }
      DependencyObject depObject = null;
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) {
        depObject = this.GetThumb(VisualTreeHelper.GetChild(root, i));

        if (depObject is Thumb) {
          return depObject;
        }
      }
      return depObject;
    }

    public PlacementMode ThumbToolTipPlacement {
      get { return (PlacementMode)this.GetValue(ThumbToolTipPlacementProperty); }
      set { this.SetValue(ThumbToolTipPlacementProperty, value); }
    }

    public static readonly DependencyProperty ThumbToolTipPlacementProperty =
      DependencyProperty.Register("ThumbToolTipPlacement", typeof(PlacementMode), typeof(FormattedSlider), new UIPropertyMetadata(PlacementMode.Top, ThumbToolTipPlacementChanged));

    public string ThumbToolTipText {
      get { return (string)this.GetValue(ThumbToolTipTextProperty); }
      set { this.SetValue(ThumbToolTipTextProperty, value); }
    }

    public static readonly DependencyProperty ThumbToolTipTextProperty =
      DependencyProperty.Register("ThumbToolTipText", typeof(string), typeof(FormattedSlider), new UIPropertyMetadata(null, ThumbToolTipPlacementChanged));

    private static void ThumbToolTipPlacementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
      var slider = sender as FormattedSlider;
      if (slider != null && slider.Thumb != null) {
        slider.UpdateToolTip();
      }
    }
  }
}