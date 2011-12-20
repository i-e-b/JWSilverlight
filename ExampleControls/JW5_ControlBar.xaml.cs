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
			// todo: bind controls to events & player!
			// todo: background image & settings

			SetColumnDefinitions(layout);
			int i = 0;
			foreach (var element in layout.Elements) {
				FrameworkElement c;

				switch (element.Type) {
					case ControlBarElement.ElementType.Gap:
						i++; continue;

					case ControlBarElement.ElementType.Text:
						//c = new TextBoundControl(...);
						c = new TextBlock { Text = element.Name };
						break;

					case ControlBarElement.ElementType.Divider:
						c = new Image();
						pkg.BindAndResize((Image)c, "controlbar", element.Name ?? "divider");
						break;

					case ControlBarElement.ElementType.Button:
						c = new ImageHoverButton();
						pkg.BindHoverButton((ImageHoverButton)c, "controlbar", element.ElementName(), element.ElementName() + "Over");
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
			// todo: build volume slider
			return new TextBlock { Text = "Vol" };
		}

		FrameworkElement BuildTimeSlider(JwSkinPackage pkg) {
			// todo: build time slider
			return new TextBlock { Text = "TimeSliderHere" };
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