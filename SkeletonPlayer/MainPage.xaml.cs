﻿using System;
using System.Windows.Controls;
using jwSkinLoader;

namespace JwslPlayer {
	public partial class MainPage : UserControl {
		private readonly HtmlInteraction bridge;
		private readonly JwSkinPackage jwSkinPackage;
		private string srcPlaylist = "";

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

			DebugInfo.Text += "\r\nControls bound to player";
		}

		void JwSkinPackageSkinPackageReady(object sender, EventArgs e) {
			DebugInfo.Text += "\r\nSkin loaded:\r\n" +jwSkinPackage.AllFiles;
			//ControlBarView.SetSkin(
		}
	}
}
