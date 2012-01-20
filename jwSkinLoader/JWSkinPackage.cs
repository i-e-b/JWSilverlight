using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace jwSkinLoader {
	public class JwSkinPackage {
		readonly IsolatedStorageSettings files = IsolatedStorageSettings.ApplicationSettings;
		public string XmlContent {get; private set;}

		public void Load (string packageUrl) {
			var wc = new WebClient();
			wc.OpenReadCompleted += ReadPackage;
			wc.OpenReadAsync(new Uri(packageUrl, UriKind.RelativeOrAbsolute));
		}

		void ReadPackage(object sender, OpenReadCompletedEventArgs e) {
			if (e.Error != null) throw e.Error;
			if (e.Cancelled) throw new Exception("Skin download cancelled");

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
				.Where(x => HasAttributeWithValue(x, "name", name))
				.FirstOrDefault();
		}

		static bool HasAttributeWithValue(XElement xml, string attribute, string value) { 
			var attrib = xml.Attribute(attribute);
			if (attrib == null) return false;
			return attrib.Value.ToLower() == value.ToLower(); 
		}

		public BitmapImage GetNamedElement(string componentName, string elementName) {
			var component = GetComponent(componentName);
			if (component == null) return null;

			var element = component
				.Elements("elements").Elements("element")
				.Where(x => HasAttributeWithValue(x, "name", elementName))
				.FirstOrDefault();

			if (element == null) return null;
			var src = element.Attribute("src");
			if (src == null) return null;
			return files[componentName +"/" +src.Value] as BitmapImage;
		}

		public bool HasNamedElement (string componentName, string elementName) {
			return GetNamedElement(componentName, elementName) != null;
		}

		public void BindAndResize(Image target, string componentName, string elementName) {
			var source = GetNamedElement(componentName, elementName);
			if (source == null) return;
			target.Source = source;
			target.Height = source.PixelHeight;
			target.Width = source.PixelWidth;
		}

		public void BindHoverButton (IImageHoverControl target, string componentName, string outElementName, string overElementName) {
			target.OutImage = GetNamedElement(componentName, outElementName);
			target.OverImage = GetNamedElement(componentName, overElementName);
		}

		/// <summary>
		/// Get a setting for a component
		/// </summary>
		public string GetSettingValue(string componentName, string elementName) {
			var element = GetComponent(componentName)
				.Elements("settings").Elements("setting")
				.Where(x=> HasAttributeWithValue(x, "name", elementName))
				.FirstOrDefault();

			if (element == null) return null;
			var attr = element.Attribute("value");
			if (attr == null) return null;
			return attr.Value;
		}

		/// <summary>
		/// Get a general setting
		/// </summary>
		public string GetSettingValue (string elementName) {
			var element = XElement.Parse(XmlContent).Descendants("settings")
				.Elements("setting")
				.Where(x => HasAttributeWithValue(x, "name", elementName))
				.FirstOrDefault();

			if (element == null) return null;
			var attr = element.Attribute("value");
			if (attr == null) return null;
			return attr.Value;
		}
	}
}
