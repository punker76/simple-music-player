using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Splat;

namespace SimpleMusicPlayer.Core
{
    public static class WindowExtensions
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class NativeMethods
        {
            /// <summary>
            /// Win32 API Imports
            /// </summary>
            [DllImport("user32.dll")]
            private static extern bool IsIconic(IntPtr hWnd);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

            [DllImport("user32.dll")]
            private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

            [DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands cmdShow);

            [DllImport("user32.dll")]
            internal static extern bool ShowWindowAsync(IntPtr hWnd, ShowWindowCommands cmdShow);

            [DllImport("user32.dll")]
            internal static extern int SetForegroundWindow(IntPtr hWnd);

            internal enum ShowWindowCommands : int
            {
                /// <summary>
                /// Hides the window and activates another window.
                /// </summary>
                Hide = 0,
                /// <summary>
                /// Activates and displays a window. If the window is minimized or 
                /// maximized, the system restores it to its original size and position.
                /// An application should specify this flag when displaying the window 
                /// for the first time.
                /// </summary>
                Normal = 1,
                /// <summary>
                /// Activates the window and displays it as a minimized window.
                /// </summary>
                ShowMinimized = 2,
                /// <summary>
                /// Maximizes the specified window.
                /// </summary>
                Maximize = 3, // is this the right value?
                /// <summary>
                /// Activates the window and displays it as a maximized window.
                /// </summary>       
                ShowMaximized = 3,
                /// <summary>
                /// Displays a window in its most recent size and position. This value 
                /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
                /// the window is not activated.
                /// </summary>
                ShowNoActivate = 4,
                /// <summary>
                /// Activates the window and displays it in its current size and position. 
                /// </summary>
                Show = 5,
                /// <summary>
                /// Minimizes the specified window and activates the next top-level 
                /// window in the Z order.
                /// </summary>
                Minimize = 6,
                /// <summary>
                /// Displays the window as a minimized window. This value is similar to
                /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
                /// window is not activated.
                /// </summary>
                ShowMinNoActive = 7,
                /// <summary>
                /// Displays the window in its current size and position. This value is 
                /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
                /// window is not activated.
                /// </summary>
                ShowNA = 8,
                /// <summary>
                /// Activates and displays the window. If the window is minimized or 
                /// maximized, the system restores it to its original size and position. 
                /// An application should specify this flag when restoring a minimized window.
                /// </summary>
                Restore = 9,
                /// <summary>
                /// Sets the show state based on the SW_* value specified in the 
                /// STARTUPINFO structure passed to the CreateProcess function by the 
                /// program that started the application.
                /// </summary>
                ShowDefault = 10,
                /// <summary>
                ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
                /// that owns the window is not responding. This flag should only be 
                /// used when minimizing windows from a different thread.
                /// </summary>
                ForceMinimize = 11
            }

            /// <summary>
            /// Activates and sets focus to the Window Object
            /// </summary>
            internal static void ActivateWindowHandle(IntPtr hWnd)
            {
                var threadId1 = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
                var threadId2 = GetWindowThreadProcessId(hWnd, IntPtr.Zero);

                if (threadId1 != threadId2)
                {
                    AttachThreadInput(threadId1, threadId2, 1);
                    SetForegroundWindow(hWnd);
                    AttachThreadInput(threadId1, threadId2, 0);
                }
                else
                {
                    SetForegroundWindow(hWnd);
                }

                if (IsIconic(hWnd))
                {
                    ShowWindowAsync(hWnd, ShowWindowCommands.Restore);
                }
                else
                {
                    ShowWindowAsync(hWnd, ShowWindowCommands.Show);
                }
            }
        }

        public static void Unminimize(this Window window)
        {
            if (window == null)
            {
                return;
            }
            var hwnd = new WindowInteropHelper(window).Handle;
            NativeMethods.ShowWindowAsync(hwnd, NativeMethods.ShowWindowCommands.Restore);
        }

        public static void ShowAndActivate(this Window window)
        {
            if (window == null)
            {
                return;
            }
            var hwnd = new WindowInteropHelper(window).Handle;
            NativeMethods.ActivateWindowHandle(hwnd);
        }

        /// <summary>
        /// Fits the window into the current screen.
        /// </summary>
        public static void FitIntoScreen(this Window w)
        {
            if (w == null)
            {
                return;
            }
            var workingArea = GetDesktopWorkingArea(w);
            var formSize = w.RestoreBounds;
            Rect newFormSize;
            if (FitIntoScreen(workingArea, formSize, out newFormSize))
            {
                LogHost.Default.Info($"FitIntoScreen: left={newFormSize.X}, top={newFormSize.Y}, width={newFormSize.Width}, height={newFormSize.Height}");
                w.Left = newFormSize.X;
                w.Top = newFormSize.Y;
                w.Width = newFormSize.Width;
                w.Height = newFormSize.Height;
            }
        }

        private static Rect GetDesktopWorkingArea(Window w)
        {
            var rectangle = Screen.GetWorkingArea(new System.Drawing.Point(Convert.ToInt32(w.Left + (w.Width / 2.0)), Convert.ToInt32(w.Top)));
            return new Rect(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        private static bool FitIntoScreen(Rect workArea, Rect formSize, out Rect newFormSize)
        {
            var hasChanged = false;
            newFormSize = formSize == Rect.Empty ? new Rect() : formSize;
            if (!workArea.Contains(formSize))
            {
                // limiting size guarantees form fits into screen
                newFormSize.Width = Math.Min(newFormSize.Width, workArea.Width);
                newFormSize.Height = Math.Min(newFormSize.Height, workArea.Height);
                if (newFormSize.Right > workArea.Right)
                {
                    newFormSize.Offset(workArea.Right - newFormSize.Right, 0);
                    hasChanged = true;
                }
                else if (newFormSize.Left < workArea.Left)
                {
                    newFormSize.Offset(workArea.Left - newFormSize.Left, 0);
                    hasChanged = true;
                }
                if (newFormSize.Top < workArea.Top)
                {
                    newFormSize.Offset(0, workArea.Top - newFormSize.Top);
                    hasChanged = true;
                }
                else if (newFormSize.Bottom > workArea.Bottom)
                {
                    newFormSize.Offset(0, workArea.Bottom - newFormSize.Bottom);
                    hasChanged = true;
                }
            }
            return hasChanged;
        }
    }
}