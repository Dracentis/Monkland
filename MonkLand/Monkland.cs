using System;
using On;
using Partiality.Modloader;
using UnityEngine;
using Monkland.SteamManagement;

namespace Monkland
{
    public class Monkland : PartialityMod
    {
        public override void Init()
        {
            this.ModID = "Monkland";
            Version = "0.1.9";
            author = "Dracenis";
        }
        public override void OnLoad()
        {
            base.OnLoad();
            On.Room.Update += RoomUpdateHK;
        }

        public void RoomUpdateHK(On.Room.orig_Update orig, Room self)
        {
            orig(self); // same as this.orig_Update();
                        // then all the rest of the code, but using `self` instead of `this`
            if (MonklandSteamManager.isInGame)
            {
                if (MonklandSteamManager.WorldManager.commonRooms.ContainsKey(self.abstractRoom.name) && self.game.Players[0].realizedObject != null && self.game.Players[0].Room.name == self.abstractRoom.name)
                {
                    MonklandSteamManager.EntityManager.Send(self.game.Players[0].realizedObject, MonklandSteamManager.WorldManager.commonRooms[self.abstractRoom.name]);
                }
            }
        }
    }
}
