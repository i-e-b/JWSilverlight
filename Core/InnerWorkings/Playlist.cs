using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Json;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Media;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Text;
using System.Net;

namespace ComposerCore {
	/// <summary>
	/// exception for invalid playlist
	/// </summary>
	public class InvalidPlaylistException : Exception {
		public InvalidPlaylistException (): base("Invalid Playlist") {}
		public InvalidPlaylistException (string message) : base(message) {}
	}

	[ScriptableType]
	public class Playlist : DependencyObject, IPlaylist {

		/// <summary>
		/// Event fired when a playlist has been loaded and is read for use.
		/// </summary>
		public event RoutedEventHandler PlaylistLoaded;

		/// <summary>
		/// Create a new empty playlist.
		/// </summary>
		public Playlist () {
			StretchMode = Stretch.None;
			items = new PlaylistCollection();
			items.CollectionChanged += Items_CollectionChanged;
			Init();
		}

		/// <summary>
		/// Create a new playlist from either a raw XML string or Uri to a playlist XML asset.
		/// </summary>
		public Playlist (string Contents) {
			ReadPlaylist(Contents);
		}

		/// <summary>
		/// Populate this playlist from either a raw XML string or Uri to a playlist XML asset.
		/// </summary>
		public void ReadPlaylist (string Contents) {
			try {
				StretchMode = Stretch.None;
				items = new PlaylistCollection();
				items.CollectionChanged += Items_CollectionChanged;

				Init();


				string strPlaylist = Uri.UnescapeDataString(Contents);
				if (strPlaylist.StartsWith("<?xml")) { // assume it's a raw playlist
					ParseXml(strPlaylist);
				} else if (strPlaylist.StartsWith("[[JSON]]")) { // JW JSON playlist
					ParseJSON(strPlaylist.Substring(8));
				} else { // assume it's a url for the playlist
					var playlistUri = new Uri(strPlaylist);

					var wc = new WebClient();
					wc.DownloadStringCompleted += WcDownloadStringCompleted;
					wc.DownloadStringAsync(playlistUri);
				}
			} catch (XmlException) {
				// special case. Encoder precompiled template? fail silently...
				if (Contents.IndexOf("<$=", StringComparison.OrdinalIgnoreCase) > 0 && Contents.IndexOf("$>", StringComparison.OrdinalIgnoreCase) > 0) {
					StretchMode = Stretch.None;
					items = new PlaylistCollection();
					items.CollectionChanged += Items_CollectionChanged;
					Init();
					return;
				}
				throw;
			}
		}


		#region PlaylistCore


		private void OnPlaylistLoaded (object sender, RoutedEventArgs args) {
			var freeze = PlaylistLoaded;
			if (freeze != null) {
				freeze(sender, args);
			}
		}

		/// <summary>
		/// Respond to a downloaded playlist XML asset.
		/// </summary>
		private void WcDownloadStringCompleted (object sender, DownloadStringCompletedEventArgs e) {
			if (e.Cancelled || e.Error != null) throw new Exception("Could not load playlist", e.Error);
			ParseXml(e.Result);
		}

