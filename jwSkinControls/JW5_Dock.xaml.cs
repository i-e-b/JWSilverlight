using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComposerCore;
using jwSkinLoader;

namespace ExampleControls {
	/// <summary>
	/// The Silverlight player doesn't support Flash plugins, and provides no
	/// equivalent plugin system at the moment.
	/// The dock will check for Captions in the current playlist clip, and hide or
	/// show a captions tile. It also handles hiding or showing
	/// </summary>
	public partial class JW5_Dock : UserControl, IPlayerController, IXmlSkinReader {
		Playlist lastPlaylist;

		public void StatusUpdate (PlayerStatus NewStatus) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }
		public void AddBinding (IPlayer PlayerToControl) { }
		public void RemoveBinding (IPlayer PlayerToControl) { }

		public JW5_Dock () {
			InitializeComponent();

			// todo: add caption visibility event, fire with hover button
			CaptionsButton.Visibility = Visibility.Visible;
			CaptionsButton.Clicked += CaptionsButton_Clicked;
		}

		void CaptionsButton_Clicked (object sender, MouseButtonEventArgs e) {
			CaptionsButton.CaptionText = "Subtitles\r\nOn";
		}

		public void PlaylistChanged (Playlist NewPlaylist) { lastPlaylist = NewPlaylist; }
		public void StateChanged (PlayerStatus NewStatus) {
			CaptionsButton.Visibility = HasCaptions(NewStatus) ? Visibility.Visible : Visibility.Collapsed;
		}

		bool HasCaptions(PlayerStatus NewStatus) {
			if (lastPlaylist == null) return false;
			return lastPlaylist.Items[NewStatus.PlaylistItemIndex].CaptionItems.Count > 0;
		}

		public void SetSkin(JwSkinPackage pkg) {
			pkg.BindHoverButton(CaptionsButton, "dock", "button", "buttonOver");
			CaptionsButton.BadgeImage = pkg.GetNamedElement("captions", "dockIcon");
			CaptionsButton.CaptionText = "Subtitles\r\nOff";
			CaptionsButton.CaptionColor = pkg.GetSettingValue("dock", "fontcolor").HexToColor();
		}
	}
}