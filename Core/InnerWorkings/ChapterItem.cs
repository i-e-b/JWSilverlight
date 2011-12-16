using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Browser;
using System.Xml;

namespace ComposerCore {

	/// <summary>
	/// This class describes a Chapter point in the media stream. Chapter points can
	/// contain a title and a thumbnail among other things.
	/// </summary>
	[ScriptableType]
	public class ChapterItem : INotifyPropertyChanged {
		/// <summary>
		/// top level XML node for this class
		/// </summary>
		internal const string xmlNode = "ChapterItem";
		/// <summary>
		/// The position of the chapter item.
		/// </summary>
		private double m_position;
		/// <summary>
		/// The source of the thumbnail for this chapter item.
		/// </summary>
		private Uri m_thumbSource;
		/// <summary>
		/// The title of this chapter item.
		/// </summary>
		private string m_title;
		/// <summary>
		/// Event which fires whenever a property changes on this chapter item.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// initialize chapter item
		/// </summary>
		private void Init () {
			m_title = string.Empty;
			m_thumbSource = null;
		}
		/// <summary>
		/// Initializes a new instance of the ChapterItem class.
		/// </summary>
		public ChapterItem () {
			Init();
		}
		/// <summary>
		/// Gets or sets the position in the media stream of this chapter item.
		/// </summary>
		[Description("chapter position in seconds")]
		public double Position {
			get {
				return m_position;
			}

			set {
				m_position = value;
				OnPropertyChanged("Position");
				OnPropertyChanged("PositionText");
			}
		}
		/// <summary>
		/// Gets or sets the source of the thumbnail for this chapter item.
		/// </summary>
		[Description("path to chapter thumbnail if required")]
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
		/// Gets or sets the title of this chapter item.
		/// </summary>
		[Description("title text of chapter")]
		public string Title {
			get {
				return m_title;
			}

			set {
				m_title = value;
				OnPropertyChanged("Title");
			}
		}
		/// <summary>
		/// Gets the position of this chapter item as a string.
		/// </summary>
		public virtual string PositionText {
			get {
				return TimeCode.ConvertToString(m_position, SmpteFrameRate.Unknown);
			}
		}
		#region INotifyPropertyChanged Members

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
		/// <summary>
		/// deserialise this object
		/// </summary>
		/// <param name="reader">XmlReader to deserialize from</param>
		/// <returns>this</returns>
		internal ChapterItem Deserialize (XmlReader reader) {
			if (!reader.IsStartElement(xmlNode))
				throw new InvalidPlaylistException();

			reader.Read();
			while (!(reader.Name == xmlNode && reader.NodeType == XmlNodeType.EndElement)) {
				if (reader.IsStartElement("Position"))
					Position = reader.ReadElementContentAsDouble();
				else if (reader.IsStartElement("ThumbSource"))
					ThumbSource = new Uri(reader.ReadElementContentAsString(), UriKind.RelativeOrAbsolute);
				else if (reader.IsStartElement("Title"))
					Title = reader.ReadElementContentAsString();
				else if (reader.IsStartElement())
					throw new InvalidPlaylistException(xmlNode);
				else if (!reader.Read())
					break;
			}
			return this;
		}
		/// <summary>
		/// serialize this object
		/// </summary>
		/// <param name="writer">XmlWriter to serialze to</param>
		internal void Serialize (XmlWriter writer) {
			writer.WriteStartElement(xmlNode);
			writer.WriteElementString("Position", Position.ToString(CultureInfo.InvariantCulture));
			if (ThumbSource != null) {
				writer.WriteElementString("ThumbSource", ThumbSource.ToString());
			}
			writer.WriteElementString("Title", Title);
			writer.WriteEndElement();
		}
	}
}
