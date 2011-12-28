using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Browser;
using System.Xml.Linq;

namespace ComposerCore.InnerWorkings {
	public class CaptionReader {
		public static IList<CaptionItem> Read (XDocument captionXml) {
			var timedTextCaptions =
				captionXml.Descendants()
				.Where(caption => caption.Name.LocalName.ToLower() == "p")
				.Select(caption => new CaptionItem
				{
					Time = TimeSpan.Parse(caption.Attributes().First(time => time.Name.LocalName.ToLower() == "begin").Value),
					End = TimeSpan.Parse(caption.Attributes().First(time => time.Name.LocalName.ToLower() == "end").Value),
					Text = HttpUtility.HtmlDecode(caption.Value)
				})
				.ToList();

			if (timedTextCaptions.Count > 0) return timedTextCaptions;

			return ReadIsmcCaptions(captionXml);
		}

		static IList<CaptionItem> ReadIsmcCaptions(XDocument captionXml) {
			return captionXml.Descendants()
				.Where(caption => caption.Name.LocalName.ToLower() == "c")
				.Select(caption => new CaptionItem
				                   {
				                   	Time = new TimeSpan(0, 0, int.Parse(caption.Descendants().First(time => time.Name.LocalName.ToLower() == "t").Value)),
									End = TimeSpan.Zero,
				                   	Text = HttpUtility.HtmlDecode(caption.Descendants().First(time => time.Name.LocalName.ToLower() == "v").Value)
				                   })
				.ToList();
		}
	}
}
