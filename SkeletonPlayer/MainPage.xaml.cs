using System;
using System.Windows.Controls;

namespace SkeletonPlayer {
	public partial class MainPage : UserControl {
		private readonly HtmlInteraction bridge;
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
			bridge = new HtmlInteraction();
			bridge.AddBinding(Player);

			if (!String.IsNullOrEmpty(srcPlaylist)) {
				Player.LoadPlaylist(srcPlaylist);
			}
    
			//ControlBar.AddBinding(Player);
		}
	}
}
