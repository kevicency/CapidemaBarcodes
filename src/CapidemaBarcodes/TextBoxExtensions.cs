using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace codesomnia.CapidemaBarcodes
{
    public static class TextBoxExtensions
    {
        public static void FocusOnEnter(this FrameworkElement sender, FrameworkElement target)
        {
            sender.PreviewKeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    target.Focus();
                    if (target is TextBox)
                    {
                        (target as TextBox).SelectAll();
                    }
                }
            };
        }
    }
}