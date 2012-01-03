using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;
using jwSkinControls.ControlFragments;
using jwSkinLoader;

namespace jwSkinControls {
	public partial class JW5_Playlist : UserControl, IPlayerController, IXmlSkinReader {
		JwSkinPackage skinPackage;
		const string PlaylistComponent = "playlist";
		readonly ComposerControlHelper players;
		PlayerStatus lastStatus;

		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public JW5_Playlist () {
			InitializeComponent();
			players = new ComposerControlHelper();
		}

		public void StateChanged (PlayerStatus NewStatus) {
			lastStatus = NewStatus;
			BindActiveStates();
		}
		public void StatusUpdate (PlayerStatus NewStatus) { }

		void BindActiveStates() {
			foreach (var child in PlaylistItemStack.Children.OfType<JwPlaylistItem>()) {
				child.Active = (child.PlaylistItem.PlaylistIndex == lastStatus.PlaylistItemIndex);
			}
		}

		public void SetSkin(JwSkinPackage pkg) {
			skinPackage = pkg;
			BindSkins();
		}

		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		void BindSkins() {
			if (skinPackage == null) return;
			foreach (var child in PlaylistItemStack.Children.OfType<JwPlaylistItem>()) {
				(child).SetSkin(
				                skinPackage.GetNamedElement(PlaylistComponent, "item"),
				                skinPackage.GetNamedElement(PlaylistComponent, "itemActive"),
				                skinPackage.GetNamedElement(PlaylistComponent, "itemOver"),
				                skinPackage.GetNamedElement(PlaylistComponent, "itemImage")
					);
			}
			BindActiveStates();
		}

		public void PlaylistChanged (IPlaylist NewPlaylist) {
			foreach (var child in PlaylistItemStack.Children.OfType<JwPlaylistItem>()) {
				child.Clicked -= button_Clicked;
			}
			foreach (var item in NewPlaylist.Items) {
				var button = new JwPlaylistItem { PlaylistItem = item };
				button.Clicked +=button_Clicked;
				PlaylistItemStack.Children.Add(button);
			}
			BindSkins();
		}

		void button_Clicked(object sender, IndexEventArgs e) {
			players.EachPlayer(p => p.GoToPlaylistIndex(e.Index));
			players.EachPlayer(p => p.Play());
		}
	}
}