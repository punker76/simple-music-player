using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UniversalWPF
{
	public class AdaptiveTrigger : StateTriggerBase
	{


		public double MinWindowHeight
		{
			get { return (double)GetValue(MinWindowHeightProperty); }
			set { SetValue(MinWindowHeightProperty, value); }
		}

		public static readonly DependencyProperty MinWindowHeightProperty =
			DependencyProperty.Register("MinWindowHeight", typeof(double), typeof(AdaptiveTrigger), new PropertyMetadata(0d));

		public double MinWindowWidth
		{
			get { return (double)GetValue(MinWindowWidthProperty); }
			set { SetValue(MinWindowWidthProperty, value); }
		}

		public static readonly DependencyProperty MinWindowWidthProperty =
			DependencyProperty.Register("MinWindowWidth", typeof(double), typeof(AdaptiveTrigger), new PropertyMetadata(0d));


	}
}
