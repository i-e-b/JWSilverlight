using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

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
			var strPlaylist = Uri.UnescapeDataString(Contents).Trim();
			var lower = strPlaylist.ToLower();

			if (lower.StartsWith("<")) { // assume it's a raw playlist
				InterpretXmlPlaylist(strPlaylist);
				return;
			}

			if (lower.StartsWith("[[json]]")) { // JW JSON playlist
				ParseJSON(strPlaylist.Substring(8));
				return;
			} 
			
			// assume it's a url for the playlist
			var playlistUri = new Uri(strPlaylist, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage();
			var wc = new WebClient();
			wc.DownloadStringCompleted += WcDownloadStringCompleted;
			wc.DownloadStringAsync(playlistUri);
		}

		void WcDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			if (e.Cancelled || e.Error != null) throw e.Error ?? new Exception("Playlist download cancelled");

			InterpretXmlPlaylist(e.Result.Trim());
		}

		void InterpretXmlPlaylist(string xmlString) {
			if (!xmlString.StartsWith("<?xml")) xmlString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmlString; // fix bad xml

			var doc = XDocument.Parse(xmlString);
			if (doc == null) throw new Exception("Could not load XML playlist");

			var firstElement = doc.Elements().First();
			switch (firstElement.Name.LocalName.ToLower()) {
				case "playlist": // XSPF
					ParseXSPF(doc);
					break;
				case "rss": // mRSS or iTunes RSS
					if (firstElement.Attribute("xmlns:itunes") != null) {
						ParseITunes(doc);
					} else {
						ParseMRSS(doc);
					}
					break;
				case "feed": // ATOM
					ParseAtom(doc);
					break;
				case "asx": // ASX
				case "ASX": // ASX
					ParseAsx(doc);
					break;

				default:
					throw new Exception("XML Playlist format not understood");
			}
		}

		void ParseAsx (XDocument doc) {
			Items.Clear();
			var entries = doc.Descendants("ENTRY");

			foreach (var entry in entries) {
				if (!(entry.Descendants("REF").Count() > 0)) continue;
				var playItem = new PlaylistItem(Items);

				if (entry.Descendants("TITLE").FirstOrDefault() != null)
					playItem.Title = entry.Descendants("TITLE").First().Value;

				var href = entry.Descendants("REF").First().Attribute("HREF");
				if (href == null) continue;

				playItem.MediaSource = new Uri(href.Value, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage();

				Items.Add(playItem);
			}
			if (PlaylistChanged != null) PlaylistChanged(this, null);
			OnPlaylistLoaded(null);
		}

		void ParseAtom(XDocument doc) { }

		void ParseMRSS(XDocument doc) { }

		void ParseITunes(XDocument doc) {  }

		void ParseXSPF (XDocument doc) {
			Items.Clear();
			var tracks = doc.Descendants().Where(d=>d.Name.LocalName == "track");

			foreach (var track in tracks) {
				var playItem = new PlaylistItem(Items);

				playItem.Title = getContent(track, "title");

				var href = getContent(track, "location"); if (href == null) continue;
				playItem.MediaSource = new Uri(href, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage();

				double dur;
				if (double.TryParse(getContent(track, "duration"), out dur)) {
					playItem.StopPosition = dur / 1000.0;
				}

				var thumb = getContent(track, "image");
				if (thumb != null)
					playItem.ThumbSource = new Uri(thumb, UriKind.RelativeOrAbsolute).ForceAbsoluteByPage();

				playItem.Description = getContent(track, "annotation");
				
				Items.Add(playItem);
			}
			if (PlaylistChanged != null) PlaylistChanged(this, null);
			OnPlaylistLoaded(null);
		}

		string getContent(XElement element, string name) {
			return element.Descendants().Where(d => d.Name.LocalName == name).FirstOrDefault() == null 
				? null
				: element.Descendants().Where(d => d.Name.LocalName == name).First().Value;
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
			if (PlaylistChanged != null) PlaylistChanged(this, null); 
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