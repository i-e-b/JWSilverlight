using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows.Browser;
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
			chapters = new ScriptableObservableCollection<ChapterItem>();
			chapters.CollectionChanged += Chapters_CollectionChanged;
			Init();
		}
		/// <summary>
		/// Initializes a new instance of the PlaylistItem class.
		/// </summary>
		public PlaylistItem (PlaylistCollection collectionParent) {
			Init();
			this.collectionParent = collectionParent;
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
				if (collectionParent != null) {
					return collectionParent.IndexOf(this);
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
				return description;
			}

			set {
				description = value;
				OnPropertyChanged("Description");
			}
		}
		/// <summary>
		/// Gets the total file size of the encoded video for this item.
		/// </summary>
		[Description("file size of item in bytes"), DefaultValue(0)]
		public long FileSize {
			get {
				return fileSize;
			}
			set {
				fileSize = value;
			}
		}
		/// <summary>
		/// frame rate in FPS as persisted.
		/// </summary>
		[Description("frame rate in frames per second"), DefaultValue(30)]
		public double FrameRate {
			get {
				return frameRateFPS;
			}
			set {
				frameRateFPS = value;
				SmpteFrameRate = TimeCode.ParseFrameRate(value);
			}
		}
		/// <summary>
		/// Gets the width of the encoded video for this item.
		/// </summary>
		[Description("height in pixels"), DefaultValue(480)]
		public Double VideoHeight {
			get {
				return height;
			}
			set {
				height = value;
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
				return offlineVideoBitrateInKbps;
			}
			set {
				offlineVideoBitrateInKbps = value;
			}
		}
		/// <summary>
		/// Gets or sets the Url of the media item.
		/// </summary>
		[Description("uri of item")]
		public Uri MediaSource {
			get {
				return mediaUri;
			}

			set {
				if (value == null || !value.IsAbsoluteUri) {
					mediaUri = value;
					OnPropertyChanged("MediaSource");
					return;
				}

				var labsPath = value.AbsolutePath.ToLower();
				if (labsPath.EndsWith("manifest")) {
					mediaUri = value;
				} else if (labsPath.EndsWith(".ism") || labsPath.EndsWith(".isml")) {
					var left = value.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
					mediaUri = new Uri(left + value.AbsolutePath + "/manifest" + value.Query);
				} else {
					mediaUri = value;
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
				return thumbSource;
			}

			set {
				thumbSource = value;
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
				return title;
			}

			set {
				title = value;
				OnPropertyChanged("Title");
			}
		}
		/// <summary>
		/// Gets the width of the encoded video for this item.
		/// </summary>
		[Description("width in pixels"), DefaultValue(640)]
		public Double VideoWidth {
			get {
				return width;
			}
			set {
				width = value;
			}
		}
		/// <summary>
		/// Gets the chapters in this item.
		/// </summary>
		[Description("list of chapters")]
		public ScriptableObservableCollection<ChapterItem> Chapters {
			get { return chapters; }
			set { chapters = value; }
		}
		/// <summary>
		/// Gets or sets the frame rate of this item.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SmpteFrameRate SmpteFrameRate {
			get {
				return frameRate;
			}
			set {
				frameRate = value;
				frameRateFPS = 1 / TimeCode.FromFrames(1, value).TotalSeconds;
				OnPropertyChanged("FrameRate");
			}
		}

		public IList<CaptionItem> CaptionItems { get; set; }

		public Uri CaptionSource { get; set; }


		#region Inner workings
		private readonly PlaylistCollection collectionParent;
		private String title;
		private String description;
		private Uri thumbSource;
		private Double width;
		private Double height;
		private long fileSize;
		private Uri mediaUri;
		private long offlineVideoBitrateInKbps;
		private SmpteFrameRate frameRate = SmpteFrameRate.Unknown;
		private double frameRateFPS;
		private ScriptableObservableCollection<ChapterItem> chapters = new ScriptableObservableCollection<ChapterItem>();
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
			title = string.Empty;
			description = string.Empty;
			thumbSource = null;
			fileSize = 0;
			frameRate = SmpteFrameRate.Smpte30;
			width = 640;
			height = 480;
			offlineVideoBitrateInKbps = 0;
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


		public string Json () {
			var sb = new StringBuilder();
			Action<string, object> addIfNotEmpty = (name, value) =>
			{
				if (value != null && !string.IsNullOrEmpty(value.ToString())) {
					sb.Append(name); sb.Append(":\""); sb.Append(value); sb.Append("\",");
				}
			};
			sb.Append("{");

			addIfNotEmpty("file", MediaSource);
			addIfNotEmpty("image", ThumbSource);
			addIfNotEmpty("duration", StopPosition);
			addIfNotEmpty("start", StartPosition);
			addIfNotEmpty("title", Title);
			addIfNotEmpty("description", Description);
			addIfNotEmpty("captions", CaptionSource);

			sb.Append("},");
			return sb.ToString().Replace(",}", "}");
		}
		public void UpdateCaptions (TimeSpan currentPlaybackTime) {
			LoadCaptionsFromSource(currentPlaybackTime);
		}


		public void LoadCaptionsFromSource (TimeSpan currentPosition) {
			if (CaptionSource == null) return;

			// todo: reinstate smooth captions, with some intellegence!
			//var captionUri = new Uri(string.Format(CaptionSource.AbsoluteUri + "&currentPosition={0}", currentPosition));
			var captionUri = CaptionSource;

			try {
				var webRequest = (HttpWebRequest)WebRequest.Create(captionUri);

				webRequest.BeginGetResponse(callback =>
				{
					var req = (HttpWebRequest)callback.AsyncState;
					var res = (HttpWebResponse)req.EndGetResponse(callback);
					try {
						CaptionItems = CaptionReader.Read(XDocument.Load(res.GetResponseStream()));
					} catch (Exception ex) {
						drop(ex);
					}

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
