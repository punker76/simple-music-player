using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniversalWPF
{
	public class StateTrigger : StateTriggerBase
	{
		/// <summary>
		/// Initializes a new instance of the StateTrigger class.
		/// </summary>
		public StateTrigger() { }

		/// <summary>
		/// Gets or sets a value that indicates whether the trigger should be applied
		/// </summary>
		/// <returns>true if the system should apply the trigger; otherwise, false.</returns>
		public bool IsActive
		{
			get { return (bool)GetValue(IsActiveProperty); }
			set { SetValue(IsActiveProperty, value); }
		}

		/// <summary>Identifies the IsActive dependency property.</summary>
		/// <returns>The identifier for the IsActive dependency property.</returns>
		public static readonly DependencyProperty IsActiveProperty =
			DependencyProperty.Register("IsActive", typeof(bool), typeof(StateTrigger), new PropertyMetadata(false, OnIsActivePropertyChanged));

		private static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((StateTrigger)d).SetActive((bool)e.NewValue);
		}
	}
}
