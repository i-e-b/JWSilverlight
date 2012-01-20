using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ComposerCore;
using jwSkinControls.Animations;
using jwSkinControls.ControlFragments;
using jwSkinLoader;

namespace JwslPlayer {
	public partial class MainPage : UserControl, IPlayerController {
		readonly HtmlInteraction bridge;
		readonly JwSkinPackage jwSkinPackage;
		string srcPlaylist = "";
		const double ControlsFadeDelay = 3.0;
		volatile bool ControlsAreFaded;
		readonly OpacityFader controlBarFader, dockFader;
		readonly ComposerControlHelper players;

		public MainPage () {
			InitializeComponent();
			players = new ComposerControlHelper();

			jwSkinPackage = new JwSkinPackage();
			jwSkinPackage.SkinReady += JwSkinPackageSkinPackageReady;

			Player.MouseLeftButtonUp += Player_MouseLeftButtonUp;

			bridge = new HtmlInteraction(this);
			bridge.AddBinding(Player);

			if (!String.IsNullOrEmpty(srcPlaylist)) {
				Player.LoadPlaylist(srcPlaylist);
			}

			AddBinding(Player);
			ControlBarView.AddBinding(Player);
			DisplayView.AddBinding(Player);
			DockView.AddBinding(Player);
			PlaylistView.AddBinding(Player);
			CaptionView.AddBinding(Player);

			DockView.CaptionVisibilityChanged += DockView_CaptionVisibilityChanged;

			controlBarFader = new OpacityFader(ControlBarView);
			dockFader = new OpacityFader(DockView);
			SetFadeTimer();
			CaptionView.Margin = new Thickness(0, 0, 0, 75);// we don't know the height until images are all loaded... take a guess for now!
		}

		void Player_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			bridge.jwPlay(null);
		}

		void DockView_CaptionVisibilityChanged(object sender, ToggleVisibilityEventArgs e) {
			CaptionView.Visibility = e.Visibility;
			UpdateCaptionsMargin();
		}

		void UpdateCaptionsMargin() {
			if (!ControlsAreFaded && ControlBarView.BarHeight > 0) {
				CaptionView.Margin = new Thickness(0, 0, 0, ControlBarView.BarHeight);
			} else if (ControlBarView.BarHeight > 0) {
				CaptionView.Margin = new Thickness(0, 0, 0, 0);
			}
		}

		void JwSkinPackageSkinPackageReady(object sender, EventArgs e) {
			ControlBarView.SetSkin(jwSkinPackage);
			DisplayView.SetSkin(jwSkinPackage);
			DockView.SetSkin(jwSkinPackage);
			PlaylistView.SetSkin(jwSkinPackage);

			var color = (
				jwSkinPackage.GetSettingValue("display", "backgroundcolor")
				??
				jwSkinPackage.GetSettingValue("screencolor")
				??
				jwSkinPackage.GetSettingValue("backcolor")
				?? 
				"0x000000"
				).HexToColor();
			LayoutRoot.Background = new SolidColorBrush(color);
			Player.BackgroundColor = color;
			UpdateCaptionsMargin();
		}


		public string SourcePlaylist {
			set {
				if (Player != null) {
					Player.LoadPlaylist(value);
				} else {
					srcPlaylist = value;
				}
			}
		}

		public string SkinPackageUrl { set { jwSkinPackage.Load(value); } }

		public double PlaylistSize {
			get {
				return PlaylistView.ActualWidth;
			}
			set {
				PlaylistView.Width = value;
				if (value > 0) {
					PlaylistView.Visibility = Visibility.Visible;
				} else {
					PlaylistView.Visibility = Visibility.Collapsed;
				}
			}
		}

		public bool AutoPlay { get; set; }

		private DispatcherTimer fadeTimer;
		private void SetFadeTimer() {
			if (fadeTimer != null) return;
			fadeTimer = new DispatcherTimer();
			fadeTimer.Tick += FadeDispatcherTimerTick;
			fadeTimer.Interval = TimeSpan.FromSeconds(ControlsFadeDelay);
			fadeTimer.Start();
		}

		void FadeDispatcherTimerTick(object sender, EventArgs e) {
			if (Dispatcher.CheckAccess()) {
				FadeControls();
			} else {
				Dispatcher.BeginInvoke(FadeControls);
			}
		}

		void FadeControls() {
			if (ControlsAreFaded) return;

			if (!Player.IsActive())
				return;

			ControlsAreFaded = true;
			controlBarFader.Out(); dockFader.Out();
			UpdateCaptionsMargin();
		}

		void UnfadeControls () {
			if (!ControlsAreFaded) return;

			ControlsAreFaded = false;
			controlBarFader.In(); dockFader.In();
			UpdateCaptionsMargin();
		}

		private void LayoutRoot_MouseEnter (object sender, MouseEventArgs e) {
			UnfadeControls();
			fadeTimer.Start();
		}

		private void LayoutRoot_MouseLeave (object sender, MouseEventArgs e) {
			FadeControls();
			fadeTimer.Stop();
		}

		private void LayoutRoot_MouseMove (object sender, MouseEventArgs e) {
			UnfadeControls();
			fadeTimer.Stop();
			fadeTimer.Start();
		}

		public void PlaylistChanged(IPlaylist NewPlaylist) {
			if (AutoPlay) players.EachPlayer(p => p.Play());
		}
		public void PlayingClipChanged(IPlaylistItem NewClip) { }
		public void PlayStateChanged(PlayerStatus NewStatus) {  }
		public void SeekCompleted(PlayerStatus NewStatus) { }
		public void VolumeChanged(double NewVolume) { }
		public void MuteChanged(bool IsMuted) { }
		public void StatusUpdate(PlayerStatus NewStatus) { }
		public void CaptionFired(TimelineMarker Caption) { }
		public void ErrorOccured(Exception Error) { }
		public void AddBinding (IPlayer PlayerToControl) { players.AddBinding(PlayerToControl, this); }
		public void RemoveBinding (IPlayer PlayerToControl) { players.RemoveBinding(PlayerToControl, this); }
	}
}
