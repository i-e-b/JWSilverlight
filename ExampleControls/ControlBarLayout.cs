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
			elements.Add(ControlBarElement.Button("playButton"));
			elements.Add(ControlBarElement.Button("pauseButton"));
			elements.Add(ControlBarElement.Button("prevButton"));
			elements.Add(ControlBarElement.Button("nextButton"));
			elements.Add(ControlBarElement.Button("stopButton"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("elapsedText"));
			elements.Add(ControlBarElement.TimeSlider());
			elements.Add(ControlBarElement.Button("durationText"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("blankButton"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("fullscreenButton"));
			elements.Add(ControlBarElement.Button("normalscreenButton"));
			elements.Add(ControlBarElement.Divider("divider"));
			elements.Add(ControlBarElement.Button("muteButton"));
			elements.Add(ControlBarElement.Button("unmuteButton"));
			elements.Add(ControlBarElement.VolumeSlider());
		}

		readonly List<ControlBarElement> elements;
		public IEnumerable<ControlBarElement> Elements { get { return elements; } }
	}
}
