using System;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class JW5_CaptionText : UserControl, IPlayerController {
		readonly ComposerControlHelper players;

		public JW5_CaptionText () {
			InitializeComponent();
			players = new ComposerControlHelper();
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

		public void PlaylistChanged(Playlist NewPlaylist) {}
		public void StateChanged(PlayerStatus NewStatus) {}
		public void StatusUpdate(PlayerStatus NewStatus) {}
		public void ErrorOccured (Exception Error) {}
	}
}
