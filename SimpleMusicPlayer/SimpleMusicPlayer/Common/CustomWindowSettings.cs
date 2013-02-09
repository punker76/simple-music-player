using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using MahApps.Metro.Native;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Common
{
  public class CustomWindowSettings : IWindowsSettings
  {
    public void SetSave(DependencyObject dependencyObject, bool enabled) {
      var window = dependencyObject as Window;
      if (window == null || !enabled) {
        return;
      }
      this.window = window;
      this.smpSettings = window.DataContext is MainWindowViewModel ? ((MainWindowViewModel)window.DataContext).SMPSettings : null;
      this.Attach();
    }

    private Window window;
    private SMPSettings smpSettings;

    private void LoadWindowState() {
      if (this.smpSettings.MainSettings.Placement == null) {
        return;
      }

      try {
        var wp = this.smpSettings.MainSettings.Placement.Value;

        wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
        wp.flags = 0;
        wp.showCmd = (wp.showCmd == Constants.SW_SHOWMINIMIZED ? Constants.SW_SHOWNORMAL : wp.showCmd);
        var hwnd = new WindowInteropHelper(this.window).Handle;
        UnsafeNativeMethods.SetWindowPlacement(hwnd, ref wp);
      }
      catch (Exception ex) {
        Debug.WriteLine(string.Format("Failed to load window state:\r\n{0}", ex));
      }
    }

    private void SaveWindowState() {
      WINDOWPLACEMENT wp;
      var hwnd = new WindowInteropHelper(this.window).Handle;
      UnsafeNativeMethods.GetWindowPlacement(hwnd, out wp);
      this.smpSettings.MainSettings.Placement = wp;
    }

    private void Attach() {
      if (this.window == null) {
        return;
      }
      this.window.Closing += this.WindowClosing;
      this.window.SourceInitialized += this.WindowSourceInitialized;
    }

    private void WindowSourceInitialized(object sender, EventArgs e) {
      this.LoadWindowState();
    }

    private void WindowClosing(object sender, CancelEventArgs e) {
      this.SaveWindowState();
      this.window.Closing -= this.WindowClosing;
      this.window.SourceInitialized -= this.WindowSourceInitialized;
      this.window = null;
      this.smpSettings = null;
    }

    private static CustomWindowSettings instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static CustomWindowSettings() {
    }

    private CustomWindowSettings() {
    }

    public static CustomWindowSettings Instance {
      get { return instance ?? (instance = new CustomWindowSettings()); }
    }
  }

  [SuppressUnmanagedCodeSecurity]
  internal static class UnsafeNativeMethods
  {
    [DllImport("user32.dll")]
    internal static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
  }
}