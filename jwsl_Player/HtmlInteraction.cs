using System;
using System.Linq;
using System.Text;
using System.Windows.Browser;
using System.Windows.Media;
using ComposerCore;
using Microsoft.Web.Media.SmoothStreaming;

namespace JwslPlayer {
	public class HtmlInteraction : IPlayerController {
		private readonly ComposerControlHelper helper;

		/*
		 * The following internal calls will need to be replicated:
		 * 
		 * jwGetBuffer, jwDockShow, jwDockHide, jwControlbarShow, jwControlbarHide,
		 * jwDisplayShow, jwDisplayHide, jwGetDuration, jwGetFullscreen, jwGetHeight,
		 * jwGetLockState, jwGetMute, jwGetPlaylist, jwGetPosition, jwGetState,
		 * jwGetVolume, jwGetWidth, jwSetFullscreen, jwSetMute, jwLoad, jwPlaylistItem,
		 * jwPlaylistPrev, jwPlaylistNext, jwPause, jwPlay, jwStop, jwSeek (seconds),
		 * jwSetVolume, jwGetPlaylistIndex
		 * 
		 * The following events will need to be triggered:
		 * 
		 * jwplayerAPIReady & jwplayerReady (on plugin loaded?), 
		 * jwplayerFullscreen, jwplayerResize, jwplayerError,
		 * jwplayerMediaBeforePlay, jwplayerComponentShow, jwplayerComponentHide, jwplayerMediaBuffer,
		 * jwplayerMediaBufferFull, jwplayerMediaError, jwplayerMediaLoaded, jwplayerMediaComplete,
		 * jwplayerMediaSeek, jwplayerMediaTime, jwplayerMediaVolume, jwplayerMediaMeta,
		 * jwplayerMediaMute, jwplayerPlayerState, jwplayerPlaylistLoaded, jwplayerPlaylistItem
		 * 
		 * The following states will need to be presented:
		 * 
		 * BUFFERING, IDLE, PAUSED, PLAYING
		 * 
		 */

		const string ScriptRegistration = "jwplayer";
		public HtmlInteraction () {
			helper = new ComposerControlHelper();
			HtmlPage.RegisterScriptableObject(ScriptRegistration, this);

			// experimental: bind scriptable object events back to html element (like Flash does):
			BackBind("jwAddEventListener", 2);
			BackBind("jwPause", 0);
			BackBind("jwPlay", 0);
			BackBind("jwGetState", 0);

			// trigger player ready event
			HtmlPage.Window.Eval("jwplayer().playerReady(document.getElementById('" + HtmlPage.Plugin.Id + "'))");
		}

		void BackBind (string methodName, int argCount) {
			var id = HtmlPage.Plugin.Id;
			var sb = new StringBuilder();
			sb.Append("var x = document.getElementById('");
			sb.Append(id);
			sb.Append("'); x.");
			sb.Append(methodName);
			sb.Append(" = function(");

			for (int i = 0; i < argCount; i++) {
				if (i > 0) sb.Append(',');
				sb.Append((char)('A' + i)); // if you have more than 26 arguments, improve this!
			}

			sb.Append("){return x.content.");
			sb.Append(ScriptRegistration);
			sb.Append(".");
			sb.Append(methodName);
			sb.Append("(");

			for (int i = 0; i < argCount; i++) {
				if (i > 0) sb.Append(',');
				sb.Append((char)('A' + i)); // if you have more than 26 arguments, improve this!
			}

			sb.Append(");};");
			HtmlPage.Window.Eval(sb.ToString());
			// var x = document.getElementById('container'); x.jwPlay = function(){return x.content.jwplayer.jwPlay();};
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
		public void jwPause () {
			foreach (var player in helper.PlayerList) {
				player.Pause();
			}
		}

		[ScriptableMember]
		public void jwPlay () {
			foreach (var player in helper.PlayerList) {
				player.Play();
			}
		}

		[ScriptableMember]
		public string jwGetState () {
			switch (lastState.CurrentPlayState) {
				case SmoothStreamingMediaElementState.ClipPlaying:
				case SmoothStreamingMediaElementState.Playing:
					return "PLAYING";

				case SmoothStreamingMediaElementState.Paused:
					return "PAUSED";

				case SmoothStreamingMediaElementState.Closed:
				case SmoothStreamingMediaElementState.Stopped:
					return "IDLE";

				default:
					return "BUFFERING";
			}
		}
		[ScriptableMember]
		public void jwAddEventListener (string eventType, string callback) {

		}

		PlayerStatus lastState;

		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void StateChanged (PlayerStatus NewStatus) { lastState = NewStatus; }
		public void StatusUpdate (PlayerStatus NewStatus) { lastState = NewStatus; } 
		public void CaptionFired (TimelineMarker Caption) { } 
		public void ErrorOccured (Exception Error) { } 
		public void AddBinding (IPlayer PlayerToControl) { helper.AddBinding(PlayerToControl, this); }
		public void RemoveBinding (IPlayer PlayerToControl) { helper.RemoveBinding(PlayerToControl, this); }

	}
}
