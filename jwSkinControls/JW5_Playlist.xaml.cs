using System;
using System.Linq;
using System.Windows;
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
			ScrollSlider.TargetProportionChanged += ScrollSlider_TargetProportionChanged;
		}

		public void MuteChanged(bool IsMuted) { }

		public void StatusUpdate (PlayerStatus NewStatus) {
			ScrollSlider.TotalHeight = PlaylistItemStack.ActualHeight;
			ScrollSlider.VisibleHeight = LayoutRoot.ActualHeight;
			if (ScrollSlider.TotalHeight <= ScrollSlider.VisibleHeight) ScrollSlider.Visibility = Visibility.Collapsed;
			else ScrollSlider.Visibility = Visibility.Visible;
		}

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
				(child).SetSkin(skinPackage);
			}

			ScrollSlider.SetSkin(
				skinPackage.GetNamedElement(PlaylistComponent, "sliderRail"),
				skinPackage.GetNamedElement(PlaylistComponent, "sliderThumb"),
				skinPackage.GetNamedElement(PlaylistComponent, "sliderCapTop"),
				skinPackage.GetNamedElement(PlaylistComponent, "sliderCapBottom")
				);

			BackgroundImage.Source = skinPackage.GetNamedElement(PlaylistComponent, "background");

			BindActiveStates();
		}

		public void PlaylistChanged (IPlaylist NewPlaylist) {
			foreach (var child in PlaylistItemStack.Children.OfType<JwPlaylistItem>()) {
				child.Clicked -= button_Clicked;
			}
			PlaylistItemStack.Children.Clear();
			SetScroll(0);
			foreach (var item in NewPlaylist.Items) {
				var button = new JwPlaylistItem { PlaylistItem = item };
				button.Clicked +=button_Clicked;
				PlaylistItemStack.Children.Add(button);
			}
			BindSkins();
		}

		public void PlayingClipChanged (IPlaylistItem NewClip) {
			BindActiveStates();
		}
		public void PlayStateChanged (PlayerStatus NewStatus) {
			lastStatus = NewStatus;
			BindActiveStates();
		}
		public void SeekCompleted (PlayerStatus NewStatus) {
			lastStatus = NewStatus;
			BindActiveStates();
		}
		public void VolumeChanged(double NewVolume) {  }

		void button_Clicked(object sender, IndexEventArgs e) {
			players.EachPlayer(p => p.GoToPlaylistIndex(e.Index));
			players.EachPlayer(p => p.Play());
		}

		void ScrollSlider_TargetProportionChanged (object sender, ProportionEventArgs e) {
			var prop = e.Proportion;
			SetScroll(prop);
		}

		void SetScroll(double prop) {
			var toScroll = PlaylistItemStack.ActualHeight - LayoutRoot.ActualHeight;
			if (double.IsNaN(toScroll) || toScroll < 1) {
				PlaylistItemStack.Margin = new Thickness(0);
				return;
			}

			PlaylistItemStack.Margin = new Thickness(0, -(toScroll * prop), 0, 0);
		}
	}
}