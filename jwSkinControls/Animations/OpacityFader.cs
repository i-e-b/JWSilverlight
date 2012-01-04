using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace jwSkinControls.Animations {
	public class OpacityFader {
		protected Storyboard OverAnimation, OutAnimation;

		public OpacityFader (DependencyObject target) {
			Setup(target, 0.0, 1.0);
		}

		public OpacityFader (DependencyObject target, double lower, double upper) {
			Setup(target, lower, upper);
		}

		private void Setup (DependencyObject target, double lower, double upper) {
			OverAnimation = new Storyboard();
			var ca = new DoubleAnimation{
				To = upper,
				Duration = TimeSpan.FromSeconds(0.2)
			};
			Storyboard.SetTarget(ca, target);
			Storyboard.SetTargetProperty(ca, new PropertyPath("(UIElement.Opacity)"));
			OverAnimation.Children.Add(ca);

			OutAnimation = new Storyboard();
			var ca2 = new DoubleAnimation{
				To = lower,
				Duration = TimeSpan.FromSeconds(0.2)
			};
			Storyboard.SetTarget(ca2, target);
			Storyboard.SetTargetProperty(ca2, new PropertyPath("(UIElement.Opacity)"));
			OutAnimation.Children.Add(ca2);
		}

		public void In () {
			OverAnimation.Begin();
		}

		public void Out () {
			OutAnimation.Begin();
		}
	}
}
