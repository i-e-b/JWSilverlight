using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using ComposerCore.InnerWorkings;

namespace ComposerCore {
	public interface IPlaylistItem {
		/// <summary>
		/// Gets the index of this item in the collection.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		int PlaylistIndex { get; }

		/// <summary>
		/// Gets or sets the description of this playlist item.
		/// </summary>
		[Description("Description of media item")]
		string Description { get; set; }

		/// <summary>
		/// Gets the total file size of the encoded video for this item.
		/// </summary>
		[Description("file size of item in bytes"), DefaultValue(0)]
		long FileSize { get; set; }

		/// <summary>
		/// frame rate in FPS as persisted.
		/// </summary>
		[Description("frame rate in frames per second"), DefaultValue(30)]
		double FrameRate { get; set; }

		/// <summary>
		/// Gets the width of the encoded video for this item.
		/// </summary>
		[Description("height in pixels"), DefaultValue(480)]
		double VideoHeight { get; set; }

		/// <summary>
		/// Time to start play in seconds offset.
		/// This defines a clip (sub-section of playlist item media)
		/// </summary>
		[Description("Time to start play in seconds offset"), DefaultValue(0.0)]
		double StartPosition { get; set; }

		/// <summary>
		/// Time to end play in seconds offset.
		/// This defines a clip (sub-section of playlist item media)
		/// </summary>
		[Description("Time to end play in seconds offset"), DefaultValue(-1.0)]
		double StopPosition { get; set; }

		/// <summary>
		/// Time to start play in seconds offset.
		/// This DOES NOT define a clip. Normal seeking can go before this point,
		/// and controls will show that play has started part-way through media.
		/// </summary>
		[Description("Time to start play in seconds offset"), DefaultValue(0.0)]
		double ResumePosition { get; set; }

		/// <summary>
		/// Gets or sets the Url of the media item.
		/// </summary>
		[Description("uri of item")]
		Uri MediaSource { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this item uses adaptive streaming.
		/// </summary>
		[Description("is adaptive streaming item")]
		bool IsAdaptiveStreaming { get; }

		/// <summary>
		/// Gets or sets the source of the thumbnail for this item. 
		/// Uses a string instead of a URI to facilitate binding and handling cases where 
		/// the thumbnail file is missing without generating a page error.
		/// </summary>
		[Description("optional thumbnail for gallery and poster frame")]
		Uri ThumbSource { get; set; }

		/// <summary>
		/// Gets or sets the title of the playlist item.
		/// </summary>
		[Description("title of item")]
		string Title { get; set; }

		/// <summary>
		/// Gets the width of the encoded video for this item.
		/// </summary>
		[Description("width in pixels"), DefaultValue(640)]
		double VideoWidth { get; set; }

		/// <summary>
		/// Gets the chapters in this item.
		/// </summary>
		[Description("list of chapters")]
		ScriptableObservableCollection<ChapterItem> Chapters { get; set; }

		IList<CaptionItem> CaptionItems { get; set; }
		Uri CaptionSource { get; }

		/// <summary>
		/// This dictionary is populated with any unknown elements found in the playlist.
		/// These will NOT be serialised.
		/// </summary>
		Dictionary<string, string> CustomProperties { get; set; }

		/// <summary>
		/// Optional duration to display thumbnail as poster frame before starting media in seconds.
		/// </summary>
		[Description("optional duration to display thumbnail as poster frame before starting media"), DefaultValue(0)]
		double ThumbDuration { get; set; }

		void Serialize (XmlWriter writer);

		/// <summary>
		/// Returns a JSON object representing the state of the playlist item
		/// </summary>
		string Json ();

		/// <summary>
		/// Load or reload captions for the playlist item based on the captions source property.
		/// </summary>
		void UpdateCaptions(TimeSpan currentPlaybackTime);
	}
}