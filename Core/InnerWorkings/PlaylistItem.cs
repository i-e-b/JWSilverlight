using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Browser;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;
using System.Xml.Linq;
using ComposerCore.InnerWorkings;

namespace ComposerCore {
	/// <summary>
	/// This class represents a media item in a playlist.
	/// </summary>
	[ScriptableType]
	public class PlaylistItem : INotifyPropertyChanged, IPlaylistItem {
		/// <summary>
		/// parameterless constructor required for Edit in Blend.
		/// </summary>
		public PlaylistItem () {
			m_chapters = new ScriptableObservableCollection<ChapterItem>();
			m_chapters.CollectionChanged += Chapters_CollectionChanged;
			Init();
		}
		/// <summary>
		/// Initializes a new instance of the PlaylistItem class.
		/// </summary>
		public PlaylistItem (PlaylistCollection collectionParent) {
			Init();
			m_collectionParent = collectionParent;
		}

		/// <summary>
		/// constructor provided for scripting.
		/// </summary>
		/// <returns>new chapter</returns>
		public ChapterItem CreateNewChapterItem () {
			return new ChapterItem();
		}
		/// <summary>
		/// Gets the index of this item in the collection.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int PlaylistIndex {
			get {
				if (m_collectionParent != null) {
					return m_collectionParent.IndexOf(this);
				}
				return -1;
			}
		}
		/// <summary>
		/// Gets or sets the description of this playlist item.
		/// </summary>
		[Description("Description of media item")]
		public String Description {
			get {
				return m_description;
			}

			set {
				m_description = value;
				OnPropertyChanged("Description");
			}
		}
		/// <summary>
		/// Gets the total file size of the encoded video for this item.
		/// </summary>
		[Description("file size of item in bytes"), DefaultValue(0)]
		public long FileSize {
			get {
				return m_fileSize;
			}
			set {
				m_fileSize = value;
			}
		}
		/// <summary>
		/// frame rate in FPS as persisted.
		/// </summary>
		[Description("frame rate in frames per second"), DefaultValue(30)]
		public double FrameRate {
			get {
				return m_frameRateFPS;
			}
			set {
				m_frameRateFPS = value;
				SmpteFrameRate = TimeCode.ParseFrameRate(value);
			}
		}
		/// <summary>
		/// Gets the width of the encoded video for this item.
		/// </summary>
		[Description("height in pixels"), DefaultValue(480)]
		public Double VideoHeight {
			get {
				return m_height;
			}
			set {
				m_height = value;
			}
		}

		/// <summary>
		/// Time to start play in seconds offset.
		/// This defines a clip (sub-section of playlist item media)
		/// </summary>
		[Description("Time to start play in seconds offset"), DefaultValue(0.0)]
		public double StartPosition { get; set; }

		/// <summary>
		/// Time to end play in seconds offset.
		/// This defines a clip (sub-section of playlist item media)
		/// </summary>
		[Description("Time to end play in seconds offset"), DefaultValue(-1.0)]
		public double StopPosition { get; set; }


		/// <summary>
		/// Time to start play in seconds offset.
		/// This DOES NOT define a clip. Normal seeking can go before this point,
		/// and controls will show that play has started part-way through media.
		/// </summary>
		[Description("Time to start play in seconds offset"), DefaultValue(0.0)]
		public double ResumePosition { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this item uses adaptive streaming.
		/// </summary>
		[Description("is adaptive streaming item")]
		public bool IsAdaptiveStreaming {
			get {
				if (!MediaSource.IsAbsoluteUri) return false;
				var lpath = MediaSource.AbsolutePath.ToLower();
				return lpath.EndsWith(".ism") || lpath.EndsWith(".isml") || lpath.EndsWith("/manifest");
			}
		}
		/// <summary>
		/// Gets the offline video bitrate used for adaptive streaming.
		/// This item is not scriptable
		/// </summary>
		internal long OfflineVideoBitrateInKbps {
			get {
				return m_offlineVideoBitrateInKbps;
			}
			set {
				m_offlineVideoBitrateInKbps = value;
			}
		}
		/// <summary>
		/// Gets or sets the Url of the media item.
		/// </summary>
		[Description("uri of item")]
		public Uri MediaSource {
			get {
				return m_mediaUri;
			}

			set {
				if (value == null || !value.IsAbsoluteUri) {
					m_mediaUri = value;
				OnPropertyChanged("MediaSource");
					return;
				}
				
				var labsPath = value.AbsolutePath.ToLower();
				if (labsPath.EndsWith("manifest")) {
					m_mediaUri = value;
				} else if (labsPath.EndsWith(".ism") || labsPath.EndsWith(".isml")) {
					var left = value.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
					m_mediaUri = new Uri(left + value.AbsolutePath + "/manifest" + value.Query);
				} else {
					m_mediaUri = value;
				}

				OnPropertyChanged("MediaSource");
			}
		}
		/// <summary>
		/// Gets or sets the source of the thumbnail for this item. 
		/// Uses a string instead of a URI to facilitate binding and handling cases where 
		/// the thumbnail file is missing without generating a page error.
		/// </summary>
		[Description("optional thumbnail for gallery and poster frame")]
		public Uri ThumbSource {
			get {
				return m_thumbSource;
			}

			set {
				m_thumbSource = value;
				OnPropertyChanged("ThumbSource");
			}
		}


		/// <summary>
		/// Optional duration to display thumbnail as poster frame before starting media in seconds.
		/// </summary>
		[Description("optional duration to display thumbnail as poster frame before starting media")]
		public double ThumbDuration { get; set; }

		/// <summary>
		/// Gets or sets the title of the playlist item.
		/// </summary>
		[Description("title of item")]
		public String Title {
			get {
				return m_title;
			}

			set {
				m_title = value;
				OnPropertyChanged("Title");
			}
		}
		/// <summary>
		/// Gets the width of the encoded video for this item.
		/// </summary>
		[Description("width in pixels"), DefaultValue(640)]
		public Double VideoWidth {
			get {
				return m_width;
			}
			set {
				m_width = value;
			}
		}
		/// <summary>
		/// Gets the chapters in this item.
		/// </summary>
		[Description("list of chapters")]
		public ScriptableObservableCollection<ChapterItem> Chapters {
			get { return m_chapters; }
			set { m_chapters = value; }
		}
		/// <summary>
		/// Gets or sets the frame rate of this item.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SmpteFrameRate SmpteFrameRate {
			get {
				return m_frameRate;
			}
			set {
				m_frameRate = value;
				m_frameRateFPS = 1 / TimeCode.FromFrames(1, value).TotalSeconds;
				OnPropertyChanged("FrameRate");
			}
		}

