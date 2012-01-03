using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Media;
using ComposerCore;
using Microsoft.Web.Media.SmoothStreaming;

namespace JwslPlayer {
	public class HtmlInteraction : IPlayerController {
		private readonly ComposerControlHelper players;

		/*
		 * Notes -- jwGetBuffer is 0..100 of buffered %, jwGetMeta is JSON string of playlist clip info,
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
		 */

		const string ScriptRegistration = "jwplayer";
		public HtmlInteraction () {
			players = new ComposerControlHelper();

			// Register normal Silverlight bridge object
			HtmlPage.RegisterScriptableObject(ScriptRegistration, this);

			// bind scriptable object events back to html element (like Flash does):
			BackBind("jwAddEventListener", 2);
			BackBind("jwRemoveEventListener", 2);

			BackBind("jwGetBuffer", 0);//
			BackBind("jwGetDuration", 0);//
			BackBind("jwGetFullscreen", 0);//
			BackBind("jwGetHeight", 0);//
			BackBind("jwGetMute", 0);
			BackBind("jwGetPlaylist", 0);
			BackBind("jwGetPlaylistIndex", 0);
			BackBind("jwGetPosition", 0);//
			BackBind("jwGetState", 0);//
			BackBind("jwGetWidth", 0);//
			BackBind("jwGetVersion", 0);
			BackBind("jwGetVolume", 0);//

			BackBind("jwPlay", 1);//
			BackBind("jwPause", 1);//
			BackBind("jwStop", 0);//
			BackBind("jwSeek", 1);//
			BackBind("jwLoad", 1);
			BackBind("jwPlaylistItem", 1);
			BackBind("jwPlaylistNext", 0);
			BackBind("jwPlaylistPrev", 0);
			BackBind("jwSetMute", 1);
			BackBind("jwSetVolume", 1);//
			BackBind("jwSetFullscreen", 1);//

			BackBind("jwControlbarShow", 0);
			BackBind("jwControlbarHide", 0);
			BackBind("jwDisplayShow", 0);
			BackBind("jwDisplayHide", 0);
			BackBind("jwDockHide", 0);
			BackBind("jwDockSetButton", 4);
			BackBind("jwDockShow", 0);

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
		public double jwGetBuffer () {
			return players.PlayerList.First().Status.BufferingProgress;
		}

		[ScriptableMember]
		public double jwGetDuration () {
			return players.PlayerList.First().Status.NaturalDuration.TimeSpan.TotalSeconds;
		}

		[ScriptableMember]
		public bool jwGetFullscreen () {
			return Application.Current.Host.Content.IsFullScreen;
		}

		[ScriptableMember]
		public void jwSetFullscreen (object ignored) {
			// not supported, due to Silverlight restrictions.
		}

		[ScriptableMember]
		public double jwGetVolume () {
			return players.PlayerList.First().AudioVolume * 100.0;
		}

		[ScriptableMember]
		public void jwSetVolume (double vol) {
			players.EachPlayer(p=>p.AudioVolume = vol / 100.0);
		}

		[ScriptableMember]
		public bool jwGetMute () {
			return players.Any(p => p.Mute);
		}

		[ScriptableMember]
		public void jwSetMute (string value) {
			if (value == null) {
				players.EachPlayer(p => p.Mute = !p.Mute);
				return;
			}

			players.EachPlayer(p => p.Mute = value.ToLower() == "true");
		}

		[ScriptableMember]
		public double jwGetPosition () {
			return players.PlayerList.First().Status.PlayTime.TotalSeconds;
		}

		[ScriptableMember]
		public void jwSeek (double seconds) {
			players.EachPlayer(p => p.SeekTo(TimeSpan.FromSeconds(seconds)));
		}

		[ScriptableMember]
		public void jwPause (string state) {
			foreach (var player in players.PlayerList) {
				if (state == null) togglePlay(player);
				else if (state.ToLower() == "false") player.Pause();
				else player.Play();
			}
		}

		[ScriptableMember]
		public void jwPlay (string state) {
			foreach (var player in players.PlayerList) {
				if (state == null) togglePlay(player);
				else if (state.ToLower() == "false") player.Play();
				else player.Pause();
			}
		}

		void togglePlay(IPlayer player) {
			if (player.IsActive()) player.Pause();
			else player.Play();
		}

		[ScriptableMember]
		public void jwStop () {
			players.EachPlayer(p => p.Pause());
			players.EachPlayer(p => p.SeekTo(0));
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
		public int jwGetHeight () {
			return (int)Application.Current.RootVisual.RenderSize.Height;
		}
		[ScriptableMember]
		public int jwGetWidth () {
			return (int)Application.Current.RootVisual.RenderSize.Width;
		}

		[ScriptableMember]
		public void jwAddEventListener (string eventType, string callback) {
			// todo: event callbacks!
		}
		[ScriptableMember]
		public void jwRemoveEventListener (string eventType, string callback) {
			// todo: event callbacks!
		}

		PlayerStatus lastState;

		public void PlaylistChanged (Playlist NewPlaylist) { }
		public void StateChanged (PlayerStatus NewStatus) { lastState = NewStatus; }
		public void StatusUpdate (PlayerStatus NewStatus) { lastState = NewStatus; } 
		public void CaptionFired (TimelineMarker Caption) { } 
		public void ErrorOccured (Exception Error) { } 
		public void AddBinding (IPlayer PlayerToControl) { players.AddBinding(PlayerToControl, this); }
		public void RemoveBinding (IPlayer PlayerToControl) { players.RemoveBinding(PlayerToControl, this); }

	}
}
