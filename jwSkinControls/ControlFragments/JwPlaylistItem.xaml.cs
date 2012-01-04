using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComposerCore;
using jwSkinLoader;

namespace jwSkinControls.ControlFragments {
	public partial class JwPlaylistItem : UserControl {
		IPlaylistItem plistItem;
		const string PlaylistComponent = "playlist";

		public JwPlaylistItem () {
			InitializeComponent();
			Active = false;
		}

		public IPlaylistItem PlaylistItem {
			get { return plistItem; }
			set {
				plistItem = value;
				UpdateValues();
			}
		}

		bool active;
		bool mouseOver;

		public bool Active {
			get { return active; }
			set {
				active = value;
				UpdateBackgroundState();
			}
		}

		void UpdateValues () {
			Resize();
			UpdateThumbnailImage();

			TitleBlock.Text = plistItem.Title;
			if (plistItem.StopPosition > 0) {
				var playTime = TimeSpan.FromSeconds(plistItem.StopPosition);
				DurationBlock.Text = playTime.Minutes.ToString("00") + ":" + playTime.Seconds.ToString("00"); 
				DurationBlock.Visibility = Visibility.Visible;
			} else {
				DurationBlock.Visibility = Visibility.Collapsed;
			}

			DescriptionBlock.Text = plistItem.Description;

			OutBackground.Visibility = Visibility.Visible;
			OverBackground.Visibility = Visibility.Collapsed;
			ActiveBackground.Visibility = Visibility.Collapsed;

		}

		void UpdateThumbnailImage() {
			if (plistItem.ThumbSource != null) {
				Thumbnail.Source = new BitmapImage(plistItem.ThumbSource.ForceAbsoluteByPage());
				if (ThumbnailBackground.Source != null) {
					Thumbnail.Height = ThumbnailBackground.Height;
					Thumbnail.Width = ThumbnailBackground.Width;
					Thumbnail.OpacityMask = new ImageBrush
											{
												ImageSource = ThumbnailBackground.Source,
												Stretch = Stretch.None,
												AlignmentX = AlignmentX.Left,
												AlignmentY = AlignmentY.Top
											};
				} else {
					Thumbnail.Height = Height;
					Thumbnail.Width = Height * 1.4; // quick hack!
				}
			}
		}

		public void SetSkin (JwSkinPackage pkg) {
			SetImages(
				pkg.GetNamedElement(PlaylistComponent, "item"),
				pkg.GetNamedElement(PlaylistComponent, "itemActive"),
				pkg.GetNamedElement(PlaylistComponent, "itemOver"),
				pkg.GetNamedElement(PlaylistComponent, "itemImage"));

			var outHex = pkg.GetSettingValue(PlaylistComponent, "fontcolor") ?? "0xffffff";
			outColor = (outHex).HexToColor();
			overColor = (pkg.GetSettingValue(PlaylistComponent, "overcolor") ?? outHex).HexToColor();
			activeColor = (pkg.GetSettingValue(PlaylistComponent, "activecolor") ?? outHex).HexToColor();
		}

		Color outColor, overColor, activeColor;


		public void SetImages (BitmapImage item, BitmapImage itemActive, BitmapImage itemOver, BitmapImage itemImageBackground) {
			SetWithHeight(OutBackground, item);
			SetWithHeight(OverBackground, itemOver);
			SetWithHeight(ActiveBackground, itemActive);

			SetWithHeightAndWidth(ThumbnailBackground, itemImageBackground);
			UpdateValues();
		}

		void Resize () {
			var a = OutBackground.Source as BitmapImage;
			var b = OverBackground.Source as BitmapImage;
			var c = a ?? b;

			if (c == null) {
				Height = 0;
				return;
			}

			if (a == null || b == null) {
				Height = c.PixelHeight;
				return;
			}

			Height = Math.Max(a.PixelHeight, b.PixelHeight);
		}

		static void SetWithHeight (Image dst, BitmapImage src) {
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

		private void LayoutRoot_MouseEnter (object sender, MouseEventArgs e) {
			mouseOver = true;
			UpdateBackgroundState();
		}

		private void LayoutRoot_MouseLeave (object sender, MouseEventArgs e) {
			mouseOver = false;
			UpdateBackgroundState();
		}

		void UpdateBackgroundState () {
			OutBackground.Visibility = Visibility.Visible;
			if (mouseOver) {
				if (Active && ActiveBackground.Source != null) {
					ActiveBackground.Visibility = Visibility.Visible;
					OverBackground.Visibility = Visibility.Collapsed;
				} else if (OverBackground.Source != null) {
					OverBackground.Visibility = Visibility.Visible;
					ActiveBackground.Visibility = Visibility.Collapsed;
				} else {
					ActiveBackground.Visibility = Visibility.Collapsed;
					OverBackground.Visibility = Visibility.Collapsed;
				}

			} else {// mouse out
				if (Active && ActiveBackground.Source != null) {
					ActiveBackground.Visibility = Visibility.Visible;
					OverBackground.Visibility = Visibility.Collapsed;
				} else {
					OverBackground.Visibility = Visibility.Collapsed;
					ActiveBackground.Visibility = Visibility.Collapsed;
				}
			}
			UpdateFontColours();
		}

		void UpdateFontColours () {
			SolidColorBrush brush;
			if (Active) {
				brush = new SolidColorBrush(activeColor);
			} else if (mouseOver) {
				brush = new SolidColorBrush(overColor);
			} else {
				brush = new SolidColorBrush(outColor);
			}
			TitleBlock.Foreground = brush;
			DurationBlock.Foreground = brush;
			DescriptionBlock.Foreground = brush;
		}

		public event EventHandler<IndexEventArgs> Clicked;
		public void InvokeClicked () {
			var handler = Clicked;
			if (handler != null) handler(this, new IndexEventArgs { Index = PlaylistItem.PlaylistIndex });
		}
		private void LayoutRoot_MouseLeftButtonUp (object sender, MouseButtonEventArgs e) {
			InvokeClicked();
		}
	}
}
