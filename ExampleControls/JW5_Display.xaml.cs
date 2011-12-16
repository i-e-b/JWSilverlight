using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ComposerCore;
using jwSkinLoader;

namespace ExampleControls {
	public partial class JW5_Display : UserControl, IPlayerController, IXmlSkinReader {
		private readonly RotateTransform rot;
		private readonly ComposerControlHelper players;

		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public JW5_Display () {
			InitializeComponent();
			players = new ComposerControlHelper();
			rot = new RotateTransform {Angle = 15};
		}

		public void StateChanged (PlayerStatus NewStatus) {
			StatusUpdate(NewStatus);
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
			// todo: work out the rotation speed of (bufferroatation / bufferinterval) and do the actual rotation by real time.
			Background.Visibility = Visibility.Collapsed;
			PlayIcon.Visibility = Visibility.Collapsed;
			PlayIconOver.Visibility =Visibility.Collapsed;
			MuteIcon.Visibility = Visibility.Collapsed;
			MuteIconOver.Visibility = Visibility.Collapsed;
			BufferIcon.Visibility = Visibility.Collapsed;

			switch (NewStatus.CurrentPlayState) {
				case MediaElementState.Playing:
					break;

				case MediaElementState.Paused:
				case MediaElementState.Closed:
				case MediaElementState.Stopped:
					PlayIcon.Visibility = Visibility.Visible;
					Background.Visibility = Visibility.Visible;
					break;

				default:
					BufferIcon.Visibility = Visibility.Visible;
					Background.Visibility = Visibility.Visible;
					break;
			}

			if (BufferIcon.Visibility == Visibility.Visible) {
				if (rot.Angle <= 355) {
					rot.Angle += 5;
				} else {
					rot.Angle = 0;
				}
				rot.CenterX = 24;
				rot.CenterY = 24;

				BufferIcon.RenderTransform = rot;
			}
		}
		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		public void SetSkin(JwSkinPackage pkg) {
			pkg.BindAndResize(Background, "display", "background");
			pkg.BindAndResize(PlayIcon, "display", "playIcon");
			pkg.BindAndResize(PlayIconOver, "display", "playIconOver");
			pkg.BindAndResize(MuteIcon, "display", "muteIcon");
			pkg.BindAndResize(MuteIconOver, "display", "muteIconOver");
			pkg.BindAndResize(BufferIcon, "display", "bufferIcon");
		}

		private void LayoutRoot_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (PlayIcon.Visibility == Visibility.Visible) {
				players.EachPlayer(p => p.Play());
			}
		}
		
	}
}