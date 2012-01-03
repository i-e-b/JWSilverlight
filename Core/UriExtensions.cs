using System;
using System.Windows.Browser;

namespace ComposerCore {
	public static class UriExtensions {
		public static Uri ForceAbsoluteByPage (this Uri uri) {
			if (!uri.IsAbsoluteUri) {
				if (uri.OriginalString.StartsWith("/")) {
					return new Uri(
						HtmlPage.Document.DocumentUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
						+ uri.OriginalString,
						UriKind.Absolute);
				}
				return new Uri(HtmlPage.Document.DocumentUri, uri.OriginalString);
			}
			return uri;
		}
	}
}
