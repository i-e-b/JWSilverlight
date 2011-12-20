using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComposerCore;
using jwSkinLoader;
using Microsoft.Web.Media.SmoothStreaming;

namespace ExampleControls {
	public partial class JW5_ControlBar : UserControl, IPlayerController, IXmlSkinReader {
		const string ControlBarComponent = "controlbar";
		readonly ComposerControlHelper players;

		JwElapsedText elapsedText;
		JwDurationText durationText;
		JwSliderHorizontal volumeSlider;
		JwSliderHorizontal timeSlider;
		ImageHoverButton playButton;
		ImageHoverButton pauseButton;
		ImageHoverButton fullScreenButton;
		ImageHoverButton normalScreenButton;

		public JW5_ControlBar () {
			InitializeComponent();
			players = new ComposerControlHelper();
			BindFullScreenEvents();
		}

		void BindFullScreenEvents() { 
			if (Application.Current == null) return;
			Application.Current.Host.Content.FullScreenChanged+=UpdateFullScreenButtonState;
		}

		#region Skinning
		public void SetSkin (JwSkinPackage pkg) {
			var layout = new ControlBarLayout(pkg);
			BuildControls(pkg, layout);

			SetBackground(pkg);
			// todo: margin on full-screen.

			UpdateFullScreenButtonState(null, null);
			ShowPlayButton();
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
						c = BindButton(element, pkg);
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

		FrameworkElement BindButton(ControlBarElement element, JwSkinPackage pkg) {
			var btn = new ImageHoverButton();
			pkg.BindHoverButton(btn, ControlBarComponent, element.ElementName(), element.ElementName() + "Over");
			btn.Clicked += GetBinding(element.Name);

			if (element.Name == "play") playButton = btn;
			if (element.Name == "pause") pauseButton = btn;

			if (element.Name == "fullscreen") fullScreenButton = btn;
			if (element.Name == "normalscreen") normalScreenButton = btn;

			return btn;
		}

		private EventHandler<MouseButtonEventArgs> GetBinding (string name) {
			switch (name) {
				case "play":
					return Play;
				case "pause":
					return Pause;
				case "fullscreen":
				case "normalscreen":
					return SwitchFullScreen;
				default:
					return Ignore;
			}
		}

		void Play (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => p.Play()); }
		void Pause (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => p.Pause()); }
		void Ignore (object sender, MouseButtonEventArgs e) { }
		void SwitchFullScreen (object sender, MouseButtonEventArgs e) {
			if (Application.Current == null) return;

			if (Application.Current.Host.Content.IsFullScreen) {
				Application.Current.Host.Content.IsFullScreen = false;
			} else {
				players.EachPlayer(p => p.Pause());
				Application.Current.Host.Content.IsFullScreen = true;
				players.EachPlayer(p => p.Play());
			}
		}

		FrameworkElement BuildVolumeSlider(JwSkinPackage pkg) {
			// todo: bind events
			volumeSlider = new JwSliderHorizontal();
			volumeSlider.SetSkin(
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderRail"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderBuffer"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderProgress"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderThumb"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderCapLeft"),
				pkg.GetNamedElement(ControlBarComponent, "volumeSliderCapRight"));

			volumeSlider.BufferProgress = 0.75;
			volumeSlider.SliderProgress = 0.25;
			volumeSlider.Margin = new Thickness(0);
			return volumeSlider;
		}

		FrameworkElement BuildTimeSlider(JwSkinPackage pkg) {
			// todo: bind events
			timeSlider = new JwSliderHorizontal();
			timeSlider.AutoScale = true;
			timeSlider.SetSkin(
				pkg.GetNamedElement(ControlBarComponent, "timeSliderRail"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderBuffer"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderProgress"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderThumb"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderCapLeft"),
				pkg.GetNamedElement(ControlBarComponent, "timeSliderCapRight"));

			timeSlider.BufferProgress = 0.75;
			timeSlider.SliderProgress = 0.25;
			timeSlider.Margin = new Thickness(0);

			timeSlider.TargetProportionChanged += TimeSlider_TargetProportionChanged;

			return timeSlider;
		}

		void TimeSlider_TargetProportionChanged(object sender, ProportionEventArgs e) {
			players.EachPlayer(p => p.SeekTo(e.Proportion));
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
			StatusUpdate(NewStatus);
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
			if (timeSlider != null) {
				timeSlider.SliderProgress = NewStatus.PlayProgress;
				timeSlider.BufferProgress = NewStatus.BufferingProgress;
			}

			switch (NewStatus.CurrentPlayState) {
				case SmoothStreamingMediaElementState.Playing:
					ShowPauseButton();
					break;

				default:
					ShowPlayButton();
					break;
			}
		}

		void ShowPlayButton () {
			if (pauseButton == null || playButton == null) return;
			playButton.Visibility = Visibility.Visible;
			pauseButton.Visibility = Visibility.Collapsed;
		}

		void ShowPauseButton() {
			if (pauseButton == null || playButton == null) return;
			playButton.Visibility = Visibility.Collapsed;
			pauseButton.Visibility = Visibility.Visible;
		}

		void UpdateFullScreenButtonState(object sender, EventArgs e) {
			if (Application.Current == null) return;
			if (fullScreenButton == null || normalScreenButton == null) return;
			if (Application.Current.Host.Content.IsFullScreen) {
				fullScreenButton.Visibility = Visibility.Collapsed;
				normalScreenButton.Visibility = Visibility.Visible;
			} else {
				normalScreenButton.Visibility = Visibility.Collapsed;
				fullScreenButton.Visibility = Visibility.Visible;
			}
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