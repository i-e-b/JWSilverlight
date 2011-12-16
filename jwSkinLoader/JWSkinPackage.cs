using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows.Media.Imaging;
using ICSharpCode.SharpZipLib.Zip;

namespace jwSkinLoader {
	public class JwSkinPackage {
		readonly IsolatedStorageSettings files = IsolatedStorageSettings.ApplicationSettings;
		public string XmlContent {get; private set;}
		public string AllFiles {
			get {
				if (files.Keys.Count < 1) return "no files loaded";
				return files.Keys.Cast<string>().Aggregate((a, b) => a + ", " + b);
			}
		}

		public void Load (string packageUrl) {
			var wc = new WebClient();
			wc.OpenReadCompleted += ReadPackage;
			wc.OpenReadAsync(new Uri(packageUrl, UriKind.RelativeOrAbsolute));
		}

		void ReadPackage(object sender, OpenReadCompletedEventArgs e) {
			if (e.Cancelled || e.Error != null) throw e.Error ?? new Exception("Skin download cancelled");

			var zipStream = new ZipInputStream(e.Result);

			ZipEntry file;
			while ((file = zipStream.GetNextEntry()) != null) {
				using (var ms = zipStream.ReadAll()) {
					var ext = file.Name.SubstringAfterLast('.').ToLower();
					switch (ext) {
						case "jpg":
						case "jpeg":
						case "png":
						case "gif":
							StoreImage(file.Name, ms);
							break;

						case "xml":
							StoreXml(ms);
							break;

						default:
							break;
					}
				}
			}
			InvokeSkinReady();
		}

		void StoreXml(MemoryStream ms) {
			using (var reader = new StreamReader(ms))
				XmlContent = reader.ReadToEnd();
		}

		void StoreImage(string name, MemoryStream ms) {
			var img = new BitmapImage();
			img.SetSource(ms);

			var endFolder = name.SubstringBeforeLast('/').SubstringAfterLast('/');
			var fileName = name.SubstringAfterLast('/');
			var safeName = endFolder + "/" + fileName;

			files[safeName] = img;
		}

		public event EventHandler SkinReady;
		public void InvokeSkinReady() {
			EventHandler handler = SkinReady;
			if (handler != null) handler(this, new EventArgs());
		}
	}
}
