using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ComposerCore.InnerWorkings;
using Microsoft.Web.Media.SmoothStreaming;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Browser;

namespace ComposerCore {

	public class Player : Canvas, IPlayer {
		#region Status properties
		/// <summary>
		/// Get a containing element for the player.
		/// </summary>
		public UIElement GetLayoutContainer { get { return this; } }

		/// <summary>
		/// Get the current playlist. Returns a blank playlist if none loaded.
		/// </summary>
		public IPlaylist CurrentPlaylist { get; private set; }

		/// <summary>
		/// Get the playlist item currently being played, if any.
		/// </summary>
		public IPlaylistItem CurrentItem { get; private set; }

		Color backgroundColor = Color.FromArgb(0, 0, 0, 0);
		public Color BackgroundColor {
			set {
				MediaPlayer.Background = new SolidColorBrush(value);
				PosterDisplay.Fill = new SolidColorBrush(value);
				backgroundColor = value;
			}
			get { return backgroundColor; }
		}

		/// <summary>
		/// Index of the current playlist item.
		/// If there is not current item, this will be invalid.
		/// </summary>
		public int CurrentIndex { get; set; }

		/// <summary>
		/// Gets the player's current status.
		/// This is updated by a timer, so may not be frame-accurate.
		/// </summary>
		public PlayerStatus Status { get; private set; }

		public TimeSpan CurrentSliderPosition { get; set; }

		public bool Mute {
			get { return MediaPlayer.IsMuted; }
			set {
				MediaPlayer.IsMuted = value;
				SendToControls(c => c.MuteChanged(value));
			}
		}

		/// <summary>
		/// Gets or Sets the player's audio volume. Range is 0..1
		/// </summary>
		public double AudioVolume {
			get { return MediaPlayer.Volume; }
			set {
				MediaPlayer.Volume = Math.Min(1.0, Math.Max(0.0, value));
				SendToControls(c => c.VolumeChanged(MediaPlayer.Volume));
			}
		}

		private bool SmoothstreamBeginVariablesSet { get; set; }



		/// <summary>
		/// Gets; Returns true if media is seekable, false for non-seekable (WME live, un-indexed VOD etc)
		/// </summary>
		public bool CanSeek {
			get {
				if (MediaPlayer == null) return false;

				if (MediaPlayer.SmoothStreamingSource != null) return MediaPlayer.CanSeek;

				// WME Live and live-archive don't report properly in MediaElement. Hack:
				if (MediaPlayer.NaturalDuration == TimeSpan.Zero
					&& MediaPlayer.Position > TimeSpan.Zero)
					return false;
				return MediaPlayer.CanSeek;
			}
		}

