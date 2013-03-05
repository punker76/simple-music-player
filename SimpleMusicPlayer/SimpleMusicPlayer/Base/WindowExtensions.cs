using System;
using System.Windows;
using System.Windows.Forms;

namespace SimpleMusicPlayer.Base
{
  public static class WindowExtensions
  {
    /// <summary>
    /// Fits the window into the current screen.
    /// </summary>
    public static void FitIntoScreen(this Window w) {
      if (w == null) {
        return;
      }
      var workingArea = GetDesktopWorkingArea(w);
      var formSize = w.RestoreBounds;
      Rect newFormSize;
      if (FitIntoScreen(workingArea, formSize, out newFormSize)) {
        w.Left = newFormSize.X;
        w.Top = newFormSize.Y;
        w.Width = newFormSize.Width;
        w.Height = newFormSize.Height;
      }
    }

    private static Rect GetDesktopWorkingArea(Window w) {
      var rectangle = Screen.GetWorkingArea(new System.Drawing.Point(Convert.ToInt32(w.Left + (w.Width / 2.0)), Convert.ToInt32(w.Top)));
      return new Rect(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    }

    private static bool FitIntoScreen(Rect workArea, Rect formSize, out Rect newFormSize) {
      var hasChanged = false;
      newFormSize = formSize == Rect.Empty ? new Rect() : formSize;
      if (!workArea.Contains(formSize)) {
        // limiting size guarantees form fits into screen
        newFormSize.Width = Math.Min(newFormSize.Width, workArea.Width);
        newFormSize.Height = Math.Min(newFormSize.Height, workArea.Height);
        if (newFormSize.Right > workArea.Right) {
          newFormSize.Offset(workArea.Right - newFormSize.Right, 0);
          hasChanged = true;
        } else if (newFormSize.Left < workArea.Left) {
          newFormSize.Offset(workArea.Left - newFormSize.Left, 0);
          hasChanged = true;
        }
        if (newFormSize.Top < workArea.Top) {
          newFormSize.Offset(0, workArea.Top - newFormSize.Top);
          hasChanged = true;
        } else if (newFormSize.Bottom > workArea.Bottom) {
          newFormSize.Offset(0, workArea.Bottom - newFormSize.Bottom);
          hasChanged = true;
        }
      }
      return hasChanged;
    }
  }
}