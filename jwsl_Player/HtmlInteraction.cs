using System;
using System.Linq;
using System.Windows.Browser;
using System.Windows.Media;
using ComposerCore;

namespace JwslPlayer {
	public class HtmlInteraction : IPlayerController {
		private readonly ComposerControlHelper helper;

		public HtmlInteraction () {
			helper = new ComposerControlHelper();
			HtmlPage.RegisterScriptableObject("jwplayer", this);
		}

		[ScriptableMember]
		public double GetMilliseconds () {
			return helper.PlayerList.First().Status.PlayTime.TotalMilliseconds;
		}

		[ScriptableMember]
		public void GotoMilliseconds (double milliseconds) {
			foreach (var player in helper.PlayerList) {
				player.SeekTo(TimeSpan.FromMilliseconds(milliseconds));
			}
		}

		[ScriptableMember]
		public void Pause () {
			foreach (var player in helper.PlayerList) {
				player.Pause();
			}
		}

		[ScriptableMember]
		public void Play () {
			foreach (var player in helper.PlayerList) {
				player.Play();
			}
		}

		public void PlaylistChanged (Playlist NewPlaylist) {
		}

		public void StateChanged (PlayerStatus NewStatus) {
		}

		public void StatusUpdate (PlayerStatus NewStatus) {
		}

		public void CaptionFired (TimelineMarker Caption) {
		}

		public void ErrorOccured (Exception Error) {
		}

		public void AddBinding (IPlayer PlayerToControl) {
			helper.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
		}

	}
}
