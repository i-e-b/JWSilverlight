using System;
using System.Windows;
using System.Windows.Controls;

namespace jwSkinControls {
	public partial class PairedImage : UserControl {
		readonly FrameworkElement pairTarget;

		public PairedImage (FrameworkElement pairTarget) {
			InitializeComponent();

			if (pairTarget != null) {
				this.pairTarget = pairTarget;
				this.pairTarget.LayoutUpdated += PairTargetLayoutUpdated;
			}
		}

		void PairTargetLayoutUpdated(object sender, EventArgs e) {
			Visibility = pairTarget.Visibility;
		}

		public Image Image { get { return SelfImage; } set { SelfImage = value; } }

	}
}
