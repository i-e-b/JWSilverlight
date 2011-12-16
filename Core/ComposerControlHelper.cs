using System;
using System.Collections.Generic;

namespace ComposerCore {
	/// <summary>
	/// This class helps keep connect to players and keep track of them.
	/// Use this unless you need to do anything special.
	/// </summary>
	public class ComposerControlHelper {
		public ComposerControlHelper () {
			PlayerList = new List<IPlayer>();
		}

		/// <summary>
		/// List of players this Control is connected to.
		/// </summary>
		public List<IPlayer> PlayerList;

		/// <summary>
		/// Bind a controller to a player.
		/// This method should be called by the composing app.
		/// </summary>
		public void AddBinding (IPlayer PlayerToControl, IPlayerController Controller) {
			PlayerToControl.AddController(Controller);

			if (PlayerList == null)
				PlayerList = new List<IPlayer>();

			if (!PlayerList.Contains(PlayerToControl)) {
				var new_list = new List<IPlayer>(PlayerList){PlayerToControl};
				PlayerList = new_list;
			}
		}

		/// <summary>
		/// Unbind this controller from a player.
		/// This method should be called by the composing app.
		/// </summary>
		public void RemoveBinding (IPlayer PlayerToControl, IPlayerController Controller) {
			PlayerToControl.RemoveController(Controller);

			if (PlayerList == null)
				PlayerList = new List<IPlayer>();

			if (PlayerList.Contains(PlayerToControl)) {
				var new_list = new List<IPlayer>(PlayerList);
				new_list.Remove(PlayerToControl);
				PlayerList = new_list;
			}
		}

		public void EachPlayer(Action<IPlayer> action) {
			foreach (var p in PlayerList) action(p);
		}
	}
}
