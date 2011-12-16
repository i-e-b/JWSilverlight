using System;
using System.Windows.Browser;
using System.Windows.Media;
using ComposerCore;

namespace SkeletonPlayer {
	public class HtmlInteraction : IPlayerController {
		private readonly ComposerControlHelper _helper;
		private readonly string _captionInvokeScriptMethod;

		public HtmlInteraction (string captionInvokeScriptMethod) {
			_helper = new ComposerControlHelper();
			_captionInvokeScriptMethod = captionInvokeScriptMethod;
		}

		#region IPlayerController Members

		public void PlaylistChanged (Playlist NewPlaylist) {
		}

		public void StateChanged (PlayerStatus NewStatus) {
		}

		public void StatusUpdate (PlayerStatus NewStatus) {
		}

		public void CaptionFired (TimelineMarker Caption) {
			if (string.IsNullOrEmpty(_captionInvokeScriptMethod)) return;

			string formattedCaption = (HttpUtility.UrlDecode(Caption.Text) ?? "").Replace("/ScriptEvent.html?", "").Trim();
			HtmlPage.Window.Invoke(_captionInvokeScriptMethod, formattedCaption);
		}

		public void ErrorOccured (Exception Error) {
		}

		public void AddBinding (IPlayer PlayerToControl) {
			_helper.AddBinding(PlayerToControl, this);
		}

		public void RemoveBinding (IPlayer PlayerToControl) {
		}

		#endregion
	}
}
