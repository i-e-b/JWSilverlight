using System;
using System.Windows;

namespace ComposerCore {
	public interface IPlayer {
		#region Status Properties and Methods (getting info)
		/// <summary>
		/// Get a containing element for the player.
		/// </summary>
		UIElement GetLayoutContainer { get; }

		/// <summary>
		/// Get the current playlist. Returns a blank playlist if none loaded.
		/// </summary>
		Playlist CurrentPlaylist { get; }

		/// <summary>
		/// Get the playlist item currently being played, if any.
		/// </summary>
		PlaylistItem CurrentItem { get; }

		/// <summary>
		/// Index of the current playlist item.
		/// If there is not current item, this will be invalid.
		/// </summary>
		int CurrentIndex { get; set; }

		/// <summary>
		/// Gets the player's current status.
		/// This is updated by a timer, so may not be frame-accurate.
		/// </summary>
		PlayerStatus Status { get; }

		/// <summary>
		/// Returns true if the Media Player is active, false otherwise.
		/// Buffering is considered active.
		/// </summary>
		bool IsPlayerActive ();

		/// <summary>
		/// Gets; Returns true if media is seekable, false for non-seekable (WME live, un-indexed VOD etc)
		/// </summary>
		bool CanSeek { get; }

		/// <summary>
		/// Gets; Returns true if media can be paused and resumed, false otherwise (WME live etc)
		/// </summary>
		bool CanPause { get; }

		#endregion

		#region Control Properties and Methods (changing the player state)
		/// <summary>
		/// Gets or Sets the player's audio volume. Range is 0..1
		/// </summary>
		double AudioVolume {get; set;}

		/// <summary>
		/// Gets or Sets; Default = false.
		/// If true, audio will not be played -- and may not be loaded.
		/// </summary>
		bool PictureInPicture {get; set; }

		/// <summary>
		/// Display the thumbnail for the given playlist item index.
		/// Causes playback to be paused. Sets current playlist position to 'Index'
		/// </summary>
		void DisplayPoster (int Index);

		/// <summary>
		/// Continue playing at the given playlist item index.
		/// If player is currently paused, play will resume if "AutoPlay" is set to true.
		/// If 'Index' is out of the range of the current playlist, the position will be reset to zero and the player paused.
		/// </summary>
		void GoToPlaylistIndex (int Index);

		/// <summary>
		/// Stops playback without changing position, regardless of current playstate
		/// </summary>
		void Pause ();

		/// <summary>
		/// Continues playback without changing position, regardless of current playstate
		/// </summary>
		void Play ();

		/// <summary>
		/// Continue playback at live point, if available.
		/// If media is not live, nothing happens.
		/// </summary>
		void GoToLive ();

		/// <summary>
		/// Try to move the playback position to an offset from the beginning of the clip.
		/// Seek will not continue past the start or end of the current item's media.
		/// Returns a guess at play-head time that will result.
		/// </summary>
		/// <remarks>If the playlist item has an 'IN' point and you wish to seek before this,
		/// Pass a negative value for offset (up to -IN). Seeking past the 'OUT' point is
		/// not supported</remarks>
		TimeSpan SeekTo (TimeSpan Offset);


		/// <summary>
		/// Try to move the playback position to an proportion of the clip.
		/// Returns a best-guess at the actual position set.
		/// Seek will not continue past the start or end of the current item's media.
		/// </summary>
		/// <param name="Proportion">Value 0..1 of clip length</param>
		TimeSpan SeekTo (double Proportion);

		/// <summary>
		/// Try to move the playback position to an offset from the current play position.
		/// Both positive and negative values are valid.
		/// 
		/// Seek will not continue past the start or end of the current item's media.
		/// Returns a guess at play-head time that will result.
		/// </summary>
		TimeSpan SeekRelative (TimeSpan Offset);


        TimeSpan CurrentSliderPosition {get; set;}

		#endregion

		#region Setup and Hook-up Methods (connecting players to controllers, setting playlists)
		/// <summary>
		/// Add a controller to the list of those to be notified of events.
		/// </summary>
		/// <remarks>A controller doesn't have to be in the list to send commands.</remarks>
		void AddController (IPlayerController Controller);

		/// <summary>
		/// Remove a controller from the list of those to be notified of events.
		/// </summary>
		/// <remarks>A controller doesn't have to be in the list to send commands.</remarks>
		void RemoveController (IPlayerController Controller);

		/// <summary>
		/// Load a playlist from the string, and replace current playlist (if any)
		/// </summary>
		void LoadPlaylist (string PlayList);

		/// <summary>
		/// Load a playlist, and replace current playlist (if any)
		/// </summary>
		void LoadPlaylist (Playlist p);
		#endregion
	}
}
