using System.Windows.Media.Imaging;

namespace jwSkinLoader {
	public interface IImageHoverControl {
		BitmapImage OverImage { get; set; }
		BitmapImage OutImage { get; set; }
	}
}
