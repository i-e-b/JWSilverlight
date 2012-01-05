using System;

namespace ComposerCore {
	/// <summary>
	/// exception for invalid playlist
	/// </summary>
	public class InvalidPlaylistException : Exception {
		public InvalidPlaylistException (): base("Invalid Playlist") {}
		public InvalidPlaylistException (string message) : base(message) {}
	}
}