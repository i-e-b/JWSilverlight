using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using jwSkinLoader;

namespace ExampleControls {
	public class ControlBarElement {
		public static ControlBarElement Divider (string name) { return new ControlBarElement { Name = name, Type = ElementType.Divider }; }
		public static ControlBarElement Button (string name) { return new ControlBarElement { Name = name, Type = ElementType.Button }; }
		public static ControlBarElement Text (string name) { return new ControlBarElement { Name = name, Type = ElementType.Text }; }
		public static ControlBarElement TimeSlider () { return new ControlBarElement { Type = ElementType.TimeSlider }; }
		public static ControlBarElement VolumeSlider () { return new ControlBarElement { Type = ElementType.VolumeSlider }; }
		public static ControlBarElement Gap (int pixels) { return new ControlBarElement { Type = ElementType.Gap, Width = pixels }; }

		public enum ElementType {
			Divider, Button, Text, TimeSlider, VolumeSlider, Gap
		}
		public ElementType Type { get; set; }
		public string Name { get; set; } // also used for 'element' in dividers

		public int Width { get; set; } // divider only

		public string ElementName () {
			if (Name == "capLeft" || Name == "capRight") return Name;
			return Name + Type;
		}
	}

	public class ControlBarLayout {
		public ControlBarLayout (JwSkinPackage pkg) {
			elements = new List<ControlBarElement>();

			var component = pkg.GetComponent("controlbar");
			var layout = component.Elements("layout").ToArray();

			elements.Add(ControlBarElement.Button("capLeft"));
			if (layout.Length < 1) {
				SetupDefaults();
			} else {
				ReadPlaceholders(layout[0], "left");
				ReadPlaceholders(layout[0], "center");
				ReadPlaceholders(layout[0], "right");
			}
			elements.Add(ControlBarElement.Button("capRight"));

			// if no pause button defined, add one after the play button (if there is one)
			if (!elements.Any(e => (e.Type == ControlBarElement.ElementType.Button) && (e.Name == "pause"))) {
				for (var i = 0; i < elements.Count; i++) {
					if ((elements[i].Type != ControlBarElement.ElementType.Button) || (elements[i].Name != "play")) continue;
					elements.Insert(i, ControlBarElement.Button("pause"));
					break;
				}
			}

			for (var i = 0; i < elements.Count; i++) {
				if (elements[i].Type != ControlBarElement.ElementType.Button) continue;
				
				if (!pkg.HasNamedElement("controlbar", elements[i].ElementName())) {
					elements.RemoveAt(i);
					i--;
				}
			}
		}

		void ReadPlaceholders(XElement layout, string groupName) {
			foreach (var item in GroupNodes(layout, groupName)) {
				var name = item.AttributeValue("name");
				var element = item.AttributeValue("element");
				var width = item.AttributeValue("width");
				switch (item.Name.LocalName.ToLower()) {
					case "button":
						if (name != null) elements.Add(ControlBarElement.Button(name));
						break;

					case "text":
						if (name != null) elements.Add(ControlBarElement.Text(name));
						break;

					case "divider":
						if (width == null) elements.Add(ControlBarElement.Divider(element));
						else elements.Add(ControlBarElement.Gap(int.Parse(width)));
						break;

					case "slider":
						if (name == "time") elements.Add(ControlBarElement.TimeSlider());
						else if (name == "volume") elements.Add(ControlBarElement.VolumeSlider());
						break;
				}
			}
		}

		static IEnumerable<XElement> GroupNodes(XElement layout, string groupName) {
			var e = layout.Elements("group").Where(g=> g.AttributeValue("position") == groupName).FirstOrDefault();
			return (e==null) ? (new XElement[]{}) : (e.Elements());
		}

		void SetupDefaults () {
			elements.Add(ControlBarElement.Button("play"));
			elements.Add(ControlBarElement.Button("pause"));
			elements.Add(ControlBarElement.Button("prev"));
			elements.Add(ControlBarElement.Button("next"));
			elements.Add(ControlBarElement.Button("stop"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("elapsed"));
			elements.Add(ControlBarElement.TimeSlider());
			elements.Add(ControlBarElement.Button("duration"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("blank"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("fullscreen"));
			elements.Add(ControlBarElement.Button("normalscreen"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("mute"));
			elements.Add(ControlBarElement.Button("unmute"));
			elements.Add(ControlBarElement.VolumeSlider());
		}

		readonly List<ControlBarElement> elements;
		public IEnumerable<ControlBarElement> Elements { get { return elements; } }
	}
}
