using System.Windows.Controls;
using System.Windows.Input;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core
{
    public static class KeyHandlerExtensions
    {
        public static void HandlePreviewKeyDown(this IKeyHandler keyHandler, System.Windows.Input.KeyEventArgs e)
        {
            //bool fFocusedControlIsTextBox = FocusManager.GetFocusedElement(this).GetType().Equals(typeof(TextBox));
            var fFocusedControlIsTextBox = Keyboard.FocusedElement != null && Keyboard.FocusedElement.GetType() == typeof(TextBox);
            if (fFocusedControlIsTextBox)
            {
                e.Handled = false;
            }
            else if (keyHandler != null)
            {
                var handled = keyHandler.HandleKeyDown(e);
                e.Handled = handled;
            }
        }
    }
}