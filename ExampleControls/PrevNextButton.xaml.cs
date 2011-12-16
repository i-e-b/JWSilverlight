using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class PrevNextButton : UserControl, IPlayerController {
		private ComposerControlHelper helper;
		private int expectedPlaylistCount;

		public PrevNextButton ():base() {
			InitializeComponent();
			helper = new ComposerControlHelper();
			expectedPlaylistCount = 0;
		}

		public void PlaylistChanged (Playlist NewPlaylist) {
			expectedPlaylistCount = NewPlaylist.Items.Count;
		}

		public void StateChanged (PlayerStatus NewStatus) {}

		public void StatusUpdate (PlayerStatus NewStatus) {
			PrevButtom.Visibility = Visibility.Visible;
			NextButton.Visibility = Visibility.Visible;
			if (NewStatus.PlaylistItemIndex <= 0) {
				PrevButtom.Visibility = Visibility.Collapsed;
			}
			if (NewStatus.PlaylistItemIndex >= expectedPlaylistCount - 1) {
				NextButton.Visibility = Visibility.Collapsed;
			}
		}

		public void CaptionFired (TimelineMarker Caption) {}

		public void ErrorOccured (Exception Error) {}

		public void AddBinding (IPlayer PlayerToControl) {
			helper.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			helper.RemoveBinding(PlayerToControl, this);
		}

		private void PrevButtom_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			foreach (var player in helper.PlayerList) {
				player.GoToPlaylistIndex(player.CurrentIndex - 1);
			}
		}

		private void NextButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			foreach (var player in helper.PlayerList) {
				player.GoToPlaylistIndex(player.CurrentIndex + 1);
			}
		}
	}
}
