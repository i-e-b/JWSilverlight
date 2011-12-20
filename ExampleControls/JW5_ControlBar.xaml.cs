using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;
using jwSkinLoader;

namespace ExampleControls {
	public partial class JW5_ControlBar : UserControl, IPlayerController, IXmlSkinReader {
		const string ControlBarComponent = "controlbar";
		readonly ComposerControlHelper players;

		JwElapsedText elapsedText;
		JwDurationText durationText;

		public JW5_ControlBar () {
			InitializeComponent();
			players = new ComposerControlHelper();
		}

		#region Skinning
		public void SetSkin (JwSkinPackage pkg) {
			var layout = new ControlBarLayout(pkg);
			BuildControls(pkg, layout);

			SetBackground(pkg);
			// todo: margin on full-screen.


		}

		void SetBackground(JwSkinPackage pkg) {
			var img = pkg.GetNamedElement(ControlBarComponent, "background");
			if (img == null) return;

			var bgBrush = new ImageBrush{ImageSource = img, Stretch = Stretch.Fill};
			LayoutRoot.Background = bgBrush;
		}

		void BuildControls(JwSkinPackage pkg, ControlBarLayout layout) {
			SetColumnDefinitions(layout);
			int i = 0;
			foreach (var element in layout.Elements) {
				FrameworkElement c;

				switch (element.Type) {
					case ControlBarElement.ElementType.Gap:
						i++; continue;

					case ControlBarElement.ElementType.Text:
						if (element.Name == "elapsed") {
							elapsedText = new JwElapsedText();
							c = elapsedText;
							players.EachPlayer(p => players.AddBinding(p, elapsedText));
						} else if (element.Name == "duration") {
							durationText = new JwDurationText();
							c = durationText;
							players.EachPlayer(p => players.AddBinding(p, durationText));
						} else {
							i++; continue;
						}
						break;

					case ControlBarElement.ElementType.Divider:
						c = new Image();
						pkg.BindAndResize((Image)c, ControlBarComponent, element.Name ?? "divider");
						break;

					case ControlBarElement.ElementType.Button:
						c = new ImageHoverButton();
						pkg.BindHoverButton((ImageHoverButton)c, ControlBarComponent, element.ElementName(), element.ElementName() + "Over");
						// todo: bind to an event!
						break;

					case ControlBarElement.ElementType.TimeSlider:
						c = BuildTimeSlider(pkg);
						break;

					case ControlBarElement.ElementType.VolumeSlider:
						c = BuildVolumeSlider(pkg);
						break;

					default:
						i++; continue;
				}

				LayoutRoot.Children.Add(c);
				c.SetValue(Grid.ColumnProperty, i);
				i++;
			}
		}

		FrameworkElement BuildVolumeSlider(JwSkinPackage pkg) {
			// todo: bind events
			var slider = new JwSliderHorizontal();
			slider.SetSkin(
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderRail"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderBuffer"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderProgress"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderThumb"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderCapLeft"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderCapRight"));

			slider.BufferProgress = 0.75;
			slider.SliderProgress = 0.25;
			slider.Margin = new Thickness(0);
			return slider;
		}

		FrameworkElement BuildTimeSlider(JwSkinPackage pkg) {
			// todo: bind events
			var slider = new JwSliderHorizontal();
			slider.AutoScale = true;
			slider.SetSkin(
				pkg.GetNamedElement(ControlBarComponent, "timeSliderRail"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderBuffer"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderProgress"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderThumb"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderCapLeft"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderCapRight"));

			slider.BufferProgress = 0.75;
			slider.SliderProgress = 0.25;
			slider.Margin = new Thickness(0);
			return slider;
		}

		void SetColumnDefinitions(ControlBarLayout layout) {
			LayoutRoot.ColumnDefinitions.Clear();
			foreach (var element in layout.Elements) {
				// This isn't what the spec says, but behaves like the actual player.
				if (element.Type == ControlBarElement.ElementType.TimeSlider) {
					LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
				} else {
					LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
				}
			}
		}
		#endregion

		#region Player controls
		public void StateChanged (PlayerStatus NewStatus) {
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
		}
		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
			if (elapsedText != null) players.AddBinding(PlayerToControl, elapsedText); 
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
			if (elapsedText != null) players.RemoveBinding(PlayerToControl, elapsedText); 
		}
		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }
		#endregion
	}
}