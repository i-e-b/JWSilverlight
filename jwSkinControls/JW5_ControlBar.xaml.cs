using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComposerCore;
using jwSkinControls.ControlFragments;
using jwSkinLoader;
using Microsoft.Web.Media.SmoothStreaming;

namespace jwSkinControls {
	public partial class JW5_ControlBar : UserControl, IPlayerController, IXmlSkinReader {
		const string ControlBarComponent = "controlbar";
		readonly ComposerControlHelper players;

		double ControlbarOverMargin;
		JwElapsedText elapsedText;
		JwDurationText durationText;
		JwSliderHorizontal volumeSlider;
		JwSliderHorizontal timeSlider;
		ImageHoverButton playButton;
		ImageHoverButton pauseButton;
		ImageHoverButton fullScreenButton;
		ImageHoverButton normalScreenButton;
		ImageHoverButton muteButton;
		ImageHoverButton unmuteButton;
		Image leftCap, rightCap;
		ImageBrush backgroundBrush;
		double TargetFontSize;
		Color FontColour;

		public JW5_ControlBar () {
			InitializeComponent();
			players = new ComposerControlHelper();
			BindFullScreenEvents();
		}

		public double BarHeight {
			get { return LayoutRoot.ActualHeight; }
		}

		void BindFullScreenEvents() { 
			if (Application.Current == null) return;
			Application.Current.Host.Content.FullScreenChanged+=UpdateFullScreenButtonState;
		}

		#region Skinning
		public void SetSkin (JwSkinPackage pkg) {
			GetBackground(pkg);
			LayoutRoot.SizeChanged+=LayoutRoot_SizeChanged;

			ControlbarOverMargin = double.Parse(pkg.GetSettingValue(ControlBarComponent, "margin") ?? "0.0");
			TargetFontSize = double.Parse(pkg.GetSettingValue(ControlBarComponent, "fontsize") ?? "10.0");
			FontColour = (pkg.GetSettingValue(ControlBarComponent, "fontcolor") ?? "0xffffff").HexToColor();

			var layout = new ControlBarLayout(pkg);
			BuildControls(pkg, layout);

			UpdateFullScreenButtonState(null, null);
			UpdateSoundButtonState();
			ShowPlayButton();
			PaddingBorder.Padding = new Thickness(ControlbarOverMargin);
		}

		void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e) {
			ResetControlHeights();
			SetBackground();
		}

		void SetBackground() {
			BackgroundCanvas.Background = backgroundBrush;
			BackgroundCanvas.Height = GetTargetHeight();

			double margLeft = 0.0, margRight = 0.0;
			if (leftCap != null && !double.IsNaN(leftCap.ActualWidth)) margLeft = leftCap.ActualWidth;
			if (rightCap != null && !double.IsNaN(rightCap.ActualWidth)) margRight = rightCap.ActualWidth;

			BackgroundCanvas.Margin = new Thickness(margLeft, 0.0, margRight, 0.0);
		}

		/// <summary>
		/// Some skins have invalid button heights;
		/// This corrects the container controls to compensate.
		/// </summary>
		void ResetControlHeights() {
			double targetHeight = GetTargetHeight();
			if (double.IsNaN(targetHeight) || targetHeight <= 0.0) return;
			foreach (var child in LayoutRoot.Children) {
				var elem = child as FrameworkElement;
				if (elem != null) {
					elem.Height = targetHeight;
				}
				var img = child as Image;
				if (img != null) {
					img.Stretch = Stretch.Fill;
					img.VerticalAlignment = VerticalAlignment.Stretch;
					img.Height = targetHeight;
					continue;
				}
			}
		}

		double GetTargetHeight() {
			var bgHeight = ((BitmapImage)backgroundBrush.ImageSource).PixelHeight;
			var layoutHeight = LayoutRoot.ActualHeight;
			if (!double.IsNaN(bgHeight) && bgHeight > 1) return bgHeight;
			if (!double.IsNaN(layoutHeight) && layoutHeight > 1) return layoutHeight;
			return 75.0; // safety net
		}

