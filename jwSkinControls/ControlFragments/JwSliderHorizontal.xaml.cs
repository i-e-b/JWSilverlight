using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace jwSkinControls.ControlFragments {
	public partial class JwSliderHorizontal : UserControl {
		double bufferProg;
		double sliderProg;
		double thumbProg;
		bool isDragging;
		bool ignoreJump;

		public JwSliderHorizontal () {
			InitializeComponent();
		}

		/// <summary>
		/// Value between 0.0 and 1.0
		/// </summary>
		public double BufferProgress {
			get { return bufferProg; }
			set {
				bufferProg = value;
				ResizeBars();
			}
		}

		/// <summary>
		/// Value between 0.0 and 1.0
		/// </summary>
		public double SliderProgress {
			get { return sliderProg; }
			set {
				sliderProg = value;
				if (!isDragging) thumbProg = value;
				ResizeBars();
			}
		}

		public new Brush Background {
			set {
				OuterContainer.Background = value;
			}
		}

		public event EventHandler<ProportionEventArgs> TargetProportionChanged;

		/// <summary>
		/// If true, slider will scale to horizontal size of control.
		/// If false, slide will fix size to largest image supplied.
		/// </summary>
		public bool AutoScale { get; set; }

		public void SetSkin (BitmapImage rail, BitmapImage buffer, BitmapImage progress, BitmapImage thumb, BitmapImage leftCap, BitmapImage rightCap) {

			if (AutoScale) {
				SetWithHeight(Rail, rail);
				SetWithHeight(Buffer, buffer);
				SetWithHeight(Progress, progress);
			} else {
				SetWithHeightAndWidth(Rail, rail);
				SetWithHeightAndWidth(Buffer, buffer);
				SetWithHeightAndWidth(Progress, progress);
			}

			SetWithHeightAndWidth(Thumb, thumb);
			SetWithHeightAndWidth(LeftCap, leftCap);
			SetWithHeightAndWidth(RightCap, rightCap);

			if (AutoScale) return;

			LayoutRoot.Width = Max(rail.PixelWidth, buffer.PixelWidth, progress.PixelWidth);
			LayoutRoot.Height = Max(rail.PixelHeight, buffer.PixelHeight, progress.PixelHeight);
		}

		static int Max (params int[] values) { return values.Max(); }

		static void SetWithHeight(Image dst, BitmapImage src) {
			if (src == null) return;
			dst.Source = src;
			dst.Stretch = Stretch.Fill;
			dst.Height = src.PixelHeight;
		}

		static void SetWithHeightAndWidth (Image dst, BitmapImage src) {
			if (src == null) return;
			dst.Source = src;
			dst.Stretch = Stretch.None;
			dst.HorizontalAlignment = HorizontalAlignment.Left;
			dst.Height = src.PixelHeight;
			dst.Width = src.PixelWidth;
		}

		void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
			ResizeBars();
		}

		void ResizeBars () {
			var realWidth = LayoutRoot.ActualWidth;
			if (realWidth < 0) realWidth = 0.0;

			Buffer.Width = realWidth * BufferProgress;
			Progress.Width = realWidth * SliderProgress;

			if (realWidth > 0)
				Thumb.Margin = new Thickness(realWidth * thumbProg, 0, 0, 0);
		}

		public void InvokeTargetProportionChanged (double prop) {
			EventHandler<ProportionEventArgs> handler = TargetProportionChanged;
			if (handler != null) handler(this, new ProportionEventArgs { Proportion = prop });
		}

		private void JumpToPositionClick (object sender, MouseButtonEventArgs e) {
			if (ignoreJump) {
				ignoreJump = false;
				return;
			}
			if (Rail.ActualWidth < 1.0 || double.IsNaN(Rail.ActualWidth)) return;
			double prop = e.GetPosition(Rail).X / Rail.ActualWidth;
			if (prop < 0.0) prop = 0.0;
			if (prop > 1.0) prop = 1.0;
			InvokeTargetProportionChanged(prop);
		}

		private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			isDragging = Thumb.CaptureMouse();
			ignoreJump = isDragging;
			thumbProg = sliderProg;
		}

		private void Thumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (isDragging) Thumb.ReleaseMouseCapture();
			isDragging = false;
			InvokeTargetProportionChanged(thumbProg);
		}

		private void Thumb_MouseMove(object sender, MouseEventArgs e) {
			if (!isDragging) return;

			if (Rail.ActualWidth < 1.0 || double.IsNaN(Rail.ActualWidth)) return;
			double prop = e.GetPosition(Rail).X / Rail.ActualWidth;
			if (prop < 0.0) prop = 0.0;
			if (prop > 1.0) prop = 1.0;

			thumbProg = prop;
			ResizeBars();
		}
	}
}
