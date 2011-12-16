using System;
using System.Windows;
using Microsoft.Web.Media.SmoothStreaming;

namespace ComposerCore {
	/// <summary>
	/// Structure for passing media player status among ComposerPlayer elements
	/// </summary>
	public struct PlayerStatus {
		/// <summary>
		/// Last reported play state
		/// </summary>
		public SmoothStreamingMediaElementState CurrentPlayState;

		/// <summary>
		/// Playhead position, as a proportion of the video to be played (clip or entire media).
		/// Range is 0..1; where 0 is the start of the playlist item, and 1 is the end.
		/// </summary>
		public double PlayProgress;

		/// <summary>
		/// Time of play-head (whether playing or paused)
		/// </summary>
		public TimeSpan PlayTime;

		/// <summary>
		/// Zero-based index of the current media item in the loaded playlist.
		/// </summary>
		public int PlaylistItemIndex;

		/// <summary>
		/// Natural duration of the currently loaded media (or zero if none), when playing at normal rate.
		/// This value ignores clip start and end points.
		/// </summary>
		public Duration NaturalDuration;

		/// <summary>
		/// The expected starting position for the media:
		/// For VOD, this will normally be ClipStart or zero;
		/// For LIVE, this will normally be the head ('NaturalDuration' - buffer time).<br/>
		/// If a ClipStart is set, live content will be played from this point if possible.
		/// </summary>
		public TimeSpan NaturalStartPosition;


		/// <summary>
		/// Duration of the current playlist item, as played (including clip start and end positions).
		/// </summary>
		public Duration ClipDuration;

		/// <summary>
		/// Offset from start of loaded media where play should begin.
		/// If &lt;= 0, media will be played from it's natural start.
		/// </summary>
		public TimeSpan ClipStart;

		/// <summary>
		/// Offset from start of loaded media where play should end.
		/// If &lt;= 0 or &gt;= NaturalDuration, media will stop at it's natural end.
		/// </summary>
		public TimeSpan ClipEnd;

		/// <summary>
		/// value 0..1 indicating buffering progress. 0 = empty buffer, 1 = buffering complete.
		/// </summary>
		public double BufferingProgress;

		/// <summary>
		/// True if the feed is currently live, false otherwise.
		/// If true, 'NaturalStartPosition' will be the head of the live feed.
		/// </summary>
		public bool IsLive;
	}
}
