using System.Globalization;
using System.Windows.Media;

namespace ExampleControls {
	public static class Tools {

		public static Color HexToColor (string hex) {
			if (hex.Length != 8) return Colors.Black;
			var r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			var g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			var b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
			return Color.FromArgb(255, r, g, b);
		}

	}
}
