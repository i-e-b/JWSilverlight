using System;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class JwElapsedText : UserControl, IPlayerController {
		private readonly ComposerControlHelper players;

		public JwElapsedText () {
			InitializeComponent();
			players = new ComposerControlHelper();
		}

		public new Brush Background {
			set {
				LayoutRoot.Background = value;
			}
		}

		public new double FontSize {
			set {
				ElapsedTime.FontSize = value;
			}
		}

		public Color FontColour {
			set {
				ElapsedTime.Foreground = new SolidColorBrush(value);
			}
		}

		public void PlaylistChanged(Playlist NewPlaylist) { }
		public void StateChanged(PlayerStatus NewStatus) { }
		public void StatusUpdate(PlayerStatus NewStatus) {
			ElapsedTime.Text = NewStatus.PlayTime.Minutes.ToString("00")+":"+NewStatus.PlayTime.Seconds.ToString("00");
		}
		public void CaptionFired(TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public void AddBinding (IPlayer PlayerToControl) { players.AddBinding(PlayerToControl, this); } 
		public void RemoveBinding (IPlayer PlayerToControl) { players.RemoveBinding(PlayerToControl, this); }
	}
}
