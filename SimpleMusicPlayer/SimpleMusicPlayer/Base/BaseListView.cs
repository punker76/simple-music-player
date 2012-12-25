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
    public BaseListView() {
      ItemsSourceProperty.AddOwner(typeof(BaseListView), new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));
    }

    private static void OnItemsSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
      if (e.OldValue != e.NewValue && e.NewValue != null) {
        var lv = (BaseListView)dependencyObject;
        var gridView = new GridView();
        gridView.AllowsColumnReorder = true;
        var properties = lv.DataType.GetProperties();
        foreach (var pi in properties) {
          var browsable = pi.GetCustomAttributes(true).FirstOrDefault(a => a is BrowsableAttribute) as BrowsableAttribute;
          if (browsable != null && !browsable.Browsable) {
            continue;
          }
          var binding = new Binding {Path = new PropertyPath(pi.Name), Mode = BindingMode.OneWay};
          var gridViewColumn = new GridViewColumn() {Header = pi.Name, DisplayMemberBinding = binding};
          gridView.Columns.Add(gridViewColumn);
        }
        lv.View = gridView;
      }
    }

    public Type DataType { get; set; }
  }
}