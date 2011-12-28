﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ExampleControls;
using jwSkinLoader;

namespace JwslPlayer {
	public partial class MainPage : UserControl {
		readonly HtmlInteraction bridge;
		readonly JwSkinPackage jwSkinPackage;
		string srcPlaylist = "";
		const double ControlsFadeDelay = 3.0;
		volatile bool ControlsAreFaded;
		readonly OpacityFader controlBarFader, dockFader;

		public MainPage () {
			InitializeComponent();

			jwSkinPackage = new JwSkinPackage();
			jwSkinPackage.SkinReady += JwSkinPackageSkinPackageReady;

			bridge = new HtmlInteraction();
			bridge.AddBinding(Player);

			if (!String.IsNullOrEmpty(srcPlaylist)) {
				Player.LoadPlaylist(srcPlaylist);
			}

			ControlBarView.AddBinding(Player);
			DisplayView.AddBinding(Player);
			DockView.AddBinding(Player);
			PlaylistView.AddBinding(Player);
			CaptionView.AddBinding(Player);

			DockView.CaptionVisibilityChanged += DockView_CaptionVisibilityChanged;

			controlBarFader = new OpacityFader(ControlBarView);
			dockFader = new OpacityFader(DockView);
			SetFadeTimer();
		}

		void DockView_CaptionVisibilityChanged(object sender, ToggleVisibilityEventArgs e) {
			CaptionView.Visibility = e.Visibility;
			UpdateCaptionsMargin();
		}

		void UpdateCaptionsMargin() {
			if (!ControlsAreFaded) {
				CaptionView.Margin = new Thickness(0, 0, 0, ControlBarView.BarHeight);
			} else {
				CaptionView.Margin = new Thickness(0, 0, 0, 0);
			}
		}

		void JwSkinPackageSkinPackageReady(object sender, EventArgs e) {
			ControlBarView.SetSkin(jwSkinPackage);
			DisplayView.SetSkin(jwSkinPackage);
			DockView.SetSkin(jwSkinPackage);
			PlaylistView.SetSkin(jwSkinPackage);
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

			if (!Player.IsPlayerActive())
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

		private void LayoutRoot_MouseEnter (object sender, System.Windows.Input.MouseEventArgs e) {
			UnfadeControls();
			fadeTimer.Start();
		}

		private void LayoutRoot_MouseLeave (object sender, System.Windows.Input.MouseEventArgs e) {
			FadeControls();
			fadeTimer.Stop();
		}

		private void LayoutRoot_MouseMove (object sender, System.Windows.Input.MouseEventArgs e) {
			UnfadeControls();
			fadeTimer.Stop();
			fadeTimer.Start();
		}
	}
}