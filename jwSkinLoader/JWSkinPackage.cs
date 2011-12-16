using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
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


		public XElement GetComponent (string name) {
			return XElement.Parse(XmlContent)
				.Descendants("component")
				.Where(x => HasAttributeWithValue(x, "name", "display"))
				.FirstOrDefault();
		}

		static bool HasAttributeWithValue(XElement xml, string attribute, string value) { 
			var attrib = xml.Attribute(attribute);
			if (attrib == null) return false;
			return attrib.Value.ToLower() == value; 
		}

		public ImageSource GetNamedElement(string componentName, string elementName) {
			var element = GetComponent(componentName).Elements("elements").Elements("element").
				Where(x=> HasAttributeWithValue(x, "name", elementName))
					.FirstOrDefault();

			if (element == null) return null;
			var src = element.Attribute("src");
			if (src == null) return null;
			return files[componentName +"/" +src.Value] as ImageSource;
		}
	}
}
