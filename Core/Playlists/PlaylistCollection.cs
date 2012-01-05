using System.Windows.Browser;
using System.Collections.Specialized;

namespace ComposerCore {

	/// <summary>
	/// This class represents a collection of media items to play.
	/// </summary>    
	[ScriptableType]
	public class PlaylistCollection : ScriptableObservableCollection<IPlaylistItem> {
		/// <summary>
		/// Inserts an item into the collection.
		/// </summary>
		/// <param name="index">Index to insert the item at.</param>
		/// <param name="item">Item to insert.</param>
		protected void InsertItem (int index, PlaylistItem item) {
			item.PropertyChanged += item_PropertyChanged;
			base.InsertItem(index, item);
		}

		/// <summary>
		/// playlist item changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void item_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}
