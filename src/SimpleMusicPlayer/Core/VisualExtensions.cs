using System;
using System.Windows;
using System.Windows.Media;

namespace SimpleMusicPlayer.Core
{
    public static class VisualExtensions
    {
        /// <summary>
        /// Gets a descendant by type at the given visual.
        /// </summary>
        /// <param name="visual">The visual that contains the descendant.</param>
        /// <param name="type">The type to search for the descendant.</param>
        /// <returns>Returns the searched descendant or null if nothing was found.</returns>
        public static Visual GetDescendantByType(this Visual visual, Type type)
        {
            if (visual == null)
            {
                return null;
            }

            if (visual.GetType() == type)
            {
                return visual;
            }

            // sometimes it's necessary to apply a template before getting the childrens
            var frameworkElement = visual as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.ApplyTemplate();
            }

            Visual foundElement = null;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(visual, i) as Visual;
                foundElement = GetDescendantByType(childVisual, type);
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }
    }
}