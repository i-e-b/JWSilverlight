using System;
using System.Windows;

namespace jwSkinControls.ControlFragments {
	public class ToggleVisibilityEventArgs : EventArgs {
		public bool IsVisible { get; set; }
		public Visibility Visibility {
			get {
				return (IsVisible) ? (Visibility.Visible) : (Visibility.Collapsed);
			}
		}
	}
}