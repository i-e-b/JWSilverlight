using System.Collections.ObjectModel;
using System.Windows.Browser;

namespace ComposerCore {

	public class ScriptableObservableCollection<T> : ObservableCollection<T> {
		/// <summary>
		/// add item
		/// </summary>
		/// <param name="item">itemt o add</param>
		[ScriptableMember]
		public new void Add (T item) {
			base.Add(item);
		}
		/// <summary>
		/// index item
		/// </summary>
		/// <param name="index">index</param>
		/// <returns>item</returns>
		[ScriptableMember]
		public new T this[int index] {
			get { return base[index]; }
			set { base[index] = value; }
		}
		/// <summary>
		/// number of items in collection
		/// </summary>
		[ScriptableMember]
		public new int Count {
			get { return base.Count; }
		}
		/// <summary>
		/// clear collection
		/// </summary>
		[ScriptableMember]
		public new void Clear () {
			base.Clear();
		}
		/// <summary>
		/// does list contain this item
		/// </summary>
		/// <param name="item">item to find</param>
		/// <returns>item found</returns>
		[ScriptableMember]
		public new bool Contains (T item) {
			return base.Contains(item);
		}
		/// <summary>
		/// return index of this item
		/// </summary>
		/// <param name="item">item to find</param>
		/// <returns>index of this item</returns>
		[ScriptableMember]
		public new int IndexOf (T item) {
			return base.IndexOf(item);
		}
		/// <summary>
		/// insert item into collection
		/// </summary>
		/// <param name="index">index to insert at</param>
		/// <param name="item">item to insert</param>
		[ScriptableMember]
		public new void Insert (int index, T item) {
			base.Insert(index, item);
		}
		/// <summary>
		/// remove item at this index
		/// </summary>
		/// <param name="index">index to remove at</param>
		[ScriptableMember]
		public new void RemoveAt (int index) {
			base.RemoveAt(index);
		}
		/// <summary>
		/// remove item from collection
		/// </summary>
		/// <param name="item">item to remove</param>
		/// <returns>true if found and removed</returns>
		[ScriptableMember]
		public new bool Remove (T item) {
			return base.Remove(item);
		}
	}
}