		private bool IsLive {
			get {
				if (MediaPlayer.SmoothStreamingSource != null) return MediaPlayer.IsLive;

				try {
					if (MediaPlayer.NaturalDuration == TimeSpan.Zero
						&& MediaPlayer.Position > TimeSpan.Zero)
						return true;
				} catch (InvalidOperationException) {
					if (MediaPlayer.Position > TimeSpan.Zero) return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets; Returns true if media can be paused and resumed, false otherwise (WME live etc)
		/// </summary>
		public bool CanPause {
			get {
				if (MediaPlayer == null) return false;

				if (MediaPlayer.SmoothStreamingSource != null) return MediaPlayer.CanPause;

				// WME Live and live-archive don't report properly in MediaElement. Hack:
				if (MediaPlayer.NaturalDuration == TimeSpan.Zero
					&& MediaPlayer.Position > TimeSpan.Zero) return false;
				return MediaPlayer.CanPause;
			}
		}

		#endregion

		#region Player Controls

		/// <summary>
		/// Gets or Sets; Default = false.
		/// If true, audio will not be played -- and may not be loaded.
		/// </summary>
		public bool PictureInPicture {
			get { return MediaPlayer.PipMode; }
			set { MediaPlayer.PipMode = value; }
		}

		/// <summary>
		/// Display the thumbnail for the given playlist item index.
		/// Causes playback to be paused. Sets current playlist position to 'Index'
		/// </summary>
		public void DisplayPoster (int Index) {
			Pause();
			if (CurrentPlaylist.Items.Count < 1) return;

			if (Index < 0 || Index >= CurrentPlaylist.Items.Count) {
				CurrentItem = CurrentPlaylist.Items[0];
				ShowThumbnail(CurrentItem.ThumbSource);
			} else {
				CurrentItem = CurrentPlaylist.Items[Index];
				ShowThumbnail(CurrentItem.ThumbSource);
			}

			PosterMode = CurrentItem.ThumbDuration > 0.0;
		}

		/// <summary>
		/// Continue playing at the given playlist item index.
		/// If player is currently paused, play will resume if "AutoPlay" is set to true.
		/// If 'Index' is out of the range of the current playlist, the position be wrapped.
		/// </summary>
		public void GoToPlaylistIndex (int Index) {
			if (InPosterDisplay() && Index > CurrentIndex) return; // can't leave poster yet.

			if (CurrentPlaylist.Items.Count < 1) {
				CurrentIndex = 0;
				Pause();
			} else if (Index < 0) {
				if (CurrentIndex != 0) {
					CurrentIndex = CurrentPlaylist.Items.Count - 1;
					CurrentItem = CurrentPlaylist.Items[CurrentIndex];
					SeekToItem();
				}
			} else if (Index >= CurrentPlaylist.Items.Count) {
				int end_item = CurrentPlaylist.Items.Count - 1;
				if (CurrentIndex != end_item) {
					CurrentIndex = end_item;
					CurrentItem = CurrentPlaylist.Items[CurrentIndex];
					SeekToItem();
				} else {
					CurrentIndex = 0;
					CurrentItem = CurrentPlaylist.Items[CurrentIndex];
					SeekToItem();
					Pause();
				}
			} else {
				CurrentItem = CurrentPlaylist.Items[Index];
				CurrentIndex = Index;
				SeekToItem();
			}

			SendToControls(c => c.PlayingClipChanged(CurrentItem));
		}

		void TryPlay () {
			try { MediaPlayer.Play(); } catch (Exception ex) { drop(ex); }
		}
		void TryPause () {
			try { MediaPlayer.Pause(); } catch (Exception ex) { drop(ex); }
		}

		/// <summary>
		/// Stops playback without changing position, regardless of current play-state
		/// </summary>
		public void Pause () {
			WantToPlay = false;
			InPlayMode = false;
			switch (MediaPlayer.CurrentState) {
				case SmoothStreamingMediaElementState.Closed:
				case SmoothStreamingMediaElementState.Stopped:
					TryPlay(); // load the player state again.
					break;
			}
			if (MediaPlayer.CurrentState == SmoothStreamingMediaElementState.Playing) {
				ResumeTime = MediaPlayer.Position;
			}
			TryPause();
		}

		/// <summary>
		/// Continues playback without changing position, regardless of current play-state
		/// </summary>
		public void Play () {
			if (MediaPlayer.CurrentState == SmoothStreamingMediaElementState.Playing) {
				return;
			}
			ForcePlay();
		}

		/// <summary>
		/// Restarts playback from last resume time.
		/// </summary>
		private void ForcePlay () {
			if (MediaPlayer.CurrentState == SmoothStreamingMediaElementState.Closed) {
				PreparePlayer(); // Something about the MediaPlayer breaks when it enters 'Closed' mode.
				GoToPlaylistIndex(CurrentIndex);
			}

			MediaPlayer.Visibility = Visibility.Visible; // ensure we're visible.
			// ReSharper disable RedundantCheckBeforeAssignment
			try {
				if (MediaPlayer.Position != ResumeTime) MediaPlayer.Position = ResumeTime;
			} catch (Exception ex) { drop(ex); }
			// ReSharper restore RedundantCheckBeforeAssignment
			WantToPlay = true;
			TryPlay();
		}

		/// <summary>
		/// Try to seek to live point. Does nothing if not live or not seekable.
		/// </summary>
		public void GoToLive () {
			if (!MediaPlayer.IsLive) return;

			MediaPlayer.FlushBuffers(TimeSpan.Zero, false, false);
			MediaPlayer.LivePlaybackStartPosition = PlaybackStartPosition.End;

			// Try some nasty cache-defeating hacks:
			if (MediaPlayer.SmoothStreamingSource != null) {
				var rnd = (new Random()).Next(100, 999).ToString();
				MediaPlayer.SmoothStreamingSource = new Uri(MediaPlayer.SmoothStreamingSource, "MANIFEST?v=" + rnd);
			}

			MediaPlayer.StartSeekToLive();
		}

		/// <summary>
		/// Try to move the playback position to an offset from the beginning of the clip.
		/// Seek will not continue past the start or end of the current item's media.
		/// Returns a guess at play-head time that will result.
		/// </summary>
		/// <remarks>If the playlist item has an 'IN' point and you wish to seek before this,
		/// Pass a negative value for offset (up to -IN). Seeking past the 'OUT' point is
		/// not supported</remarks>
		public TimeSpan SeekTo (TimeSpan Offset) {
			if (!MediaPlayer.CanSeek) return TimeSpan.Zero;
			if (PosterMode) return TimeSpan.Zero; // no seeking in poster mode!
			ResumeTime = Offset.Add(TimeSpan.FromSeconds(CurrentItem.StartPosition));
			ShouldSeek = true;
			return ResumeTime;
		}


		/// <summary>
		/// Try to move the playback position to an proportion of the clip.
		/// Returns a best-guess at the actual position set.
		/// Seek will not continue past the start or end of the current item's media.
		/// </summary>
		/// <param name="Proportion">Value 0..1 of clip length</param>
		public TimeSpan SeekTo (double Proportion) {
			Proportion = Math.Min(1.0, Math.Max(0.0, Proportion)); // pin range

			if (IsLive) {
				if (!CurrentItem.IsAdaptiveStreaming) return Status.PlayTime;

				TimeSpan base_start = Status.ClipStart;
				ResumeTime = TimeSpan.FromSeconds(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds * Proportion).Add(base_start);

				ShouldSeek = true;
				return ResumeTime;
			}
			TimeSpan est = TimeSpan.FromSeconds(ClipDuration().TotalSeconds * Proportion) + MediaPlayer.StartPosition;
			return SeekTo(est);
		}

		/// <summary>
		/// Try to move the playback position to an offset from the current play position.
		/// Both positive and negative values are valid.
		/// Seek may go past the beginning or end of a clip in or out point.
		/// Seek will not continue past the start or end of the current item's media.
		/// Returns a guess at play-head time that will result.
		/// </summary>
		public TimeSpan SeekRelativeUnclipped (TimeSpan Offset) {
			if (!MediaPlayer.CanSeek) return TimeSpan.Zero;
			if (PosterMode) return TimeSpan.Zero; // no seeking in poster mode!

			ResumeTime = MediaPlayer.Position.Add(Offset);
			ShouldSeek = true;
			return ResumeTime;
		}


		/// <summary>
		/// Try to move the playback position to an offset from the current play position.
		/// Both positive and negative values are valid.
		/// Seek may not go past the beginning or end of a clip; nor past the start or end of the current item's media.
		/// Returns a guess at play-head time that will result.
		/// </summary>
		public TimeSpan SeekRelative (TimeSpan Offset) {
			if (!MediaPlayer.CanSeek) return TimeSpan.Zero;
			if (PosterMode) return TimeSpan.Zero; // no seeking in poster mode!

			ResumeTime = PinToClip(MediaPlayer.Position.Add(Offset));
			ShouldSeek = true;
			return ResumeTime;
		}

		/// <summary>
		/// Return the input time span, pinned to within the
		/// clip range (if there is one) or the entire media duration (if no clip).
		/// </summary>
		private TimeSpan PinToClip (TimeSpan Time) {
			double val = Time.TotalSeconds;

			double min = Math.Max(0, CurrentItem.StartPosition);
			double max = min + ClipDuration().TotalSeconds;

			return TimeSpan.FromSeconds(Math.Max(min, Math.Min(max, val)));
		}

		/// <summary>
		/// Returns true if the Media Player is active, false otherwise.
		/// Buffering is considered active.
		/// </summary>
		public bool IsActive () {
			switch (MediaPlayer.CurrentState) {
				case SmoothStreamingMediaElementState.AcquiringLicense:
				case SmoothStreamingMediaElementState.Buffering:
				case SmoothStreamingMediaElementState.Individualizing:
				case SmoothStreamingMediaElementState.Opening:
				case SmoothStreamingMediaElementState.Playing:
					return true;

				default:
					return false;
			}
		}

		#endregion

		#region IPlayerController connections

		public List<IPlayerController> ControlSets { get; set; }

		/// <summary>
		/// Add a controller to the list of those to be notified of events.
		/// </summary>
		/// <remarks>A controller doesn't have to be in the list to send commands.</remarks>
		public void AddController (IPlayerController Controller) {
			if (ControlSets.Contains(Controller)) return; // only once!
			var new_set = new List<IPlayerController>(ControlSets){Controller};
			ControlSets = new_set;
		}

		/// <summary>
		/// Remove a controller from the list of those to be notified of events.
		/// </summary>
		/// <remarks>A controller doesn't have to be in the list to send commands.</remarks>
		public void RemoveController (IPlayerController Controller) {
			if (!ControlSets.Contains(Controller)) return; // nothing to remove
			var new_set = new List<IPlayerController>(ControlSets);
			new_set.Remove(Controller);
			ControlSets = new_set;
		}

		#endregion

		/// <summary>
		/// Create a new blank player control.
		/// </summary>
		public Player () {
			CurrentPlaylist = new Playlist();
			Initialise();
			AddCloseTriggers();
		}

		/// <summary>
		/// Create a new player, preloaded with the given playlist
		/// </summary>
		/// <param name="PlayList">Playlist as either an XML string or a URL to an XML asset.</param>
		public Player (string PlayList) {
			CurrentPlaylist = new Playlist();
			CurrentPlaylist.PlaylistLoaded += CurrentPlaylist_PlaylistChanged;
			CurrentPlaylist.ReadPlaylist(PlayList);
		}


		/// <summary>
		/// Load a playlist from the string, and replace current playlist (if any)
		/// </summary>
		public void LoadPlaylist (string PlayList) {
			CurrentPlaylist = new Playlist();
			CurrentPlaylist.PlaylistLoaded += CurrentPlaylist_PlaylistChanged;
			CurrentPlaylist.ReadPlaylist(PlayList);
		}

		/// <summary>
		/// Load a playlist, and replace current playlist (if any)
		/// </summary>
		public void LoadPlaylist (IPlaylist p) {
			CurrentPlaylist = p;
			PreparePlaylist();
		}


		#region Inner Workings

		private ImageBrush Poster { get; set; }
		private SmoothStreamingMediaElement MediaPlayer { get; set; }
		private Rectangle PosterDisplay { get; set; }
		/// <summary>Time to seek to on Play(). Set by playlists and by Pause().</summary>
		private TimeSpan ResumeTime { get; set; }
		/// <summary>Should seek on next clock tick.</summary>
		private bool ShouldSeek { get; set; }
		private DispatcherTimer ActionTimer { get; set; }
		private DispatcherTimer CaptionTimer { get; set; }
		private DateTime PosterSetTime { get; set; }
		private bool PosterMode { get; set; }

		/// <summary>This is true whenever we've started playing -- it helps make AutoStart=false more intuitive</summary>
		private bool InPlayMode { get; set; }

		/// <summary>This set true when 'play' is called, and set 'false' when playing. Used to fix player element bugs.</summary>
		private bool WantToPlay { get; set; }

		/// <summary>
		/// Initial setup of the player.
		/// </summary>
		private void Initialise () {
			ControlSets = new List<IPlayerController>();

			CurrentIndex = -1;
			ResumeTime = TimeSpan.Zero;
			Poster = new ImageBrush();

			Background = Poster;
			PosterSetTime = DateTime.MinValue;
			PosterMode = false;

			PreparePlayer();
			SetupTimer();

			PosterDisplay = new Rectangle{
				Stretch = Stretch.Uniform, 
				HorizontalAlignment = HorizontalAlignment.Center, 
				VerticalAlignment = VerticalAlignment.Center,
				Fill = Poster
			};
			Children.Add(PosterDisplay);

			LayoutUpdated += Player_LayoutUpdated;
		}


		/// <summary>
		/// Sets up a few tricks to send events when the player or it's window are closed.
		/// </summary>
		private void AddCloseTriggers () {
			Application.Current.Exit += ExitHandler;
			if (!HtmlPage.IsEnabled) return; // no bridge -- can't inject JavaScript close event.
			if (String.IsNullOrEmpty(HtmlPage.Plugin.Id)) return; // need an ID to bind to the better window closing event.


			HtmlPage.RegisterScriptableObject("Bridge", this);
			HtmlPage.Window.Eval(
@"
window.onbeforeunload = function() {
	var slApp = document.getElementById('" + HtmlPage.Plugin.Id + @"');
	var result = slApp.Content.Bridge.OnBeforeUnload();
}
"
);
		}

		/// <summary>
		/// A JavaScript hook to catch window closing events earlier than Application.Current.Exit;
		/// </summary>
		/// <returns></returns>
		[ScriptableMember]
		public string OnBeforeUnload () {
			ExitHandler(null, null);
			return "";
		}

		/// <summary>
		/// Sends a 'stop' message to all connected controls.
		/// </summary>
		/// <remarks>At all other times, the play state should either  be 'paused' or 'closed'</remarks>
		protected void ExitHandler (object sender, EventArgs e) {
			if (Status.PlaylistItemIndex < 0 && Status.CurrentPlayState == SmoothStreamingMediaElementState.Stopped) return; // no need to signal.

			var new_status = new PlayerStatus{
				ClipEnd = TimeSpan.Zero,
				ClipStart = TimeSpan.Zero,
				CurrentPlayState = SmoothStreamingMediaElementState.Stopped,
				NaturalDuration = TimeSpan.Zero,
				PlaylistItemIndex = -1,
				PlayTime = TimeSpan.Zero,
				BufferingProgress = 0.0,
				NaturalStartPosition = TimeSpan.Zero,
				IsLive = false, 
				ClipDuration = TimeSpan.Zero,
				PlayProgress = 0.0
			};
			Status = new_status;
			SendToControls(c => c.PlayStateChanged(Status));
		}

		void Player_LayoutUpdated (object sender, EventArgs e) {
			MediaPlayer.Width = ActualWidth;
			MediaPlayer.Height = ActualHeight;
			MediaPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;
			MediaPlayer.VerticalAlignment = VerticalAlignment.Stretch;
			MediaPlayer.Margin = new Thickness(0.0);
		}

		/// <summary>
		/// Create a new player, removing the old one if needed.
		/// </summary>
		private void PreparePlayer () {
			if (MediaPlayer != null) {
				Children.Remove(MediaPlayer);
			}

			MediaPlayer = new SmoothStreamingMediaElement();
			MediaPlayer.Volume = 0.9;
			Children.Insert(0, MediaPlayer);

			MediaPlayer.CurrentStateChanged += MediaPlayer_StateChange;
			MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
			MediaPlayer.ClipStateChanged += MediaPlayer_PlaybackStateChange;
			MediaPlayer.MarkerReached += MediaPlayer_MarkerReached;
			MediaPlayer.BufferingProgressChanged += MediaPlayer_BufferingProgressChanged;
			MediaPlayer.ClipError += MediaPlayer_ClipError;
			MediaPlayer.SmoothStreamingErrorOccurred += MediaPlayer_SmoothStreamingErrorOccurred;


			if (CurrentPlaylist != null) {
				MediaPlayer.Stretch = CurrentPlaylist.StretchMode;
				MediaPlayer.CacheMode = (CurrentPlaylist.EnableCachedComposition && (MediaPlayer.CacheMode == null)) ? new BitmapCache() : null;
			} else {
				MediaPlayer.Stretch = Stretch.Uniform;
			}
		}

		void MediaPlayer_StateChange(object sender, RoutedEventArgs e) {
			RefreshStatus();
			SendToControls(c => c.PlayStateChanged(Status));
		}

		void CurrentPlaylist_PlaylistChanged (object sender, RoutedEventArgs e) {
			PreparePlaylist();
		}

		/// <summary>
		/// Cross-load playlist settings and start if AutoStart is set.
		/// </summary>
		private void PreparePlaylist () {
			Status = new PlayerStatus(); // reset status

			if (CurrentPlaylist == null) return;

			MediaPlayer.Stretch = CurrentPlaylist.StretchMode;

			Poster.Stretch = CurrentPlaylist.StretchMode;
			MediaPlayer.CacheMode = (CurrentPlaylist.EnableCachedComposition && (MediaPlayer.CacheMode == null)) ? new BitmapCache() : null;
			if (CurrentPlaylist.StartMuted) MediaPlayer.Volume = 0.0;

			if (CurrentPlaylist.Items.Count < 1) return;

			MediaPlayer.AutoPlay = CurrentPlaylist.AutoPlay;
			InPlayMode = CurrentPlaylist.AutoPlay;
			if (CurrentPlaylist.AutoLoad || CurrentPlaylist.AutoPlay) {
				GoToPlaylistIndex(0);
			} else {
				DisplayPoster(0);
				ResumeTime = TimeSpan.FromSeconds(Math.Max(CurrentItem.ResumePosition, CurrentItem.StartPosition));
				ShouldSeek = true; // go to start point once the user decides to start.
			}

			foreach (var ctrl in ControlSets) {
				try {
					ctrl.PlaylistChanged(CurrentPlaylist);
				} catch (Exception e) {drop(e);}
			}
		}

		#region Media Player Events
		void MediaPlayer_SmoothStreamingErrorOccurred (object sender, SmoothStreamingErrorEventArgs e) {
			foreach (var ctrl in ControlSets) {
				try {
					ctrl.ErrorOccured(e.ErrorException);
				} catch (Exception ex) { drop(ex); }
			}
		}
		void MediaPlayer_ClipError (object sender, ClipEventArgs e) {
			foreach (var ctrl in ControlSets) {
				try {
					ctrl.ErrorOccured(new Exception(e.Context.ClipInformation.ClipUri + " failed"));
				} catch (Exception ex) { drop(ex); }
			}
		}
		void MediaPlayer_BufferingProgressChanged (object sender, RoutedEventArgs e) {
			RefreshStatus();
			foreach (var ctrl in ControlSets) {
				try {
					ctrl.StatusUpdate(Status);
				} catch (Exception ex) { drop(ex); }
			}
		}
		void MediaPlayer_MarkerReached (object sender, TimelineMarkerRoutedEventArgs e) {
			if (!CurrentPlaylist.EnableCaptions) return;
			foreach (var ctrl in ControlSets) {
				try {
					ctrl.CaptionFired(e.Marker);
				} catch (Exception ex) { drop(ex); }
			}
		}



		void MediaPlayer_PlaybackStateChange (object sender, ClipEventArgs e) {
			RefreshStatus();
			SendToControls(c => c.PlayStateChanged(Status));
			if (e.Context.CurrentClipState == MediaElementState.Playing) {
				WantToPlay = false;
			}
		}

		private void MediaPlayer_MediaEnded (object sender, RoutedEventArgs e) {
			if (InPlayMode) GoToPlaylistIndex(CurrentIndex + 1); // go to next, or end.
		}
		private void MediaPlayer_MediaOpened (object sender, RoutedEventArgs e) {
			RefreshStatus();
			if (Status.IsLive) {
				// If the media is live
				ResumeTime = TimeSpan.FromSeconds(MediaPlayer.LivePosition);
			}

			MediaPlayer.Position = ResumeTime;
			if (WantToPlay || InPlayMode) Play();
		}
		#endregion

		private void SetupTimer () {
			if (ActionTimer == null) {
				ActionTimer = new DispatcherTimer{Interval = TimeSpan.FromMilliseconds(350)};
				ActionTimer.Tick += ActionTimer_Tick;
			}

			ActionTimer.Start();


			if (CaptionTimer == null) {
				CaptionTimer = new DispatcherTimer{Interval = TimeSpan.FromMilliseconds(250)};
				CaptionTimer.Tick += CaptionTimer_Tick;
			}

			CaptionTimer.Start();
		}

		private void CaptionTimer_Tick (object sender, EventArgs e) {
			if (CurrentItem == null) return;

			if (CurrentItem.CaptionSource != null && (CurrentItem.CaptionItems == null || CurrentItem.CaptionItems.Count < 1)) {
				CurrentItem.UpdateCaptions(CurrentSliderPosition);
			}

			if (MediaPlayer.Markers != null && CurrentItem.IsAdaptiveStreaming && CurrentItem.CaptionItems != null) {
				// TODO: fix this for non-live/smooth transcripts!
				CurrentItem.UpdateCaptions(CurrentSliderPosition);
				AddCaptions(CurrentItem.CaptionItems);
			} else if (CurrentItem.CaptionItems != null && MediaPlayer.Markers != null && MediaPlayer.Markers.Count != CurrentItem.CaptionItems.Count) {
				CaptionTimer.Interval = TimeSpan.FromMilliseconds(3000);
				AddCaptions(CurrentItem.CaptionItems);
			}
		}

		private static double GetProportion (PlayerStatus NewStatus) {
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

		/// <summary>
		/// Update the 'Status' property with the most recently available values.
		/// </summary>
		private void RefreshStatus () {
			var new_status = new PlayerStatus();

			if (!PosterMode) {
				if (CurrentItem != null) {
					if (!SmoothstreamBeginVariablesSet && CurrentItem.IsAdaptiveStreaming && MediaPlayer.CurrentState == SmoothStreamingMediaElementState.Playing) {
						SmoothstreamBeginVariablesSet = true;
						MediaPlayer.Position = TimeSpan.FromSeconds(Math.Max(CurrentItem.ResumePosition, CurrentItem.StartPosition));
					}


					new_status.ClipEnd = (CurrentItem.StopPosition > CurrentItem.StartPosition) ? (TimeSpan.FromSeconds(CurrentItem.StopPosition)) : (MediaPlayer.EndPosition);
					new_status.ClipStart = TimeSpan.FromSeconds(Math.Max(CurrentItem.StartPosition, MediaPlayer.StartPosition.TotalSeconds));
					new_status.CurrentPlayState = MediaPlayer.CurrentState;
					new_status.NaturalDuration = BestDuration();
					new_status.PlaylistItemIndex = CurrentIndex;
					new_status.PlayTime = MediaPlayer.Position;
					new_status.BufferingProgress = MediaPlayer.BufferingProgress;
					try {
						if (IsLive) {
							new_status.ClipEnd = MediaPlayer.EndPosition;
							new_status.ClipStart = MediaPlayer.StartPosition;
						}
						new_status.NaturalStartPosition = TimeSpan.FromSeconds((IsLive) ? (MediaPlayer.LivePosition) : (Math.Max(CurrentItem.StartPosition, 0)));
					} catch {
						new_status.NaturalStartPosition = TimeSpan.Zero;
					}
					new_status.IsLive = IsLive;
					new_status.ClipDuration = ClipDuration();

					new_status.PlayProgress = GetProportion(Status);
				} else {
					new_status.ClipEnd = TimeSpan.Zero;
					new_status.ClipStart = TimeSpan.Zero;
					new_status.CurrentPlayState = SmoothStreamingMediaElementState.Stopped;
					new_status.NaturalDuration = TimeSpan.Zero;
					new_status.PlaylistItemIndex = CurrentIndex; // helpful during poster mode.
					new_status.PlayTime = TimeSpan.Zero;
					new_status.BufferingProgress = 0.0;
					new_status.NaturalStartPosition = TimeSpan.Zero;
					new_status.IsLive = false;
					new_status.ClipDuration = TimeSpan.Zero;
					new_status.PlayProgress = 0.0;
				}
			} else {
				new_status.ClipEnd = TimeSpan.Zero;
				new_status.ClipStart = TimeSpan.Zero;
				new_status.CurrentPlayState = SmoothStreamingMediaElementState.Buffering;
				new_status.NaturalDuration = TimeSpan.Zero;
				new_status.PlaylistItemIndex = CurrentIndex; // helpful during poster mode.
				new_status.PlayTime = TimeSpan.Zero;
				new_status.BufferingProgress = 0.0;
				new_status.NaturalStartPosition = TimeSpan.Zero;
				new_status.IsLive = false;
				new_status.ClipDuration = TimeSpan.Zero;
				new_status.PlayProgress = 0.0;
			}

			Status = new_status;
		}


		private bool _actionBusy;

		/// <summary>
		/// Periodic timer. This controls delayed-seeking, clip out-points
		/// and Status/UI updates.
		/// </summary>
		private void ActionTimer_Tick (object sender, EventArgs e) {
			if (_actionBusy) return;
			_actionBusy = true;

			try {

				if (InPosterDisplay()) {
					return; // Displaying poster. This is usually for legal messages, so we don't seek or switch.
				}

				if (ShouldSeek) { // a seek is desired.
					TryToSeek();
				}

				if (MediaPlayer.CurrentState == SmoothStreamingMediaElementState.Playing) {
					InPlayMode = true; // this player is now active. Keep trying to play until we've been paused!
					WantToPlay = false; // no longer need to re-issue.
					ClearPosterImage(); // make sure we're not covering the video!
				}

				if (CurrentItem != null) {
					if ((CurrentItem.StopPosition > CurrentItem.StartPosition)
						&& (MediaPlayer.Position.TotalSeconds > CurrentItem.StopPosition)) {
						// end of clip. Show next:
						GoToPlaylistIndex(CurrentIndex + 1); // go to next, or end.
					} else if (CurrentItem.MediaSource == null) { // nothing to play, move along!
						GoToPlaylistIndex(CurrentIndex + 1);
					}
				}
			} finally {
				_actionBusy = false;

				// update status, wherever we left the loop:
				RefreshStatus();
				SendToControls(c => c.StatusUpdate(Status));
			}
		}


		void SendToControls (Action<IPlayerController> action) {
			foreach (var ctrl in ControlSets) {
				try {
					action(ctrl);
				} catch (Exception ex) {
					try {
						ctrl.ErrorOccured(ex);
					} catch (Exception e) { drop(e); }
				}
			}
		}

		/// <summary>
		/// Attempts to seek to a new location in the media.
		/// </summary>
		private void TryToSeek () {
			switch (MediaPlayer.CurrentState) { // states invalid for seeking:
				case SmoothStreamingMediaElementState.AcquiringLicense:
				case SmoothStreamingMediaElementState.Closed:
				case SmoothStreamingMediaElementState.Individualizing:
				case SmoothStreamingMediaElementState.Opening:
				case SmoothStreamingMediaElementState.Stopped:
					return;
			}
			try {
				ShouldSeek = false;
				// Smooth Streaming fails badly when you seek off the end...
				TimeSpan live_pos = TimeSpan.FromSeconds(MediaPlayer.LivePosition);
				TimeSpan safe_end = live_pos;
				try {
					safe_end = (live_pos > MediaPlayer.NaturalDuration.TimeSpan) ? (live_pos) : (MediaPlayer.NaturalDuration.TimeSpan);
					safe_end += MediaPlayer.StartPosition;
				} catch (Exception ex) { drop(ex); }

				if (MediaPlayer.SmoothStreamingSource == null || safe_end > ResumeTime) {
					MediaPlayer.Position = ResumeTime;
				} else {
					MediaPlayer.Position = safe_end;
				}

				RefreshStatus();
				SendToControls(c => c.SeekCompleted(Status));
			} catch {
				ShouldSeek = true;
			}
		}

		#region Poster/thumbnail image stuff
		/// <summary>
		/// Checks to see if a displayed pre-roll poster should be expired.
		/// Returns true if the poster is still being displayed, false if expired.
		/// </summary>
		/// <remarks>If a poster duration is given to a playlist item, the
		/// poster image will be shown for that many seconds. At the start of
		/// poster display, the time will be set and the media element hidden.
		/// If the time has expired, the </remarks>
		private bool InPosterDisplay () {
			if (!PosterMode) return false;

			if (CurrentItem != null
				&& CurrentItem.ThumbSource != null
				&& CurrentItem.ThumbDuration > 0) {
				if (DateTime.Now > PosterSetTime.AddSeconds(CurrentItem.ThumbDuration)) {
					if (PosterMode) {
						ShouldSeek = false;
						PosterMode = false;

						if (CurrentItem.MediaSource == null) {// this is nothing but a poster
							GoToPlaylistIndex(CurrentIndex + 1);
						}
						if (InPlayMode) Play();

					}
					return false;
				}
				return true;
			}
			return false; // no poster or duration
		}

		/// <summary>
		/// Change background poster.
		/// </summary>
		private void ShowThumbnail (Uri uri) {
			Poster.ImageSource = new BitmapImage(uri.ForceAbsoluteByPage());

			Poster.Stretch = CurrentPlaylist.StretchMode;
			Poster.AlignmentX = AlignmentX.Center;
			Poster.AlignmentY = AlignmentY.Center;

			PosterDisplay.HorizontalAlignment = HorizontalAlignment.Center;
			PosterDisplay.VerticalAlignment = VerticalAlignment.Center;

			Background = Poster;
			PosterSetTime = DateTime.Now;
			PosterDisplay.Fill = Poster;
			PosterDisplay.Visibility = Visibility.Visible;
			PosterMode = true;
			MediaPlayer.Visibility = Visibility.Collapsed; // hide the media player
		}

		/// <summary>
		/// Remove poster/thumbnail from display.
		/// </summary>
		private void ClearPosterImage () {
			try {
				if (HasVideo()) { // don't clear poster for audio-only
					if (Background is ImageBrush) {
						MediaPlayer.Background = new SolidColorBrush(BackgroundColor);
						Background = new SolidColorBrush(BackgroundColor);
					}
					if (PosterDisplay.Visibility == Visibility.Visible) {
						PosterDisplay.Visibility = Visibility.Collapsed;
						PosterDisplay.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
					}
					MediaPlayer.Opacity = 1.0; // make sure we can be seen!
				}
				PosterMode = false;
			} catch {
				PosterMode = false;
			}
		}

		/// <summary>
		/// Returns true if current media seems to have a video track.
		/// </summary>
		/// <returns></returns>
		private bool HasVideo () {
			if (MediaPlayer == null) return false;
			if (MediaPlayer.VideoPlaybackTrack != null) return true;
			if (MediaPlayer.NaturalVideoHeight * MediaPlayer.NaturalVideoWidth > 0) return true;
			return false;
		}
		#endregion

		private TimeSpan BestDuration () {
			try {
				double nd = 0.0, dd = 0.0;
				try { dd = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds; } catch(Exception e) {drop(e);} // MediaPlayer.Duration can be problematic.
				if (MediaPlayer.NaturalDuration.HasTimeSpan) nd = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

				if (MediaPlayer.IsLive) nd = MediaPlayer.LivePosition;
				double sted = (MediaPlayer.EndPosition - MediaPlayer.StartPosition).TotalSeconds;

				return TimeSpan.FromSeconds(Math.Max(Math.Max(nd, dd), sted));
			} catch {
				return TimeSpan.Zero;
			}
		}

		private TimeSpan ClipDuration () {
			if (CurrentItem == null) return TimeSpan.Zero;
			try {
				double min = CurrentItem.StartPosition;
				double max = CurrentItem.StopPosition;
				var dur = BestDuration();
				if (max <= 0.0 || max < min) max = dur.TotalSeconds;

				double diff = max - min;
				if (diff < 0.0)
					return BestDuration();

				return TimeSpan.FromSeconds(max - min);
			} catch {
				return TimeSpan.Zero;
			}
		}

		private void AddCaptions (IEnumerable<CaptionItem> captions) {
			MediaPlayer.Markers.Clear();
			foreach (var captionItem in captions)
				MediaPlayer.Markers.Add(new TimelineMarker { Type = "CAPTION", Time = captionItem.Time, Text = captionItem.Text });
		}

		/// <summary>
		/// Set the play-head to the beginning of the current item
		/// </summary>
		private void SeekToItem () {
			if (CurrentItem == null) { Pause(); return; }
			
			if (CurrentItem.ThumbSource != null) DisplayPoster(CurrentIndex);
			if (CurrentItem.ThumbDuration > 0.0) {
				MediaPlayer.AutoPlay = false; // don't play, but allow pre-loading.
				ShouldSeek = true; // we should keep trying to load on the timer.
			} else {
				MediaPlayer.AutoPlay = InPlayMode;
			}

			ResumeTime = TimeSpan.FromSeconds(Math.Max(CurrentItem.ResumePosition, CurrentItem.StartPosition));
			if (CurrentItem.IsAdaptiveStreaming) {
				MediaPlayer.SmoothStreamingSource = CurrentItem.MediaSource;
			} else {
				MediaPlayer.Source = CurrentItem.MediaSource.ForceAbsoluteByPage();
			}

			if (!PosterMode && InPlayMode && MediaPlayer.CanSeek) {
				TrySetPosition(ResumeTime);
				TryPlay();
			}
		}

		void TrySetPosition(TimeSpan resumeTime) {
			try { MediaPlayer.Position = resumeTime; } catch (Exception ex) { drop(ex); }
		}

		// ReSharper disable UnusedParameter.Local
		// explicitly ignore an exception
		static void drop (Exception e) { }
		// ReSharper restore UnusedParameter.Local

		#endregion


	}
}