		void GetBackground (JwSkinPackage pkg) {
			var img = pkg.GetNamedElement(ControlBarComponent, "background");
			if (img == null) return;
			backgroundBrush = new ImageBrush {
				ImageSource = img,
				Stretch = Stretch.Fill,
				AlignmentX = AlignmentX.Left,
				AlignmentY = AlignmentY.Top
			};
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
							elapsedText = new JwElapsedText {
								FontSize = TargetFontSize,
								FontColour = FontColour
							};
							c = elapsedText;
							players.EachPlayer(p => players.AddBinding(p, elapsedText));
						} else if (element.Name == "duration") {
							durationText = new JwDurationText {
								FontSize = TargetFontSize,
								FontColour = FontColour
							};
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

					case ControlBarElement.ElementType.CapLeft:
						c = leftCap = new Image();
						pkg.BindAndResize(leftCap, ControlBarComponent, element.Name ?? "divider");
						break;

					case ControlBarElement.ElementType.CapRight:
						c = rightCap = new Image();
						pkg.BindAndResize(rightCap, ControlBarComponent, element.Name ?? "divider");
						break;

					case ControlBarElement.ElementType.Button:
						var btn = BindButton(element, pkg);
						c = btn;
						break;

					case ControlBarElement.ElementType.TimeSlider:
						var tsl = BuildTimeSlider(pkg);
						c = tsl;
						break;

					case ControlBarElement.ElementType.VolumeSlider:
						var vsl = BuildVolumeSlider(pkg);
						c = vsl;
						break;

					default:
						i++; continue;
				}

				LayoutRoot.Children.Add(c);
				c.SetValue(Grid.ColumnProperty, i);
				i++;
			}
		}

		ImageHoverButton BindButton (ControlBarElement element, JwSkinPackage pkg) {
			var btn = new ImageHoverButton();
			pkg.BindHoverButton(btn, ControlBarComponent, element.ElementName(), element.ElementName() + "Over");
			btn.Clicked += GetBinding(element.Name);

			if (element.Name == "play") playButton = btn;
			if (element.Name == "pause") pauseButton = btn;

			if (element.Name == "fullscreen") fullScreenButton = btn;
			if (element.Name == "normalscreen") normalScreenButton = btn;

			if (element.Name == "mute") muteButton = btn;
			if (element.Name == "unmute") unmuteButton = btn;

			return btn;
		}

		private EventHandler<MouseButtonEventArgs> GetBinding (string name) {
			switch (name) {
				case "play":
					return Play;
				case "pause":
					return Pause;
				case "stop":
					return Stop;
				case "fullscreen":
				case "normalscreen":
					return SwitchFullScreen;
				case "prev":
					return PrevClip;
				case "next":
					return NextClip;
				case "mute":
					return Mute;
				case "unmute":
					return Unmute;
				default:
					return Ignore;
			}
		}

		JwSliderHorizontal BuildVolumeSlider (JwSkinPackage pkg) {
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

			volumeSlider.TargetProportionChanged += VolumeSlider_TargetProportionChanged;

			volumeSlider.Margin = new Thickness(0);
			return volumeSlider;
		}

		void VolumeSlider_TargetProportionChanged(object sender, ProportionEventArgs e) {
			players.EachPlayer(p => p.AudioVolume = e.Proportion);
			players.EachPlayer(p => { if (p.Mute) p.Mute = false; });
			UpdateSoundButtonState();
		}

		JwSliderHorizontal BuildTimeSlider (JwSkinPackage pkg) {
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
		void Play (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => p.Play()); }
		void Pause (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => p.Pause()); }
		void Stop (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => { p.Pause(); p.SeekTo(0); }); }
		void PrevClip (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => p.GoToPlaylistIndex(p.CurrentIndex - 1)); }
		void NextClip (object sender, MouseButtonEventArgs e) { players.EachPlayer(p => p.GoToPlaylistIndex(p.CurrentIndex + 1)); }
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


		public void MuteChanged(bool IsMuted) { 
			UpdateSoundButtonState(); 
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

		void Mute (object sender, MouseButtonEventArgs e) {
			players.EachPlayer(p => p.Mute = true);
			UpdateSoundButtonState();
		}

		void Unmute (object sender, MouseButtonEventArgs e) {
			players.EachPlayer(p => p.Mute = false);
			UpdateSoundButtonState();
		}

		void UpdateSoundButtonState () {
			if (muteButton != null && unmuteButton != null) {
				if (players.Any(p => p.Mute)) {
					muteButton.Visibility = Visibility.Collapsed;
					unmuteButton.Visibility = Visibility.Visible;
				} else {
					muteButton.Visibility = Visibility.Visible;
					unmuteButton.Visibility = Visibility.Collapsed;
				}
			}
			if (volumeSlider != null) {
				if (players.Any(p => p.Mute)) {
					volumeSlider.BufferProgress = 0.0;
					volumeSlider.SliderProgress = 0.0;
				} else {
					volumeSlider.BufferProgress = 1.0;
					volumeSlider.SliderProgress = players.PlayerList[0].AudioVolume;
				}
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
		public void PlaylistChanged (IPlaylist NewPlaylist) { }
		public void PlayingClipChanged(IPlaylistItem NewClip) { }
		public void PlayStateChanged(PlayerStatus NewStatus) {  }
		public void SeekCompleted(PlayerStatus NewStatus) {  }
		public void VolumeChanged (double NewVolume) {
			UpdateSoundButtonState();
		}
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }
		#endregion
	}
}