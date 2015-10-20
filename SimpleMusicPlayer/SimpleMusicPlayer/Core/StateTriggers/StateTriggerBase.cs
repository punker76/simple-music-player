using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniversalWPF
{
	public abstract class StateTriggerBase : DependencyObject
	{
		private bool _isActive;

		/// <summary>
		/// Initializes a new instance of the StateTriggerBase class.
		/// </summary>
		protected StateTriggerBase() { }

		/// <summary>
		/// Sets the value that indicates whether the state trigger is active.
		/// </summary>
		/// <param name="IsActive">true if the system should apply the trigger; otherwise, false.</param>
		protected void SetActive(bool IsActive)
		{
			_isActive = IsActive;
			if (Owner != null && Owner.Storyboard != null)
			{
				Owner.SetActive(_isActive);
			}
		}

		internal bool IsTriggerActive { get { return _isActive; } }

		internal VisualStateUwp Owner { get; set; }

	}
}
