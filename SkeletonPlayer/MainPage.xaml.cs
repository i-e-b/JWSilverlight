using System;
using System.Windows.Controls;

namespace SkeletonPlayer {
	public partial class MainPage : UserControl {

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

		public MainPage () {
			InitializeComponent();

			if (!String.IsNullOrEmpty(srcPlaylist)) {
				Player.LoadPlaylist(srcPlaylist);
			}
    
			//ControlBar.AddBinding(Player);
		}
	}
}
