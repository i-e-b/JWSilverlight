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
		readonly MainPage jwPlayer;
		private readonly ComposerControlHelper players;

		const string ScriptRegistration = "jwplayer";
		readonly EventRegistry javascriptEvents;
		public HtmlInteraction (MainPage jwPlayer) {
			this.jwPlayer = jwPlayer;
			players = new ComposerControlHelper();
			javascriptEvents = new EventRegistry();

			// Register normal Silverlight bridge object
			HtmlPage.RegisterScriptableObject(ScriptRegistration, this);

			// bind scriptable object events back to html element (like Flash does):
			BackBind("jwAddEventListener", 2);//
			BackBind("jwRemoveEventListener", 2);//

			BackBind("jwGetBuffer", 0);//
			BackBind("jwGetDuration", 0);//
			BackBind("jwGetFullscreen", 0);//
			BackBind("jwGetHeight", 0);//
			BackBind("jwGetMute", 0);//
			BackBind("jwGetPlaylist", 0);//
			BackBind("jwGetPlaylistIndex", 0);//
			BackBind("jwGetPosition", 0);//
			BackBind("jwGetState", 0);//
			BackBind("jwGetWidth", 0);//
			BackBind("jwGetVersion", 0);
			BackBind("jwGetVolume", 0);//

			BackBind("jwPlay", 1);//
			BackBind("jwPause", 1);//
			BackBind("jwStop", 0);//
			BackBind("jwSeek", 1);//
			BackBind("jwLoad", 1);//
			BackBind("jwPlaylistItem", 1);//
			BackBind("jwPlaylistNext", 0);//
			BackBind("jwPlaylistPrev", 0);//
			BackBind("jwSetMute", 1);//
			BackBind("jwSetVolume", 1);//
			BackBind("jwSetFullscreen", 1);//

			BackBind("jwControlbarShow", 0);//
			BackBind("jwControlbarHide", 0);//
			BackBind("jwDisplayShow", 0);//
			BackBind("jwDisplayHide", 0);//
			BackBind("jwDockHide", 0);//
			BackBind("jwDockSetButton", 4);//
			BackBind("jwDockShow", 0);//

			Application.Current.Host.Content.FullScreenChanged += Content_FullScreenChanged;
			Application.Current.Host.Content.Resized += Content_Resized;

			// trigger player ready event
			HtmlPage.Window.Eval("jwplayer().playerReady(document.getElementById('" + HtmlPage.Plugin.Id + "'))");
		}

		void Content_Resized(object sender, EventArgs e) {
			FireJwEvent("jwplayerResize", "{width:" + Application.Current.Host.Content.ActualWidth + ", height:"
			+ Application.Current.Host.Content.ActualHeight + "}");
		}

		void Content_FullScreenChanged(object sender, EventArgs e) {
			FireJwEvent("jwplayerFullscreen", "{fullscreen:" + Application.Current.Host.Content.IsFullScreen.ToString().ToLower() + "}");
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
		public void jwDockSetButton (string id, string callback, string outGraphic, string overGraphic) {
			if (string.IsNullOrEmpty(callback)) {
				jwPlayer.DockView.RemoveCustomButton(id);
			} else {
				jwPlayer.DockView.SetCustomButton(id, () => {
					try {
						HtmlPage.Window.Eval(callback);
					} catch { }
				}, outGraphic, overGraphic);
			}
		}

		[ScriptableMember]
		public double jwGetBuffer () {
			return players.PlayerList.First().Status.BufferingProgress * 100;
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
		public int jwGetPlaylistIndex () {
			return players.First.CurrentIndex;
		}

		[ScriptableMember]
		public void jwPlaylistItem (int index) {
			players.EachPlayer(p => p.GoToPlaylistIndex(index));
		}

		[ScriptableMember]
		public void jwPlaylistNext () {
			players.EachPlayer(p => p.GoToPlaylistIndex(p.CurrentIndex + 1));
		}

		[ScriptableMember]
		public void jwPlaylistPrev () {
			players.EachPlayer(p => p.GoToPlaylistIndex(p.CurrentIndex - 1));
		}

		[ScriptableMember]
		public object jwGetPlaylist () {
			return HtmlPage.Window.Eval(players.First.CurrentPlaylist.Json());
		}

		[ScriptableMember]
		public void jwLoad (object obj) {
			if (obj is string) { 
				jwLoad_Single((string)obj); 
			} else if (obj is ScriptObject) { 
				jwLoad_Json((ScriptObject)obj); 
			} else throw new Exception("no way of interpreting data");
		}

		public void jwLoad_Single (string singleItem) {
			var lower = singleItem.ToLower();

			if (lower.EndsWith(".xml") || lower.EndsWith(".rss") || lower.EndsWith(".atom")) {
				players.EachPlayer(p => p.LoadPlaylist(new Uri(singleItem, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage().AbsoluteUri));
			} else {
				var fakePlaylist = "[[JSON]][{file:'" + singleItem + "'}]";
				players.EachPlayer(p => p.LoadPlaylist(fakePlaylist)); 
			}
		}

		public void jwLoad_Json (ScriptObject scriptPlaylist) {
			var str = scriptPlaylist.ToJsonValue().ToString().Trim();
			if (str.StartsWith("{") && str.EndsWith("}")) str = "["+str+"]";
			if (str.StartsWith("[") && str.EndsWith("]")) str = "[[JSON]]" + str;

			players.EachPlayer(p => p.LoadPlaylist(str));
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
			return jwState(lastState.CurrentPlayState);
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
			javascriptEvents.Bind(eventType, callback);
		}
		[ScriptableMember]
		public void jwRemoveEventListener (string eventType, string callback) {
			javascriptEvents.Unbind(eventType, callback);
		}

		[ScriptableMember]
		public void jwControlbarShow () {
			jwPlayer.ControlBarView.Visibility = Visibility.Visible;
			FireJwEvent("jwplayerComponentShow", "{component:'controlbar'}");
		}
		[ScriptableMember]
		public void jwControlbarHide () {
			jwPlayer.ControlBarView.Visibility = Visibility.Collapsed;
			FireJwEvent("jwplayerComponentHide", "{component:'controlbar'}");
		}

		[ScriptableMember]
		public void jwDisplayShow () {
			jwPlayer.DisplayView.Visibility = Visibility.Visible;
			FireJwEvent("jwplayerComponentShow", "{component:'display'}");
		}
		[ScriptableMember]
		public void jwDisplayHide () {
			jwPlayer.DisplayView.Visibility = Visibility.Collapsed;
			FireJwEvent("jwplayerComponentHide", "{component:'display'}");
		}

		[ScriptableMember]
		public void jwDockShow () {
			jwPlayer.DockView.Visibility = Visibility.Visible;
			FireJwEvent("jwplayerComponentShow", "{component:'dock'}");
		}
		[ScriptableMember]
		public void jwDockHide () {
			jwPlayer.DockView.Visibility = Visibility.Collapsed;
			FireJwEvent("jwplayerComponentHide", "{component:'dock'}");
		}

		PlayerStatus lastState;

		public void PlaylistChanged (IPlaylist NewPlaylist) {
			FireJwEvent("jwplayerPlaylistLoaded", "{playlist:"+NewPlaylist.Json()+"}");
		}
		public void PlayingClipChanged (IPlaylistItem NewClip) {
			FireJwEvent("jwplayerPlaylistItem", "{index:"+NewClip.PlaylistIndex+"}");
		}
		public void PlayStateChanged(PlayerStatus NewStatus) {
			var oldState = jwState(lastState.CurrentPlayState);
			var newState = jwState(NewStatus.CurrentPlayState);

			lastState = NewStatus;

			FireJwEvent("jwplayerPlayerState", "{oldstate:\"" + oldState + "\", newstate:\"" + newState + "\"}");
		}
		public void SeekCompleted(PlayerStatus NewStatus) {
			FireJwEvent("jwplayerMediaSeek", "{position:" + lastState.PlayTime.TotalSeconds + ", offset:" + NewStatus.PlayTime.TotalSeconds + "}");
			lastState = NewStatus;
		}
		public void VolumeChanged(double NewVolume) {
			FireJwEvent("jwplayerMediaVolume", "{volume:" + (NewVolume*100.0) + "}");
		}
		public void MuteChanged(bool IsMuted) {
			FireJwEvent("jwplayerMediaMute", "{mute:"+IsMuted.ToString().ToLower()+"}");
		}
		public void StatusUpdate (PlayerStatus NewStatus) {
			lastState = NewStatus;
			FireJwEvent("jwplayerMediaTime",
				"{duration: " + NewStatus.NaturalDuration.TimeSpan.TotalSeconds +
				//", offset: " + NewStatus.PlayTime.TotalSeconds + // todo: this should be last seek target (regardless of actual seek position)
				", position: " + NewStatus.PlayTime.TotalSeconds +
				"}");
		} 
		public void CaptionFired (TimelineMarker Caption) { } 
		public void ErrorOccured (Exception Error) {
			FireJwEvent("jwplayerMediaError", "{message:'" + Error.Message.Replace("'","\'") + "'}");
		} 
		public void AddBinding (IPlayer PlayerToControl) { players.AddBinding(PlayerToControl, this); }
		public void RemoveBinding (IPlayer PlayerToControl) { players.RemoveBinding(PlayerToControl, this); }

		void FireJwEvent(string eventName, string argsObject) {
			foreach (var callback in javascriptEvents[eventName]) {
				var str = "("+callback + ")(" + argsObject + ")";
				HtmlPage.Window.Eval(str);
			}
		}
		string jwState(SmoothStreamingMediaElementState playState) {
			switch (playState) {
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
	}
}
