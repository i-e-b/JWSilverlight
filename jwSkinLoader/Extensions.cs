using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace jwSkinLoader {
	public static class Extensions {
		public static MemoryStream ReadAll (this ZipInputStream zip) {
			var buffer = new byte[2048];
			int bytesRead;

			var ms = new MemoryStream();
			while ((bytesRead = zip.Read(buffer, 0, buffer.Length)) != 0){
				ms.Write(buffer, 0, bytesRead);
			}
			ms.Position = 0;
			return ms;
		}
		/// <summary>
		/// Return the substring up to but not including the first instance of 'c'.
		/// If 'c' is not found, the entire string is returned.
		/// </summary>
		public static string SubstringBefore (this String src, char c) {
			if (String.IsNullOrEmpty(src)) return "";

			int idx = Math.Min(src.Length, src.IndexOf(c));
			if (idx < 0) return src;
			return src.Substring(0, idx);
		}


		/// <summary>
		/// Return the substring up to but not including the last instance of 'c'.
		/// If 'c' is not found, the entire string is returned.
		/// </summary>
		public static string SubstringBeforeLast (this String src, char c) {
			if (String.IsNullOrEmpty(src)) return "";

			int idx = Math.Min(src.Length, src.LastIndexOf(c));
			if (idx < 0) return src;
			return src.Substring(0, idx);
		}

		/// <summary>
		/// Return the substring after to but not including the first instance of 'c'.
		/// If 'c' is not found, the entire string is returned.
		/// </summary>
		public static string SubstringAfter (this String src, char c) {
			if (String.IsNullOrEmpty(src)) return "";

			int idx = Math.Min(src.Length - 1, src.IndexOf(c) + 1);
			if (idx < 0) return src;
			return src.Substring(idx);
		}


		/// <summary>
		/// Return the substring after to but not including the last instance of 'c'.
		/// If 'c' is not found, the entire string is returned.
		/// </summary>
		public static string SubstringAfterLast (this String src, char c) {
			if (String.IsNullOrEmpty(src)) return "";

			int idx = Math.Min(src.Length - 1, src.LastIndexOf(c) + 1);
			if (idx < 0) return src;
			return src.Substring(idx);
		}
	}
}
