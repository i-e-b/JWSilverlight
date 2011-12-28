﻿using System;
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
		bool showCaptions;
		readonly ComposerControlHelper players;

		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public event EventHandler<ToggleVisibilityEventArgs> CaptionVisibilityChanged;

		public JW5_Dock () {
			InitializeComponent();

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
			if (handler != null) handler(this, new ToggleVisibilityEventArgs {isVisible = visible});
		}

		public void PlaylistChanged (Playlist NewPlaylist) { lastPlaylist = NewPlaylist; }
		public void StatusUpdate (PlayerStatus NewStatus) {
			CaptionsButton.Visibility = HasCaptions(NewStatus) ? Visibility.Visible : Visibility.Collapsed;
		}
		public void StateChanged (PlayerStatus NewStatus) {
			StatusUpdate(NewStatus);
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