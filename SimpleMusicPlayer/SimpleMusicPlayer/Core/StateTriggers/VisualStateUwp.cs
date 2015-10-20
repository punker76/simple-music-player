using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace UniversalWPF
{
	public class VisualStateUwp : System.Windows.VisualState, System.ComponentModel.ISupportInitialize
	{
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public class SetterBaseCollection : ObservableCollection<System.Windows.Setter> { }
        private SetterBaseCollection _setters;
		private ObservableCollection<StateTriggerBase> _triggers;

        public VisualStateUwp()
		{
			_setters = new SetterBaseCollection();
			_triggers = new ObservableCollection<StateTriggerBase>();
			_triggers.CollectionChanged += triggers_CollectionChanged;
			_setters.CollectionChanged += _setters_CollectionChanged;
		}

		private void _setters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			SetActive(_triggers.Where(t => t.IsTriggerActive).Any());
		}

		private void triggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems.OfType<StateTriggerBase>())
				{
					item.Owner = this;
				}
			}
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems.OfType<StateTriggerBase>())
				{
					if (item.Owner == this)
						item.Owner = null;
				}
			}
			SetActive(_triggers.Where(t => t.IsTriggerActive).Any());
		}

        Action afterInit;
		internal void SetActive(bool active)
		{
            if(_isInitializing)
            {
                afterInit = () => SetActive(active);
                return;
            }
			if (Storyboard != null)
			{
				if (active)
					Storyboard.Begin();
				else
					Storyboard.Stop();
			}

			//var storyboard = new System.Windows.Media.Animation.Storyboard();
			foreach(var setter in _setters.OfType< System.Windows.Setter>())
			{
				System.Windows.DependencyProperty property = setter.Property;
				object value = setter.Value; //Why doesn't this  return the actual value???
				string targetName = setter.TargetName;

				//var s = new System.Windows.Media.Animation.DoubleAnimation() { };
				//System.Windows.Media.Animation.Storyboard.SetTargetName(s, setter.TargetName);
				//System.Windows.Media.Animation.Storyboard.SetTargetProperty(s, new System.Windows.PropertyPath(string.Format("({0}.{1})", setter.Property.OwnerType.Name, setter.Property.Name )));
				//s.To = (double)setter.Value;
				//storyboard.Children.Add(s);

				//This isn't really working... need a better way
				//if (System.Windows.Application.Current.MainWindow != null)
				//{
				//	if (System.Windows.Application.Current.MainWindow.IsLoaded)
				//	{
				//		var target = System.Windows.Application.Current.MainWindow.FindName(targetName) as System.Windows.DependencyObject;
				//		if (target != null)
				//			target.SetValue(property, value);
				//	}
				//	else
				//		System.Windows.Application.Current.MainWindow.Loaded += (s, e) =>
				//		{
				//			var target = System.Windows.Application.Current.MainWindow.FindName(targetName) as System.Windows.DependencyObject;
				//			if (target != null)
				//				target.SetValue(property, value);
				//		};
				//}
			}
			//storyboard.Begin()
		}
        private bool _isInitializing;

        void ISupportInitialize.BeginInit()
        {
            _isInitializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            _isInitializing = false;
            if (afterInit != null)
                afterInit();
        }

		/// <summary>
		/// Gets a collection of Setter objects
		/// </summary>
		/// <returns>A collection of Setter objects. The default is an empty collection.</returns>
		public SetterBaseCollection Setters { get { return _setters; } }
		
		/// <summary>
		/// Gets a collection of StateTriggerBase objects.
		/// </summary>
		/// <returns>A collection of StateTriggerBase objects. The default is an empty collection.</returns>
		public ObservableCollection<StateTriggerBase> StateTriggers { get { return _triggers; } }
	}
}
