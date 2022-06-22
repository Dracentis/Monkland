using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monkland.SteamManagement;

namespace Monkland.Hooks
{
    internal class SaveStateHK
	{
		public static void ApplyHook()
		{
			On.SaveState.LoadGame += new On.SaveState.hook_LoadGame(LoadGameHK);
		}

		public static void LoadGameHK(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
		{
			if (MonklandSteamworks.isInLobby)
			{
				orig(self, "", game);
				self.denPosition = MonklandSteamworks.gameManager.hostShelter;
				return;
            }
			orig(self, str, game);
		}
	}
}
