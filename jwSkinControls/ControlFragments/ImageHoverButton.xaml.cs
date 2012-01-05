using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using jwSkinLoader;

namespace jwSkinControls.ControlFragments {
	public partial class ImageHoverButton : UserControl, IImageHoverControl {
		public ImageHoverButton () {
			InitializeComponent();
		}

		public BitmapImage OverImage {
			get { return Over.Source as BitmapImage; }
			set {
				Over.Source = value;
				Resize();
			}
		}

		public BitmapImage OutImage {
			get { return Out.Source as BitmapImage; }
			set {
				Out.Source = value;
				Resize();
			}
		}

		public BitmapImage BadgeImage {
			get { return Badge.Source as BitmapImage; }
			set {
				Badge.Source = value;
				Resize();
			}
		}

		public new Brush Background {
			set {
				LayoutRoot.Background = value;
			}
		}

		public string CaptionText {
			get { return Caption.Text; }
			set {
				Caption.Text = value;
				Caption.Visibility = string.IsNullOrEmpty(Caption.Text) ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public Color CaptionColor {
			set {
				Caption.Foreground = new SolidColorBrush(value);
			}
		}

		public BitmapImage BadgeImageOver {
			get { return BadgeOver.Source as BitmapImage; }
			set {
				BadgeOver.Source = value;
				Resize();
			}
		}

		void Resize() {
			var a = Over.Source as BitmapImage;
			var b = Out.Source as BitmapImage;
			var c = a ?? b;

			if (c == null) {
				Width = 0;
				Height = 0;
				return;
			}

			if (a == null || b == null) {
				Width = c.PixelWidth;
				Height = c.PixelHeight;
				return;
			}

			Width = Math.Max(a.PixelWidth, b.PixelWidth);
			Height = Math.Max(a.PixelHeight, b.PixelHeight);
		}

		private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
		{
			if (Over.Source != null) {
				Over.Visibility = Visibility.Visible;
				Out.Visibility = Visibility.Collapsed;
			}
			if (BadgeOver.Source != null) {
				BadgeOver.Visibility = Visibility.Visible;
				Badge.Visibility = Visibility.Collapsed;
			}
		}

		private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
		{
			Out.Visibility = Visibility.Visible;
			Badge.Visibility = Visibility.Visible;
			BadgeOver.Visibility = Visibility.Collapsed;
			Over.Visibility = Visibility.Collapsed;
		}

		public event EventHandler<MouseButtonEventArgs> Clicked;
		public void InvokeClicked(MouseButtonEventArgs e) {
			var handler = Clicked;
			if (handler != null) handler(this, e);
		}
		private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			InvokeClicked(e);
		}

		public void ClearEvents() {
			Clicked = null;
		}
	}
}
