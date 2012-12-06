using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SimpleMusicPlayer.Base
{
  /// <summary>
  /// Generic implementation of a safe way to raise NotifyPropertyChanged-event for a property.
  /// Use this to avoid hard-coding property names.
  /// </summary>
  public static class NotifyPropertyChangedExtensions
  {
    public static void OnPropertyChanged<TSenderType, TPropType>(this TSenderType sender, PropertyChangedEventHandler handler, Expression<Func<TPropType>> projection) where TSenderType : INotifyPropertyChanged {
      var tmp = handler;
      if (tmp != null) {
        tmp(sender, new PropertyChangedEventArgs(PropertyName(projection)));
      }
    }

    public static void OnPropertyChanged<TSenderType>(this TSenderType sender, PropertyChangedEventHandler handler, string propertyName) where TSenderType : INotifyPropertyChanged {
      var tmp = handler;
      if (tmp != null) {
        tmp(sender, new PropertyChangedEventArgs(propertyName));
      }
    }

    /// <summary>
    /// Gets the Property name in a type-safe way, used for PropertyChanged-Events.
    /// </summary>
    /// <typeparam name="TPropType">the return type of the property.</typeparam>
    /// <param name="projection">The property-projection, s.th. like x=&gt;x.PropertyName.</param>
    /// <returns>the PropertyName.</returns>
    private static string PropertyName<TPropType>(Expression<Func<TPropType>> projection) {
      var memberExpression = (MemberExpression)projection.Body;
      return memberExpression.Member.Name;
    }
  }
}