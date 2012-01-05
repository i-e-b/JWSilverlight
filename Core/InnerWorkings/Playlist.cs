using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Json;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ComposerCore {
	public class Playlist : IPlaylist {
		public event RoutedEventHandler PlaylistLoaded;
		public event RoutedEventHandler PlaylistChanged;

		public bool AutoLoad { get; set; }
		public bool Loop { get; set; }
		public bool AutoPlay { get; set; }
		public bool EnableCachedComposition { get; set; }
		public bool EnableCaptions { get; set; }
		public bool StartMuted { get; set; }
		public Stretch StretchMode { get; set; }
		public Color Background { get; set; }
		public PlaylistCollection Items { get; private set; }
		public Dictionary<string, string> CustomProperties { get; private set; }

		public Playlist() {
			CustomProperties = new Dictionary<string, string>();
			Items = new PlaylistCollection();
			Items.CollectionChanged += Items_CollectionChanged;
			Loop = false;
			AutoLoad = true;
			AutoPlay = false;
			EnableCachedComposition = true;
			EnableCaptions = true;
			StartMuted = false;
			StretchMode = Stretch.Uniform;
		}

		public void ReadPlaylist (string Contents) {
			var strPlaylist = Uri.UnescapeDataString(Contents);
			if (strPlaylist.StartsWith("<?xml")) { // assume it's a raw playlist
				//ParseXml(strPlaylist);
				//return;
				throw new Exception("Raw XML playlists not yet supported");
			}

			if (strPlaylist.StartsWith("[[JSON]]")) { // JW JSON playlist
				ParseJSON(strPlaylist.Substring(8));
				return;
			} 
			
			// assume it's a url for the playlist
			throw new Exception("Playlist URLs not yet supported");
			/*var playlistUri = new Uri(strPlaylist);

			var wc = new WebClient();
			wc.DownloadStringCompleted += WcDownloadStringCompleted;
			wc.DownloadStringAsync(playlistUri);*/
		}


		public void ParseJSON (string jsonObject) {
			var value = (JsonArray)JsonValue.Parse(jsonObject);
			if (value == null) return;

			Items.Clear();
			foreach (JsonObject item in value) {
				if (!item.ContainsKey("file")) continue;
				var playItem = new PlaylistItem(Items);

				foreach (var key in item.Keys) {
					switch (key) {
						case "file":
							var fileUrl = ((string)item[key]);
							playItem.MediaSource = new Uri(fileUrl, UriKind.RelativeOrAbsolute);
							break;
						case "image":
							var thumbUrl = ((string)item[key]);
							playItem.ThumbSource = new Uri(thumbUrl, UriKind.RelativeOrAbsolute);
							break;
						case "duration":
							playItem.StopPosition = double.Parse(item[key].ToString());
							break;
						case "start":
							playItem.ResumePosition = double.Parse(item[key].ToString());
							break;
						case "title":
							playItem.Title = item[key];
							break;
						case "description":
							playItem.Description = item[key];
							break;
						case "captions":
							var captionUrl = ((string)item[key]);
							playItem.CaptionSource = new Uri(captionUrl, UriKind.RelativeOrAbsolute);
							break;
						default:
							playItem.CustomProperties.Add(key, item[key]);
							break;
					}
				}

				Items.Add(playItem);
			}
			if (PlaylistChanged != null) {
				PlaylistChanged(this, null);
			}
			OnPlaylistLoaded(null);
		}

		void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			var handler = PlaylistChanged;
			if (handler != null)handler(this, null);
		}
		public void OnPlaylistLoaded (RoutedEventArgs e) {
			var handler = PlaylistLoaded;
			if (handler != null) handler(this, e);
		}
		public string Json () {
			var sb = new StringBuilder();

			sb.Append("[");
			foreach (var item in Items) {
				sb.Append(item.Json());
			}
			sb.Append("]");

			return sb.ToString().Replace(",]", "]");
		}
	}
}