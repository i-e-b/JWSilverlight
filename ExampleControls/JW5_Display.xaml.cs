using System;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class JW5_Display : UserControl, IPlayerController, IXmlSkinReader {
		private readonly RotateTransform rot;
		private readonly ComposerControlHelper helper;

		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public JW5_Display () {
			InitializeComponent();
			helper = new ComposerControlHelper();
			rot = new RotateTransform();
			rot.Angle = 15;
		}

		public void StateChanged (PlayerStatus NewStatus) {
			StatusUpdate(NewStatus);
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
			// todo: work out the rotation speed of (bufferroatation / bufferinterval) and do the actual rotation by real time.
			/*PlayingGlyph.Visibility = Visibility.Collapsed;
			PausedGlyph.Visibility = Visibility.Collapsed;
			WorkingGlyph.Visibility = Visibility.Collapsed;

			switch (NewStatus.CurrentPlayState) {
				case MediaElementState.Playing:
					PlayingGlyph.Visibility = Visibility.Visible;
					break;

				case MediaElementState.Paused:
				case MediaElementState.Closed:
				case MediaElementState.Stopped:
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
			}*/
		}
		public void AddBinding (IPlayer PlayerToControl) {
			helper.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			helper.RemoveBinding(PlayerToControl, this);
		}

		public void SetSkin(string xmlContents) {  }
	}
}