		public IList<CaptionItem> CaptionItems { get; set; }

		public Uri CaptionSource { get; set; }


		#region Inner workings
		/// <summary>The parent collection of this item.</summary>
		private readonly PlaylistCollection m_collectionParent;
		/// <summary>The title of this item.</summary>
		private String m_title;
		/// <summary>The description of this item.</summary>
		private String m_description;
		/// <summary>The thumbnail source for this item.</summary>
		private Uri m_thumbSource;
		/// <summary>The width of the encoded video for this item.</summary>
		private Double m_width;
		/// <summary>The height of the encoded video for this item.</summary>
		private Double m_height;
		/// <summary>total filesize of this item</summary>
		private long m_fileSize;
		/// <summary>The Url for the media of this item.</summary>
		private Uri m_mediaUri;
		/// <summary>
		/// The video bitrate of the adaptive stream that was downloaded and stored in isolated storage for offline playback. 
		/// </summary>
		private long m_offlineVideoBitrateInKbps;
		/// <summary>The frame rate of this item.</summary>
		private SmpteFrameRate m_frameRate = SmpteFrameRate.Unknown;
		/// <summary>frame rate in FPS as persisted.</summary>
		private double m_frameRateFPS;
		/// <summary>The chapters in this item.</summary>
		private ScriptableObservableCollection<ChapterItem> m_chapters = new ScriptableObservableCollection<ChapterItem>();


