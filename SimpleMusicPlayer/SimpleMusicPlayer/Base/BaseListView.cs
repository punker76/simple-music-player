using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleMusicPlayer.Base
{
  public class BaseListView : ListView
  {
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
      base.OnPropertyChanged(e);

      if (e.Property.Name == "ItemsSource" && e.OldValue != e.NewValue && e.NewValue != null) {
        CreateColumns(this);
      }
    }

    private static void CreateColumns(BaseListView lv) {
      var gridView = new GridView();
      gridView.AllowsColumnReorder = true;
      var properties = lv.DataType.GetProperties();
      foreach (var pi in properties) {
        var browsable = pi.GetCustomAttributes(true).FirstOrDefault(a => a is BrowsableAttribute) as BrowsableAttribute;
        if (browsable != null && !browsable.Browsable) {
          continue;
        }
        var binding = new Binding { Path = new PropertyPath(pi.Name), Mode = BindingMode.OneWay };
        var gridViewColumn = new GridViewColumn() { Header = pi.Name, DisplayMemberBinding = binding };
        //var gridViewColumn = new GridViewColumn() { Header = pi.Name, CellTemplate = GetCellTemplate(binding) };
        gridView.Columns.Add(gridViewColumn);
      }
      lv.View = gridView;
    }

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

    public Type DataType { get; set; }
  }
}