		/// <summary>
		/// playlist options
		/// </summary>
		private bool loopPlaylist;
		private bool autoLoad;
		private bool autoPlay;
		private bool enableCachedComposition;
		private bool enableCaptions;
		private bool startMuted;
		private Stretch stretchMode;
		private Color background;
		/// <summary>
		/// list of playlist items
		/// </summary>        
		private PlaylistCollection items = new PlaylistCollection();
		/// <summary>
		/// Playlist changed event. This event fires on pretty much everything.
		/// Use 'PlaylistLoaded' if you want to capture the post-load event.
		/// </summary>
		[ScriptableMember]
		public event RoutedEventHandler PlaylistChanged;
		/// <summary>
		/// should playlist cue up 
		/// </summary>
		[Description("Automatically cue video when page is loaded"), DefaultValue(true)]
		public bool AutoLoad {
			get {
				return autoLoad;
			}
			set {
				autoLoad = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}


		/// <summary>
		/// Should playlist resume from start once it ends?
		/// Default: false.
		/// </summary>
		[Description("Restart playlist from beginning once ended"), DefaultValue(false)]
		public bool Loop {
			get {
				return loopPlaylist;
			}
			set {
				loopPlaylist = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}

		/// <summary>
		/// should playlist auto start?
		/// </summary>
		[Description("Automatically start video when cued"), DefaultValue(true)]
		public bool AutoPlay {
			get {
				return autoPlay;
			}
			set {
				autoPlay = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// cached composition enabled
		/// </summary>
		[Description("Enable Cached Composition"), DefaultValue(true)]
		public bool EnableCachedComposition {
			get {
				return enableCachedComposition;
			}
			set {
				enableCachedComposition = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// captions enabled
		/// </summary>
		[Description("Allow closed captions to show"), DefaultValue(true)]
		public bool EnableCaptions {
			get {
				return enableCaptions;
			}
			set {
				enableCaptions = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// start in muted state
		/// </summary>
		[Description("Mute player on start")]
		public bool StartMuted {
			get {
				return startMuted;
			}
			set {
				startMuted = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// type of video stretch
		/// </summary>
		[Description("Stretch Mode")]
		public Stretch StretchMode {
			get {
				return stretchMode;
			}
			set {
				stretchMode = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// color of background
		/// </summary>
		public Color Background {
			get {
				return background;
			}
			set {
				background = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// list of playlist items.
		/// </summary>
		[Description("Playlist")]
		public PlaylistCollection Items {
			get {
				return items;
			}
			set {
				items = value;
			}
		}

		/// <summary>
		/// This dictionary is populated with any unknown elements found in the playlist.
		/// These will NOT be serialised.
		/// </summary>
		public Dictionary<string, string> CustomProperties { get; set; }

		/// <summary>
		/// init structure
		/// </summary>
		private void Init () {
			CustomProperties = new Dictionary<string, string>();
			Loop = false;
			AutoLoad = true;
			AutoPlay = false;
			EnableCachedComposition = true;
			EnableCaptions = true;
			StartMuted = false;
			StretchMode = Stretch.Uniform;
		}
		/// <summary>
		/// construct a playlist item, provided for scripting.
		/// </summary>
		/// <returns>new playlist item</returns>
		public PlaylistItem CreateNewPlaylistItem () {
			return new PlaylistItem(Items);
		}
		/// <summary>
		/// inform playlist changed if the items collection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Items_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			if (PlaylistChanged != null) {
				PlaylistChanged(this, null);
			}
		}
		/// <summary>
		/// create playlist from XML string
		/// </summary>
		/// <param name="playlistXml">XML string</param>
		/// <returns>playlist</returns>
		public static Playlist Create (string playlistXml) {
			var playlist = new Playlist();
			playlist.ParseXml(playlistXml);
			return playlist;
		}
		/// <summary>
		/// parse playlist from XML
		/// </summary>
		/// <param name="playlistXml"></param>
		public void ParseXml (string playlistXml) {
			var enc = new UTF8Encoding();
			using (var ms = new MemoryStream(enc.GetBytes(playlistXml))) {
				var xmlrs = new XmlReaderSettings{IgnoreComments = true, IgnoreWhitespace = true};
				XmlReader reader = XmlReader.Create(ms, xmlrs);
				Deserialize(reader);
			}
			if (PlaylistChanged != null) {
				PlaylistChanged(this, null);
			}
			OnPlaylistLoaded(this, null);
		}

		public void ParseJSON (string jsonObject) {
			var value = (JsonArray)JsonValue.Parse(jsonObject);
			if (value == null) return;

			Items.Clear();
			foreach (JsonObject item in value) {
				if (!item.ContainsKey("file")) continue;
				var playItem = new PlaylistItem(items);

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
			OnPlaylistLoaded(this, null);
		}
		#endregion

		#region Serialization
		/// <summary>
		/// top level XML node for this class
		/// </summary>
		internal const string xmlNode = "Playlist";
		/// <summary>
		/// deserialise this object
		/// </summary>
		/// <param name="reader">XmlReader to deserialize from</param>
		/// <returns>this</returns>
		internal void Deserialize (XmlReader reader) {
			if (!reader.IsStartElement(xmlNode))
				throw new InvalidPlaylistException();

			Init();
			reader.Read();
			while (!(reader.Name == xmlNode && reader.NodeType == XmlNodeType.EndElement)) {
				if (reader.IsStartElement("Loop"))
					Loop = reader.ReadElementContentAsBoolean();
				if (reader.IsStartElement("AutoLoad"))
					AutoLoad = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("AutoPlay"))
					AutoPlay = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("EnableCachedComposition"))
					EnableCachedComposition = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("EnableCaptions"))
					EnableCaptions = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("StartMuted"))
					StartMuted = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("StretchMode"))
					StretchMode = (Stretch)Enum.Parse(typeof(Stretch), reader.ReadElementContentAsString(), false);
				else if (reader.IsStartElement(PlaylistCollection.xmlNode)) {
					Items.Clear();
					Items.Deserialize(reader);
				} else if (reader.IsStartElement()) {
					string key = reader.Name;
					string value = reader.ReadElementContentAsString();
					if (CustomProperties.ContainsKey(key)) CustomProperties[key] = value;
					else CustomProperties.Add(key, value);
				} else if (!reader.Read())
					break;
			}
		}
		/// <summary>
		/// Write the current playlist out to an xml-writer.
		/// </summary>
		public void Serialize (XmlWriter writer) {
			writer.WriteStartElement(xmlNode);
			writer.WriteElementString("AutoLoad", AutoLoad.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("AutoPlay", AutoPlay.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("EnableCachedComposition", EnableCachedComposition.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("EnableCaptions", EnableCaptions.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("StartMuted", StartMuted.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("StretchMode", StretchMode.ToString());
			items.Serialize(writer);
			writer.WriteEndElement();
		}
		#endregion
	}
}
