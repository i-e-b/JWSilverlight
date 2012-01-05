using System;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace jwSkinControls.ControlFragments {
	public partial class JwElapsedText : UserControl, IPlayerController {
		private readonly ComposerControlHelper players;

		public JwElapsedText () {
			InitializeComponent();
			players = new ComposerControlHelper();
		}

		public new Brush Background {
			set {
				LayoutRoot.Background = value;
			}
		}

		public new double FontSize {
			set {
				ElapsedTime.FontSize = value;
			}
		}

		public Color FontColour {
			set {
				ElapsedTime.Foreground = new SolidColorBrush(value);
			}
		}

		public void PlaylistChanged(IPlaylist NewPlaylist) { }
		public void PlayingClipChanged(IPlaylistItem NewClip) {  }
		public void PlayStateChanged(PlayerStatus NewStatus) {  }
		public void SeekCompleted(PlayerStatus NewStatus) { }
		public void VolumeChanged(double NewVolume) {  }
		public void MuteChanged(bool IsMuted) {  }
		public void StatusUpdate(PlayerStatus NewStatus) {
			ElapsedTime.Text = NewStatus.PlayTime.Minutes.ToString("00")+":"+NewStatus.PlayTime.Seconds.ToString("00");
		}
		public void CaptionFired(TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public void AddBinding (IPlayer PlayerToControl) { players.AddBinding(PlayerToControl, this); } 
		public void RemoveBinding (IPlayer PlayerToControl) { players.RemoveBinding(PlayerToControl, this); }
	}
}
