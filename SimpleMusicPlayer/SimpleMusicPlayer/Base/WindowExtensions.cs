using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace SimpleMusicPlayer.Base
{
  public static class WindowExtensions
  {
    /// <summary>
    /// Fits the window into the current screen.
    /// </summary>
    /// <param name="w">The w.</param>
    public static void FitIntoScreen(this Window w) {
      if (w == null) {
        return;
      }
      var workingArea = GetDesktopWorkingArea(); // Screen.GetWorkingArea(new System.Drawing.Point(Convert.ToInt32(w.Left), Convert.ToInt32(w.Top)));
      var formSize = w.RestoreBounds;
      Rect newFormSize;
      if (FitIntoScreen(workingArea, formSize, out newFormSize)) {
        w.Left = newFormSize.X;
        w.Top = newFormSize.Y;
        w.Width = newFormSize.Width;
        w.Height = newFormSize.Height;
      }
    }

    private static Rect GetDesktopWorkingArea() {
      var workingArea = Rect.Empty;
      foreach (var screen in Screen.AllScreens) {
        var screenWorkingArea = screen.WorkingArea;
        workingArea = Rect.Union(workingArea, new Rect(screenWorkingArea.Left, screenWorkingArea.Top, screenWorkingArea.Width, screenWorkingArea.Height));
      }
      return workingArea;
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