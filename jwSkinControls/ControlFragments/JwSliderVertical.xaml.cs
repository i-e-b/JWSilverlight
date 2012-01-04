using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace jwSkinControls.ControlFragments {
	public partial class JwSliderVertical : UserControl {
		double proportion, total, visible;
		bool isDragging;

		public JwSliderVertical () {
			InitializeComponent();
		}

		/// <summary>
		/// Value between 0.0 and 1.0
		/// </summary>
		public double Proportion {
			get { return proportion; }
			set {
				proportion = value;
				ResizeBars();
				InvokeTargetProportionChanged();
			}
		}

		public double VisibleHeight {
			get { return visible; }
			set {
				visible = value;
				ResizeBars();
			}
		}

		public double TotalHeight {
			get { return total; }
			set {
				total = value;
				ResizeBars();
			}
		}

		void ResizeBars() {
			if (!(SliderRail.ActualHeight > 0)) return;
			if (TotalHeight < 1) return;
			if (VisibleHeight > TotalHeight) {
				SliderThumb.Margin = new Thickness(0);
				SliderThumb.Height = SliderRail.ActualHeight;
				return;
			}

			var visibleProp = SliderRail.ActualHeight * (VisibleHeight / TotalHeight);
			var remains = SliderRail.ActualHeight - visibleProp;
			SliderThumb.Margin = new Thickness(0, remains * proportion, 0, remains * (1.0 - proportion));
		}

		public event EventHandler<ProportionEventArgs> TargetProportionChanged;
		public void InvokeTargetProportionChanged() {
			EventHandler<ProportionEventArgs> handler = TargetProportionChanged;
			if (handler != null) handler(this, new ProportionEventArgs { Proportion = proportion });
		}

		public void SetSkin (BitmapImage rail, BitmapImage thumb, BitmapImage topCap, BitmapImage bottomCap) {
			SetWithWidth(SliderRail, rail);
			SetWithWidth(SliderThumb, thumb);
			SetWithHeightAndWidth(SliderCapTop, topCap);
			SetWithHeightAndWidth(SliderCapBottom, bottomCap);

			Width = Max(rail.PixelWidth, thumb.PixelWidth, topCap.PixelWidth, bottomCap.PixelWidth);

			ResizeBars();
		}
		static int Max (params int[] values) { return values.Max(); }

		void SetWithWidth (Image dst, BitmapImage src) {
			if (src == null) return;
			dst.Source = src;
			dst.Stretch = Stretch.Fill;
			dst.VerticalAlignment = VerticalAlignment.Center;
			dst.Width = src.PixelWidth;
		}

		static void SetWithHeightAndWidth (Image dst, BitmapImage src) {
			if (src == null) return;
			dst.Source = src;
			dst.Stretch = Stretch.None;
			dst.VerticalAlignment = VerticalAlignment.Center;
			dst.Height = src.PixelHeight;
			dst.Width = src.PixelWidth;
		}

		private void SliderMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
			isDragging = SliderThumb.CaptureMouse();
		}

		private void SliderMouseLeftButtonUp (object sender, MouseButtonEventArgs e) {
			isDragging = false;
			SliderThumb.ReleaseMouseCapture();
		}

		private void SliderMouseMove(object sender, MouseEventArgs e) {
			if (!isDragging) return;
			if (!(SliderRail.ActualHeight > 0)) return;

			var pos = e.GetPosition(SliderRail).Y;

			// todo: this isn't the final logic (it's not drag bar like)
			Proportion = Pin(pos / SliderRail.ActualHeight);
		}

		static double Pin(double d) {
			if (d > 1) return 1;
			return d < 0 ? 0 : d;
		}

		private void LayoutRoot_SizeChanged (object sender, SizeChangedEventArgs e) {
			ResizeBars();
		}
	}
}
