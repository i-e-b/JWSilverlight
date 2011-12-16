using System;

namespace ComposerCore {
	public class PlaylistChangedEventArgs : EventArgs {

		/// <summary>
		/// Updated or new playlist
		/// </summary>
		public Playlist NewPlaylist { get; private set; }

		/// <summary>
		/// Create a new event vased on the given playlist
		/// </summary>
		public PlaylistChangedEventArgs (Playlist NewPlaylist) {
			this.NewPlaylist = NewPlaylist;
		}
	}
}
