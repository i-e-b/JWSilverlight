using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace jwSkinControls {
	public partial class JW5_CaptionText : UserControl, IPlayerController {
		readonly ComposerControlHelper players;

		public JW5_CaptionText () {
			InitializeComponent();
			players = new ComposerControlHelper();
			LayoutRoot.SizeChanged += LayoutRoot_SizeChanged;
		}

		void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e) {
			CaptionBlock.FontSize = Math.Max(e.NewSize.Width / 33.3, 12.0);
		}

		public void CaptionFired(TimelineMarker Caption) {
			CaptionBlock.Text = Caption.Text;
		}

		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		public void PlaylistChanged(IPlaylist NewPlaylist) {}
		public void PlayingClipChanged(IPlaylistItem NewClip) {  }
		public void PlayStateChanged(PlayerStatus NewStatus) { }
		public void SeekCompleted(PlayerStatus NewStatus) { }
		public void VolumeChanged(double NewVolume) { }
		public void MuteChanged(bool IsMuted) { }
		public void StatusUpdate(PlayerStatus NewStatus) {}
		public void ErrorOccured (Exception Error) {}
	}
}
