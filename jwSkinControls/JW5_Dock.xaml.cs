using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComposerCore;
using jwSkinControls.ControlFragments;
using jwSkinLoader;

namespace jwSkinControls {
	/// <summary>
	/// The Silverlight player doesn't support Flash plugins, and provides no
	/// equivalent plugin system at the moment.
	/// The dock will check for Captions in the current playlist clip, and hide or
	/// show a captions tile. It also handles hiding or showing
	/// </summary>
	public partial class JW5_Dock : UserControl, IPlayerController, IXmlSkinReader {
		IPlaylist lastPlaylist;
		bool showCaptions;
		readonly ComposerControlHelper players;

		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public event EventHandler<ToggleVisibilityEventArgs> CaptionVisibilityChanged;

		public JW5_Dock () {
			InitializeComponent();
			customButtons = new Dictionary<string, ImageHoverButton>();
			players = new ComposerControlHelper();
			CaptionsButton.Clicked += CaptionsButtonClicked;
		}

		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		void CaptionsButtonClicked (object sender, MouseButtonEventArgs e) {
			showCaptions = !showCaptions;
			CaptionsButton.CaptionText = "Subtitles\r\n" + (showCaptions ? "On" : "Off");
			InvokeCaptionVisibilityChanged(showCaptions);
		}

		public void InvokeCaptionVisibilityChanged(bool visible) {
			var handler = CaptionVisibilityChanged;
			if (handler != null) handler(this, new ToggleVisibilityEventArgs {IsVisible = visible});
		}

		public void PlaylistChanged (IPlaylist NewPlaylist) { 
			lastPlaylist = NewPlaylist;
		}

		public void PlayingClipChanged(IPlaylistItem NewClip) {  }
		public void PlayStateChanged(PlayerStatus NewStatus) {  }
		public void SeekCompleted(PlayerStatus NewStatus) { }
		public void VolumeChanged(double NewVolume) { }
		public void MuteChanged(bool IsMuted) {  }

		public void StatusUpdate (PlayerStatus NewStatus) {
			CaptionsButton.Visibility = HasCaptions(NewStatus) ? Visibility.Visible : Visibility.Collapsed;
		}

		bool HasCaptions(PlayerStatus NewStatus) {
			if (lastPlaylist == null) return false;
			if (lastPlaylist.Items[NewStatus.PlaylistItemIndex].CaptionItems == null) return false;
			return lastPlaylist.Items[NewStatus.PlaylistItemIndex].CaptionItems.Count > 0;
		}

		public void SetSkin(JwSkinPackage pkg) {
			skinPackage = pkg;

			pkg.BindHoverButton(CaptionsButton, "dock", "button", "buttonOver");
			CaptionsButton.BadgeImage = pkg.GetNamedElement("captions", "dockIcon");
			CaptionsButton.CaptionText = "Subtitles\r\nOn";
			CaptionsButton.CaptionColor = (pkg.GetSettingValue("dock", "fontcolor") ?? "0xffffff").HexToColor();
		}

		readonly Dictionary<string, ImageHoverButton> customButtons;
		JwSkinPackage skinPackage;

		public void SetCustomButton (string id, Action callback, string outGraphic, string overGraphic) {
			if (string.IsNullOrEmpty(outGraphic)) return;
			if (!customButtons.ContainsKey(id)) {
				customButtons.Add(id, new ImageHoverButton());
				LayoutRoot.Children.Add(customButtons[id]);
			}
			var button = customButtons[id];

			button.ClearEvents();
			button.Clicked += (s, e) => callback();
			skinPackage.BindHoverButton(button, "dock", "button", "buttonOver");
			button.BadgeImage = new BitmapImage(new Uri(outGraphic, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage());
			if (!string.IsNullOrEmpty(overGraphic)) {
				button.BadgeImageOver = new BitmapImage(new Uri(overGraphic, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage());
			}
		}

		public void RemoveCustomButton(string id) {
			if (!customButtons.ContainsKey(id)) return;
			customButtons[id].ClearEvents();
			LayoutRoot.Children.Remove(customButtons[id]);
			customButtons.Remove(id);
		}
	}
}