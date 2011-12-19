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

		public enum ElementType {
			Divider, Button, Text, TimeSlider, VolumeSlider
		}
		public ElementType Type { get; set; }
		public string Name { get; set; } // also used for 'element' in dividers

		public int Width { get; set; } // divider only
	}

	public class ControlBarLayout {
		public ControlBarLayout (JwSkinPackage pkg) {
			elements = new List<ControlBarElement>();

			var component = pkg.GetComponent("controlbar");
			var layout = component.Elements("layout").ToArray();

			if (layout.Length < 1) {
				SetupDefaults();
			} else {
				ReadPlaceholders(layout[0]);
			}

			// todo: prune any elements which don't have an image available, then return.
		}

		void ReadPlaceholders(XElement xElement) {
			// todo: read layout element and fill in ControlBarElements 
			// add left cap
			// if left empty, skip; else add elements;
			// if last element of left is not a divider, add default divider
			// if center empty, skip; else add elements;
			// if last element was not a divider, add default divider
			// if right empty, skip; else add elements;
			// add right cap.
		}

		void SetupDefaults () {
			elements.Add(ControlBarElement.Button("capLeft"));
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
			elements.Add(ControlBarElement.Button("capRight"));
		}

		readonly List<ControlBarElement> elements;
		public IEnumerable<ControlBarElement> Elements { get { return elements; } }
	}
}
