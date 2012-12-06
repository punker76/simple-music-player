using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SimpleMusicPlayer.Base
{
  public class ViewModelBaseNotifyPropertyChanged : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Called when [property changed].
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property type.</typeparam>
    /// <param name="projection">The projection.</param>
    public void OnPropertyChanged<TPropertyType>(Expression<Func<TPropertyType>> projection) {
      this.OnPropertyChanged(this.PropertyChanged, projection);
    }

    public void OnPropertyChanged(string thePropertyName) {
      this.OnPropertyChanged(this.PropertyChanged, thePropertyName);
    }
  }
}