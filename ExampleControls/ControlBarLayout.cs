using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using jwSkinLoader;

namespace ExampleControls {
	public class ControlBarElement {
		public enum ElementType {
			Divider, Button, Text, Slider
		}
		public ElementType Type { get; set; }
		public string Name { get; set; } // also used for 'element' in dividers

		public int Width { get; set; } // divider only
	}

	public class ControlBarLayout {
		public ControlBarLayout (JwSkinPackage pkg) {
			left = new List<ControlBarElement>();
			centre = new List<ControlBarElement>();
			right = new List<ControlBarElement>();

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
			// todo: read layout element and fill in ControlBarElements into left, centre and right
		}

		void SetupDefaults() {
			// todo: put all standard elements into their standard containers.
			// standard items with no graphic will be pruned.
		}

		readonly List<ControlBarElement> left;
		readonly List<ControlBarElement> centre;
		readonly List<ControlBarElement> right;

		public IEnumerable<ControlBarElement> Left { get { return left; } }
		public IEnumerable<ControlBarElement> Centre { get { return centre; } }
		public IEnumerable<ControlBarElement> Right { get { return right; } }
	}
}
