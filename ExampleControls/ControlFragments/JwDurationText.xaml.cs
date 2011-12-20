using System;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class JwDurationText : UserControl, IPlayerController {
		private readonly ComposerControlHelper players;

		public JwDurationText () {
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
				DurationTime.FontSize = value;
			}
		}

		public Color FontColour {
			set {
				DurationTime.Foreground = new SolidColorBrush(value);
			}
		}

		public void PlaylistChanged(Playlist NewPlaylist) { }
		public void StateChanged(PlayerStatus NewStatus) { }
		public void StatusUpdate(PlayerStatus NewStatus) {
			DurationTime.Text = NewStatus.ClipDuration.TimeSpan.Minutes.ToString("00") + ":" + NewStatus.ClipDuration.TimeSpan.Seconds.ToString("00");
		}
		public void CaptionFired(TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public void AddBinding (IPlayer PlayerToControl) { players.AddBinding(PlayerToControl, this); } 
		public void RemoveBinding (IPlayer PlayerToControl) { players.RemoveBinding(PlayerToControl, this); }
	}
}
