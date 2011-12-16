using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComposerCore;

namespace ExampleControls {
	public partial class Slider : UserControl, IPlayerController {
		private readonly ComposerControlHelper helper;
		private bool isDragging;
		private TimeSpan baseTime;

		public Slider () {
			InitializeComponent();
			helper = new ComposerControlHelper();
			isDragging = false;
			baseTime = TimeSpan.Zero;
		}


		public void PlaylistChanged (Playlist NewPlaylist) {PositionButton.Visibility = Visibility.Collapsed;}

		public void StateChanged (PlayerStatus NewStatus) {
			baseTime = TimeSpan.Zero;
			try {
				var props = helper.PlayerList[0].CurrentItem.CustomProperties;
				if (props.ContainsKey("BaseTime")) {
					baseTime = DateTime.Parse(props["BaseTime"]).TimeOfDay;
				}
			} catch {}
		}

		public void StatusUpdate (PlayerStatus NewStatus) {
			PositionButton.Visibility = Visibility.Visible;

			if (!isDragging) {
				double prop = GetProportion(NewStatus);
				PositionButtonByProportion(prop);
				try {
					SeekGuess.Text = NewStatus.PlayTime.Add(baseTime).ToString().Substring(0, 8);
				} catch { }
			}
		}

		private double GetProportion (PlayerStatus NewStatus) {
			TimeSpan min = NewStatus.ClipStart;
			TimeSpan max = NewStatus.ClipEnd;
			if (NewStatus.NaturalDuration.HasTimeSpan) {
				if (max <= TimeSpan.Zero || max < min) max = NewStatus.NaturalDuration.TimeSpan;
			}

			double range = max.TotalSeconds - min.TotalSeconds;

			double loc = NewStatus.PlayTime.TotalSeconds - min.TotalSeconds;
			if (loc <= 0.0) return 0.0;

			if (range <= 0.0) return 0.0;
			if (loc > range) return 1.0;
			
			return loc / range;
		}

		public void CaptionFired (TimelineMarker Caption) {}
		public void ErrorOccured (Exception Error) {}

		public void AddBinding (IPlayer PlayerToControl) {
			helper.AddBinding(PlayerToControl, this);
			if (helper.PlayerList.Count > 1) throw new Exception("Slider control can't keep track of more than one player!");
		}
		public void RemoveBinding (IPlayer PlayerToControl) {helper.RemoveBinding(PlayerToControl, this);}

		private void PositionButtonByProportion (double Prop) {
			double max = (ActualWidth - PositionButton.ActualWidth);
			double pbp = (max) * Prop;

			pbp = Math.Max(0, Math.Min(max, pbp));

			PositionButton.Margin = new Thickness(pbp, 0, 0, 0); // position the button!
			SeekGuessBubble.Margin = PositionButton.Margin;
		}
		
		private void Button_MouseLeftButtonDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
			isDragging = PositionButton.CaptureMouse();
		}

		private void PositionButton_MouseLeftButtonUp (object sender, System.Windows.Input.MouseButtonEventArgs e) {
			isDragging = false;
			PositionButton.ReleaseMouseCapture();
			SeekGuessBubble.Visibility = Visibility.Collapsed;
		}

		private void Button_MouseMove (object sender, System.Windows.Input.MouseEventArgs e) {
			if (!isDragging) return;

			if (PositionBar.ActualWidth <= 0.0) return; // can't do the math!

			var pos = e.GetPosition(PositionBar);
			double prop = Math.Min(PositionBar.ActualWidth, Math.Max(0, pos.X)) / PositionBar.ActualWidth;
			PositionButtonByProportion(prop);

			// do the real seek attempt (might need to delay this a bit!)
			foreach (var player in helper.PlayerList) {
				SeekGuess.Text = player.SeekTo(prop).Add(baseTime).ToString().Substring(0,8);
			}
		}

		private void PositionButton_MouseEnter (object sender, System.Windows.Input.MouseEventArgs e) {
			SeekGuessBubble.Visibility = Visibility.Visible;
		}

		private void PositionButton_MouseLeave (object sender, System.Windows.Input.MouseEventArgs e) {
			if (!isDragging) SeekGuessBubble.Visibility = Visibility.Collapsed;
		}

		
	}
}
