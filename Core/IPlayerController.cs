using System;
using System.Windows.Media;

namespace ComposerCore {
	/// <summary>
	/// Basic interface for a set of media player controls.
	/// The CONTROLs (visual or otherwise) implement this. The PLAYER will call the methods.
	/// </summary>
	public interface IPlayerController {
		
		/// <summary>
		/// Method called when the playlist is loaded or updated.
		/// </summary>
		void PlaylistChanged(IPlaylist NewPlaylist);

		/// <summary>
		/// Called when current clip is changed
		/// </summary>
		void PlayingClipChanged (IPlaylistItem NewClip);

		/// <summary>
		/// Called when player state changes
		/// </summary>
		void PlayStateChanged (PlayerStatus NewStatus);

		/// <summary>
		/// Called when a seek takes effect
		/// </summary>
		void SeekCompleted (PlayerStatus NewStatus);

		/// <summary>
		/// Called when player audio volume is changed
		/// </summary>
		void VolumeChanged (double NewVolume);

		/// <summary>
		/// Called when player is muted or unmuted
		/// </summary>
		void MuteChanged (bool IsMuted);

		/// <summary>
		/// Method called periodically to drive visual status updates
		/// </summary>
		void StatusUpdate (PlayerStatus NewStatus);

		/// <summary>
		/// Method called when captions are encountered in the media stream.
		/// </summary>
		void CaptionFired(TimelineMarker Caption);

		/// <summary>
		/// Method called when an error occurs with the media player or one of the playlist clips.
		/// </summary>
		void ErrorOccured (Exception Error);


		/// <summary>
		/// Method called by composer to make a controller responsible for the given player.
		/// </summary>
		void AddBinding (IPlayer PlayerToControl);

		/// <summary>
		/// Method called by composer to release responsibility for the given player.
		/// </summary>
		void RemoveBinding (IPlayer PlayerToControl);
	}
}
