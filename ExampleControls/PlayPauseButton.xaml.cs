using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;
using Microsoft.Web.Media.SmoothStreaming;

namespace ExampleControls {
	public partial class PlayPauseButton : UserControl, IPlayerController {
		private RotateTransform rot;
		private ComposerControlHelper helper;

		public PlayPauseButton ():base() {
			InitializeComponent();
			helper = new ComposerControlHelper();
			rot = new RotateTransform();
			rot.Angle = 5;
		}

		public void PlaylistChanged (Playlist NewPlaylist) {}

		public void StateChanged (PlayerStatus NewStatus) {
			StatusUpdate(NewStatus);
		}

		public void StatusUpdate (PlayerStatus NewStatus) {
			PlayingGlyph.Visibility = Visibility.Collapsed;
			PausedGlyph.Visibility = Visibility.Collapsed;
			WorkingGlyph.Visibility = Visibility.Collapsed;

			switch (NewStatus.CurrentPlayState) {
				case SmoothStreamingMediaElementState.Playing:
					PlayingGlyph.Visibility = Visibility.Visible;
					break;

				case SmoothStreamingMediaElementState.Paused:
				case SmoothStreamingMediaElementState.Closed:
				case SmoothStreamingMediaElementState.Stopped:
					PausedGlyph.Visibility = Visibility.Visible;
					break;

				default:
					WorkingGlyph.Visibility = Visibility.Visible;
					break;
			}

			if (WorkingGlyph.Visibility == Visibility.Visible) {
				if (rot.Angle <= 355) {
					rot.Angle += 5;
				} else {
					rot.Angle = 0;
				}
				rot.CenterX = 24;
				rot.CenterY = 24;

				WorkingGlyph.RenderTransform = rot;
			}
		}

		public void CaptionFired (TimelineMarker Caption) {
		}

		public void ErrorOccured (Exception Error) {
		}

		private void LayoutRoot_MouseLeftButtonUp (object sender, System.Windows.Input.MouseButtonEventArgs e) {
			foreach (var player in helper.PlayerList) {
				if (PlayingGlyph.Visibility == Visibility.Visible) {
					player.Pause();
				} else {
					player.Play();
				}
			}
		}


		public void AddBinding (IPlayer PlayerToControl) {
			helper.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			helper.RemoveBinding(PlayerToControl, this);
		}
	}
}
