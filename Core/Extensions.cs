using System;
using System.IO;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Windows.Browser;

namespace ComposerCore {
	public static class Extensions {

		/// <summary>
		/// Make relative paths into absolute paths based on the page the Silverlight
		/// component is embedded into. Paths which are already absolute are returned as-is.
		/// </summary>
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

		public static JsonValue ToJsonValue (this ScriptObject a) {

			var sr = new DataContractJsonSerializer(a.GetType());
			var ms = new MemoryStream();

			sr.WriteObject(ms, a);
			var o = JsonObject.Load(ms);
			ms.Close();

			return o;
		}
	}
}
