using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace SimpleMusicPlayer.Core
{
    public static class FolderBrowserHelper
    {
        /// <summary>
        /// Opens a folder browser dialog and returns the selected folders.
        /// </summary>
        /// <param name="owner">The owner for the dialog.</param>
        /// <returns>A list with the selected folders.</returns>
        public static IList<string> GetFolders(Window owner)
        {
            if (CommonFileDialog.IsPlatformSupported)
            {
                using (var dialog = new CommonOpenFileDialog())
                {
                    dialog.IsFolderPicker = true;
                    dialog.Multiselect = true;
                    var result = dialog.ShowDialog(owner);
                    if (result == CommonFileDialogResult.Ok)
                    {
                        return dialog.FileNames.ToList();
                    }
                }
            }
            else
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    var result = owner is System.Windows.Forms.IWin32Window
                                   ? dialog.ShowDialog((System.Windows.Forms.IWin32Window)owner)
                                   : dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        return new[] { dialog.SelectedPath };
                    }
                }
            }
            return Enumerable.Empty<string>().ToList();
        }
    }
}