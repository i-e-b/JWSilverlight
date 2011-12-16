using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ComposerCore;
using jwSkinLoader;
using Microsoft.Web.Media.SmoothStreaming;

namespace ExampleControls {
	public partial class JW5_Display : UserControl, IPlayerController, IXmlSkinReader {
		private readonly RotateTransform rot;
		private double degreesPerMillisecond;
		private readonly ComposerControlHelper players;

		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void CaptionFired (TimelineMarker Caption) { }
		public void ErrorOccured (Exception Error) { }

		public JW5_Display () {
			InitializeComponent();
			players = new ComposerControlHelper();
			rot = new RotateTransform();
		}

		public void StateChanged (PlayerStatus NewStatus) {
			StatusUpdate(NewStatus);
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
			Background.Visibility = Visibility.Collapsed;
			PlayIcon.Visibility = Visibility.Collapsed;
			PlayIconOver.Visibility =Visibility.Collapsed;
			MuteIcon.Visibility = Visibility.Collapsed;
			MuteIconOver.Visibility = Visibility.Collapsed;
			BufferIcon.Visibility = Visibility.Collapsed;

			switch (NewStatus.CurrentPlayState) {
				case SmoothStreamingMediaElementState.Playing:
					break;

				case SmoothStreamingMediaElementState.Paused:
				case SmoothStreamingMediaElementState.Closed:
				case SmoothStreamingMediaElementState.Stopped:
					PlayIcon.Visibility = Visibility.Visible;
					Background.Visibility = Visibility.Visible;
					break;

				default:
					BufferIcon.Visibility = Visibility.Visible;
					Background.Visibility = Visibility.Visible;
					break;
			}

			if (BufferIcon.Visibility == Visibility.Visible) {
				rot.Angle = ((DateTime.Now - DateTime.Today).TotalMilliseconds * degreesPerMillisecond) % 360;
				// todo: add an animation to improve animation smoothness here.
				BufferIcon.RenderTransform = rot;
			}
		}
		public void AddBinding (IPlayer PlayerToControl) {
			players.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
			players.RemoveBinding(PlayerToControl, this);
		}

		public void SetSkin(JwSkinPackage pkg) {
			pkg.BindAndResize(Background, "display", "background");
			pkg.BindAndResize(PlayIcon, "display", "playIcon");
			pkg.BindAndResize(PlayIconOver, "display", "playIconOver");
			pkg.BindAndResize(MuteIcon, "display", "muteIcon");
			pkg.BindAndResize(MuteIconOver, "display", "muteIconOver");
			pkg.BindAndResize(BufferIcon, "display", "bufferIcon");

			var interval = pkg.GetSettingValue("display", "bufferinterval") ?? "100";
			var rotation = pkg.GetSettingValue("display", "bufferrotation") ?? "15";
			degreesPerMillisecond = double.Parse(rotation) / double.Parse(interval);
			
			rot.CenterX = BufferIcon.Width / 2.0;
			rot.CenterY = BufferIcon.Height / 2.0;
		}

		private void LayoutRoot_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (PlayIcon.Visibility == Visibility.Visible) {
				players.EachPlayer(p => p.Play());
			}
		}
		
	}
}