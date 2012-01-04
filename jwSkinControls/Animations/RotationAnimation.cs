using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace jwSkinControls.Animations {
	public class RotationAnimation {
		readonly FrameworkElement target;
		readonly double degrees;
		readonly RotateTransform rot;
		readonly DispatcherTimer timer;


		public bool IsRunning { get; private set; }

		public RotationAnimation (FrameworkElement target)
			: this(target, TimeSpan.FromMilliseconds(100), 15.0) {
		}

		public RotationAnimation (FrameworkElement target, TimeSpan interval, double degrees) {
			this.target = target;
			this.degrees = degrees;

			rot = new RotateTransform();

			rot.CenterX = target.Width / 2.0;
			rot.CenterY = target.Height / 2.0;

			timer = new DispatcherTimer();
			timer.Interval = interval;
			timer.Tick += timer_Tick;
			timer.Stop();
			IsRunning = false;
		}

		void timer_Tick(object sender, EventArgs e) {
			rot.Angle += degrees;
			target.RenderTransform = rot;
		}

		public void Start () {
			if (IsRunning) return;
			IsRunning = true;
			timer.Start();
		}

		public void Stop () {
			if (!IsRunning) return;
			IsRunning = false;
			timer.Stop();
		}
	}
}
