using System;
using System.ComponentModel;
using System.Collections.Generic;
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
	public class Playlist : DependencyObject {

		/// <summary>
		/// Event fired when a playlist has been loaded and is read for use.
		/// </summary>
		public event RoutedEventHandler PlaylistLoaded;

		/// <summary>
		/// Create a new empty playlist.
		/// </summary>
		public Playlist () {
			StretchMode = Stretch.None;
			_Items = new PlaylistCollection();
			_Items.CollectionChanged += Items_CollectionChanged;
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
				_Items = new PlaylistCollection();
				_Items.CollectionChanged += Items_CollectionChanged;

				Init();


				string strPlaylist = Uri.UnescapeDataString(Contents);
				if (strPlaylist.StartsWith("<?xml")) { // assume it's a raw playlist
					ParseXml(strPlaylist);
				} else { // assume it's a url for the playlistStretchMode = Stretch.None;
					var playlistUri = new Uri(strPlaylist);

					var wc = new WebClient();
					wc.DownloadStringCompleted += wc_DownloadStringCompleted;
					wc.DownloadStringAsync(playlistUri);
				}
			} catch (XmlException) {
				// special case. Encoder precompiled template? fail silently...
				if (Contents.IndexOf("<$=", StringComparison.OrdinalIgnoreCase) > 0 && Contents.IndexOf("$>", StringComparison.OrdinalIgnoreCase) > 0) {
					StretchMode = Stretch.None;
					_Items = new PlaylistCollection();
					_Items.CollectionChanged += Items_CollectionChanged;
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
		private void wc_DownloadStringCompleted (object sender, DownloadStringCompletedEventArgs e) {
			if (e.Cancelled || e.Error != null) throw new Exception("Could not load playlist", e.Error);
			ParseXml(e.Result);
		}

		/// <summary>
		/// playlist options
		/// </summary>
		private bool m_loopPlaylist;
		private bool m_autoLoad;
		private bool m_autoPlay;
		private bool m_displayTimeCode;
		private bool m_enableCachedComposition;
		private bool m_enableCaptions;
		private bool m_enableOffline;
		private bool m_enablePopOut;
		private bool m_startMuted;
		private Stretch m_stretchMode;
		private Color m_background;
		/// <summary>
		/// list of playlist items
		/// </summary>        
		private PlaylistCollection _Items = new PlaylistCollection();
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
				return m_autoLoad;
			}
			set {
				m_autoLoad = value;
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
				return m_loopPlaylist;
			}
			set {
				m_loopPlaylist = value;
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
				return m_autoPlay;
			}
			set {
				m_autoPlay = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// display timecodes
		/// </summary>
		[Description("Display Timecode")]
		public bool DisplayTimeCode {
			get {
				return m_displayTimeCode;
			}
			set {
				m_displayTimeCode = value;
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
				return m_enableCachedComposition;
			}
			set {
				m_enableCachedComposition = value;
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
				return m_enableCaptions;
			}
			set {
				m_enableCaptions = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// offlining enabled
		/// </summary>
		[Description("Enable Player to be run offline"), DefaultValue(true)]
		public bool EnableOffline {
			get {
				return m_enableOffline;
			}
			set {
				m_enableOffline = value;
				if (PlaylistChanged != null) {
					PlaylistChanged(this, null);
				}
			}
		}
		/// <summary>
		/// popout enabled
		/// </summary>
		[Description("Enable Player popout"), DefaultValue(true)]
		public bool EnablePopOut {
			get {
				return m_enablePopOut;
			}
			set {
				m_enablePopOut = value;
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
				return m_startMuted;
			}
			set {
				m_startMuted = value;
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
				return m_stretchMode;
			}
			set {
				m_stretchMode = value;
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
				return m_background;
			}
			set {
				m_background = value;
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
				return _Items;
			}
			set {
				_Items = value;
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
			AutoPlay = true;
			EnableCachedComposition = true;
			EnableCaptions = true;
			EnablePopOut = true;
			EnableOffline = true;
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
				else if (reader.IsStartElement("DisplayTimeCode"))
					DisplayTimeCode = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("EnableCachedComposition"))
					EnableCachedComposition = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("EnableCaptions"))
					EnableCaptions = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("EnablePopOut"))
					EnablePopOut = reader.ReadElementContentAsBoolean();
				else if (reader.IsStartElement("EnableOffline"))
					EnableOffline = reader.ReadElementContentAsBoolean();
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
		internal void Serialize (XmlWriter writer) {
			writer.WriteStartElement(xmlNode);
			writer.WriteElementString("AutoLoad", AutoLoad.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("AutoPlay", AutoPlay.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("DisplayTimeCode", DisplayTimeCode.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("EnableCachedComposition", EnableCachedComposition.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("EnableCaptions", EnableCaptions.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("EnablePopOut", EnablePopOut.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("EnableOffline", EnableOffline.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("StartMuted", StartMuted.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("StretchMode", StretchMode.ToString());
			_Items.Serialize(writer);
			writer.WriteEndElement();
		}
		#endregion
	}
}
