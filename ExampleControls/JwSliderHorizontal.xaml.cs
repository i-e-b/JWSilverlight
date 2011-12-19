using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ExampleControls {
	public partial class JwSliderHorizontal : UserControl {
		double bufferProg;
		double sliderProg;
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
				ResizeBars();
			}
		}

		/// <summary>
		/// If true, slider will scale to horizontal size of control.
		/// If false, slide will fix size to largest image supplied.
		/// </summary>
		public bool AutoScale { get; set; }

		public void SetSkin (BitmapImage rail, BitmapImage buffer, BitmapImage progress, BitmapImage thumb, BitmapImage leftCap, BitmapImage rightCap) {
			SetWithHeight(Rail, rail);
			SetWithHeight(Buffer, buffer);
			SetWithHeight(Progress, progress);
			SetWithHeight(Thumb, thumb);
			SetWithHeight(LeftCap, leftCap);
			SetWithHeight(RightCap, rightCap);

			LeftCap.Width = leftCap.PixelWidth;
			RightCap.Width = rightCap.PixelWidth;

			if (AutoScale) return;

			Width = Max(rail.PixelWidth, buffer.PixelWidth, progress.PixelWidth);
			Height = Max(rail.PixelHeight, buffer.PixelHeight, progress.PixelHeight);
		}

		static int Max (params int[] values) { return values.Max(); }

		static void SetWithHeight(Image dst, BitmapImage src) {
			if (src == null) return;
			dst.Source = src;
			dst.Height = src.PixelHeight;
		}

		void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
			ResizeBars();
		}

		void ResizeBars () {
			if (!AutoScale) return;

			Buffer.Width = Width * BufferProgress;
			Progress.Width = Width * SliderProgress;
			Thumb.Margin = new Thickness(Width * SliderProgress, 0, 0, 0);
		}
	}
}
