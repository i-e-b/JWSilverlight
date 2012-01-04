using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Media;
using System.Xml;

namespace ComposerCore {
	public interface IPlaylist {
		/// <summary>
		/// Event fired when a playlist has been loaded and is read for use.
		/// </summary>
		event RoutedEventHandler PlaylistLoaded;

		/// <summary> should playlist cue up</summary>
		[Description("Automatically cue video when page is loaded"), DefaultValue(true)]
		bool AutoLoad { get; set; }

		/// <summary> Should playlist resume from start once it ends? Default: false. </summary>
		[Description("Restart playlist from beginning once ended"), DefaultValue(false)]
		bool Loop { get; set; }

		/// <summary> should playlist auto start? </summary>
		[Description("Automatically start video when cued"), DefaultValue(true)]
		bool AutoPlay { get; set; }

		/// <summary> cached composition enabled </summary>
		[Description("Enable Cached Composition"), DefaultValue(true)]
		bool EnableCachedComposition { get; set; }

		/// <summary> captions enabled </summary>
		[Description("Allow closed captions to show"), DefaultValue(true)]
		bool EnableCaptions { get; set; }

		/// <summary> start in muted state </summary>
		[Description("Mute player on start")]
		bool StartMuted { get; set; }

		/// <summary>  type of video stretch </summary>
		[Description("Stretch Mode")]
		Stretch StretchMode { get; set; }

		/// <summary> color of background </summary>
		Color Background { get; set; }

		/// <summary> list of playlist items. </summary>
		[Description("Playlist")]
		PlaylistCollection Items { get; set; }

		/// <summary>
		/// This dictionary is populated with any unknown elements found in the playlist.
		/// These will NOT be serialised.
		/// </summary>
		Dictionary<string, string> CustomProperties { get; set; }

		/// <summary> Populate this playlist from either a raw XML string or Uri to a playlist XML asset. </summary>
		void ReadPlaylist (string Contents);

		/// <summary>
		/// Playlist changed event. This event fires on pretty much everything.
		/// Use 'PlaylistLoaded' if you want to capture the post-load event.
		/// </summary>
		[ScriptableMember]
		event RoutedEventHandler PlaylistChanged;

		/// <summary> Write the current playlist out to an xml-writer. </summary>
		void Serialize (XmlWriter writer);

		/// <summary> Write the current playlist out to a JSON formatted string </summary>
		string Json();
	}
}