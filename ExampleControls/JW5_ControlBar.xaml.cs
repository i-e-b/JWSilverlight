using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;
using jwSkinLoader;

namespace ExampleControls {
	public partial class JW5_ControlBar : UserControl, IPlayerController, IXmlSkinReader {
		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }
		private readonly ComposerControlHelper players;
		const string ControlBarComponent = "controlbar";

		public JW5_ControlBar () {
			InitializeComponent();
			players = new ComposerControlHelper();
		}
		public void StateChanged (PlayerStatus NewStatus) {
			StatusUpdate(NewStatus);
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
		}
		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		public void SetSkin (JwSkinPackage pkg) {
			var layout = new ControlBarLayout(pkg);
			BuildControls(pkg, layout);
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
						// todo: ... c = new TextBoundControl(...); ...
						c = new TextBlock { Text = element.Name };
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
	}
}