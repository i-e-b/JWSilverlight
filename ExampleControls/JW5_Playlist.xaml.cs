using System;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class JW5_Playlist : UserControl, IPlayerController, IXmlSkinReader {
		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void StateChanged (PlayerStatus NewStatus) { }
		public void StatusUpdate (PlayerStatus NewStatus) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }
		public void AddBinding (IPlayer PlayerToControl) { }
		public void RemoveBinding (IPlayer PlayerToControl) { }

		public JW5_Playlist () {
			InitializeComponent();
		}

		public void SetSkin(string xmlContents) {  }
	}
}