		/// <summary>Property changed event.</summary>
		public event PropertyChangedEventHandler PropertyChanged;




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
			m_title = string.Empty;
			m_description = string.Empty;
			m_thumbSource = null;
			m_fileSize = 0;
			m_frameRate = SmpteFrameRate.Smpte30;
			m_width = 640;
			m_height = 480;
			m_offlineVideoBitrateInKbps = 0;
		}

		/// <summary>
		/// chapters collection changed. means this playlist item changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Chapters_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			OnPropertyChanged("Chapters");
		}
		/// <summary>
		/// Implements INotifyPropertyChanged.OnPropertyChanged().
		/// </summary>
		/// <param name="memberName">The name of the property that changed.</param>
		protected void OnPropertyChanged (string memberName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(memberName));
			}
		}
		#endregion

		#region Serialization
		/// <summary>
		/// top level XML node for this class & chapters
		/// </summary>
		internal const string xmlNode = "PlaylistItem";
		internal const string xmlChaptersNode = "Chapters";
		/// <summary>
		/// deserialise chapters
		/// </summary>
		/// <param name="reader">XmlReader to deserialize from</param>
		/// <returns>this</returns>
		private void DeserializeChapters (XmlReader reader) {
			if (!reader.IsStartElement(xmlChaptersNode))
				throw new InvalidPlaylistException();

			reader.Read();
			while (!(reader.Name == xmlChaptersNode && reader.NodeType == XmlNodeType.EndElement)) {
				if (reader.IsStartElement("ChapterItem"))
					Chapters.Add(new ChapterItem().Deserialize(reader));
				else if (reader.IsStartElement())
					throw new InvalidPlaylistException(xmlChaptersNode);
				else if (!reader.Read())
					break;
			}
		}
		/// <summary>
		/// deserialise this object
		/// </summary>
		/// <param name="reader">XmlReader to deserialize from</param>
		/// <returns>this</returns>
		internal PlaylistItem Deserialize (XmlReader reader) {
			if (!reader.IsStartElement(xmlNode))
				throw new InvalidPlaylistException();

			Init();
			reader.Read();
			while (!(reader.Name == xmlNode && reader.NodeType == XmlNodeType.EndElement)) {
				if (reader.IsStartElement("Description"))
					Description = reader.ReadElementContentAsString();
				else if (reader.IsStartElement("FileSize"))
					FileSize = reader.ReadElementContentAsLong();
				else if (reader.IsStartElement("FrameRate"))
					FrameRate = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement("Height"))
					VideoHeight = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement("OfflineVideoBitrateInKbps"))
					OfflineVideoBitrateInKbps = reader.ReadElementContentAsLong();
				else if (reader.IsStartElement("MediaSource")) {
					string rawMediaSourceUrl = reader.ReadElementContentAsString();
					string decodedMediaSourceUrl = HttpUtility.UrlDecode(rawMediaSourceUrl) ?? "Broken URL";
					MediaSource = new Uri(decodedMediaSourceUrl, UriKind.RelativeOrAbsolute);
				} else if (reader.IsStartElement("ThumbSource")) {
					string rawThumbSourceUrl = reader.ReadElementContentAsString();
					string decodedThumbSourceUrl = HttpUtility.UrlDecode(rawThumbSourceUrl) ?? "Broken URL";
					ThumbSource = new Uri(decodedThumbSourceUrl, UriKind.RelativeOrAbsolute);
				} else if (reader.IsStartElement("Title"))
					Title = reader.ReadElementContentAsString();
				else if (reader.IsStartElement("Width"))
					VideoWidth = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement(xmlChaptersNode))
					DeserializeChapters(reader);
				else if (reader.IsStartElement("AudioCodec"))
					reader.ReadElementContentAsObject(); // ignored
				else if (reader.IsStartElement("VideoCodec"))
					reader.ReadElementContentAsObject(); // ignored
				else if (reader.IsStartElement("In"))
					StartPosition = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement("Out"))
					StopPosition = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement("Resume"))
					ResumePosition = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement("CaptionUrl")) {
					string rawCaptionSourceUrl = reader.ReadElementContentAsString();
					CaptionSource = new Uri(rawCaptionSourceUrl, UriKind.RelativeOrAbsolute);
				} else if (reader.IsStartElement("ThumbDuration"))
					ThumbDuration = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement()) {
					string key = reader.Name;
					string value = reader.ReadElementContentAsString();
					if (CustomProperties.ContainsKey(key)) CustomProperties[key] = value;
					else CustomProperties.Add(key, value);
				} else if (!reader.Read())
					break;
			}
			return this;
		}


		/// <summary>
		/// serialize this object
		/// </summary>
		/// <param name="writer">XmlWriter to serialze to</param>
		public void Serialize (XmlWriter writer) {
			writer.WriteStartElement(xmlNode);
			writer.WriteElementString("Description", Description);
			writer.WriteElementString("FileSize", FileSize.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("FrameRate", FrameRate.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Height", VideoHeight.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("IsAdaptiveStreaming", IsAdaptiveStreaming.ToString().ToLower(CultureInfo.InvariantCulture));
			writer.WriteElementString("OfflineVideoBitrateInKbps", OfflineVideoBitrateInKbps.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("MediaSource", MediaSource.ToString());
			writer.WriteElementString("ThumbSource", ThumbSource.ToString());
			writer.WriteElementString("Title", Title);
			writer.WriteElementString("Width", VideoWidth.ToString(CultureInfo.InvariantCulture));
			if (Chapters.Count > 0) {
				writer.WriteStartElement("Chapters");
				foreach (var item in Chapters) {
					item.Serialize(writer);
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public string JSON() { throw new NotImplementedException(); }
		#endregion
		public void UpdateCaptions(TimeSpan currentPlaybackTime) {
			LoadCaptionsFromSource(currentPlaybackTime);
		}


		public void LoadCaptionsFromSource (TimeSpan currentPosition) {
			if (CaptionSource == null) return;

			// todo: reinstate smooth captions, with some intellegence!
			//var captionUri = new Uri(string.Format(CaptionSource.AbsoluteUri + "&currentPosition={0}", currentPosition));
			var captionUri = CaptionSource;

			try {
				var webRequest = (HttpWebRequest)WebRequest.Create(captionUri);

				webRequest.BeginGetResponse(callback => {
					var req = (HttpWebRequest)callback.AsyncState;
					var res = (HttpWebResponse)req.EndGetResponse(callback);
					XDocument captionXml = XDocument.Load(res.GetResponseStream());

					CaptionItems = CaptionReader.Read(captionXml);


				}, webRequest);
			} catch (Exception ex) {
				drop(ex);
			}
		}

		// ReSharper disable UnusedParameter.Local
		private void drop (Exception exception) { }
		// ReSharper restore UnusedParameter.Local
	}
}
