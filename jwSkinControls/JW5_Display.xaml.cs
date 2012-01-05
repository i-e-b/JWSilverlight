using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using ComposerCore;
using jwSkinControls.Animations;
using jwSkinLoader;
using Microsoft.Web.Media.SmoothStreaming;

namespace jwSkinControls {
	public partial class JW5_Display : UserControl, IPlayerController, IXmlSkinReader {
		private RotationAnimation spinAnimation;
		private readonly ComposerControlHelper players;
		const string componentName = "display";

		public void PlaylistChanged (IPlaylist NewPlaylist) { }
		public void PlayingClipChanged(IPlaylistItem NewClip) {  }
		public void PlayStateChanged(PlayerStatus NewStatus) { }
		public void SeekCompleted(PlayerStatus NewStatus) {}
		public void VolumeChanged(double NewVolume) { }
		public void MuteChanged(bool IsMuted) {  }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public JW5_Display () {
			InitializeComponent();
			players = new ComposerControlHelper();
		}

		public void StatusUpdate (PlayerStatus NewStatus) {
			BackgroundIcon.Visibility = Visibility.Collapsed;
			PlayIcon.Visibility = Visibility.Collapsed;
			MuteIcon.Visibility = Visibility.Collapsed;
			BufferIcon.Visibility = Visibility.Collapsed;

			switch (NewStatus.CurrentPlayState) {
				case SmoothStreamingMediaElementState.Playing:
					break;

				case SmoothStreamingMediaElementState.Paused:
				case SmoothStreamingMediaElementState.Closed:
				case SmoothStreamingMediaElementState.Stopped:
					PlayIcon.Visibility = Visibility.Visible;
					BackgroundIcon.Visibility = Visibility.Visible;
					break;
				
				default:
					BufferIcon.Visibility = Visibility.Visible;
					BackgroundIcon.Visibility = Visibility.Visible;
					break;
			}

			if (BufferIcon.Visibility == Visibility.Visible) {
				if(spinAnimation != null) spinAnimation.Start();
			} else {
				if (spinAnimation != null) spinAnimation.Stop();
			}
		}
		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		public void SetSkin(JwSkinPackage pkg) {
			pkg.BindAndResize(BackgroundIcon, componentName, "background");
			pkg.BindHoverButton(PlayIcon, componentName, "playIcon", "playIconOver");
			pkg.BindHoverButton(MuteIcon, componentName, "muteIcon", "muteIconOver");
			pkg.BindAndResize(BufferIcon, componentName, "bufferIcon");

			var interval = pkg.GetSettingValue(componentName, "bufferinterval") ?? "100";
			var rotation = pkg.GetSettingValue(componentName, "bufferrotation") ?? "15";

			spinAnimation = new RotationAnimation(BufferIcon, TimeSpan.FromMilliseconds(double.Parse(interval)), double.Parse(rotation));

			PlayIcon.Clicked += PlayIconClicked;
		}

		void PlayIconClicked(object sender, MouseButtonEventArgs e) { 
			if (PlayIcon.Visibility == Visibility.Visible) {
				players.EachPlayer(p => p.Play());
			}
		}
		
	}
}