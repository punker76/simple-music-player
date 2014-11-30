using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleMusicPlayer.Core
{
    public class BaseListView : ListView
    {
        public Type DataType { get; set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == "ItemsSource"
                && e.OldValue != e.NewValue
                && e.NewValue != null
                && this.DataType != null)
            {
                CreateColumns(this);
            }
        }

        private static void CreateColumns(BaseListView lv)
        {
            var gridView = new GridView { AllowsColumnReorder = true };

            var properties = lv.DataType.GetProperties();
            foreach (var pi in properties)
            {
                var browsableAttribute = pi.GetCustomAttributes(true).FirstOrDefault(a => a is BrowsableAttribute) as BrowsableAttribute;
                if (browsableAttribute != null && !browsableAttribute.Browsable)
                {
                    continue;
                }

                var binding = new Binding { Path = new PropertyPath(pi.Name), Mode = BindingMode.OneWay };
                var gridViewColumn = new GridViewColumn() { Header = pi.Name, DisplayMemberBinding = binding };
                gridView.Columns.Add(gridViewColumn);
            }

            lv.View = gridView;
        }

        //var gridViewColumn = new GridViewColumn() { Header = pi.Name, CellTemplate = GetCellTemplate(binding) };
        private static DataTemplate GetCellTemplate(Binding binding)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            //factory.SetValue(RenderOptions.ClearTypeHintProperty, ClearTypeHint.Enabled);
            //factory.SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            //factory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            factory.SetBinding(TextBlock.TextProperty, binding);
            template.VisualTree = factory;

            return template;
        }
    }
}