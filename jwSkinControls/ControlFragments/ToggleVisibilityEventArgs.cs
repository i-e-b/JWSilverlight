using System;
using System.Windows;

namespace ExampleControls {
	public class ToggleVisibilityEventArgs : EventArgs {
		public bool isVisible { get; set; }
		public Visibility Visibility {
			get {
				return (isVisible) ? (Visibility.Visible) : (Visibility.Collapsed);
			}
		}
	}
}