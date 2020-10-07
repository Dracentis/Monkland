using Monkland.Hooks;
using Monkland.Hooks.Entities;
using Monkland.Hooks.Menus;
using Monkland.Hooks.OverWorld;
using Partiality.Modloader;
using UnityEngine;

namespace Monkland
{
    public class Monkland : PartialityMod
    {
        public Monkland()
        {
            this.Version = VERSION;
            this.ModID = "Monkland";
        }

        public const string VERSION = "0.3.1";

        // public static Monkland instance;

        public override void OnEnable()
        {
            base.OnEnable();
            ApplyAllHooks();
        }

        private static void ApplyAllHooks()
        {
            Debug.Log("Applying monkland");

            #region Entities
            AbstractPhysicalObjectHK.ApplyHook();
            CreatureHK.ApplyHook();
            PlayerGraphicsHK.ApplyHook();
            PlayerHK.ApplyHook();
            RockHK.ApplyHook();
            RoomHK.ApplyHook();
            SpearHK.ApplyHook();
            WeaponHK.ApplyHook();
            #endregion Entities

            #region Menus
            MainMenuHK.ApplyHook();
            HUDHK.ApplyHook();
            //RainMeterHK.ApplyHook();
            #endregion Menus

            #region OverWorld
            AbstractRoomHK.ApplyHook();
            OverWorldHK.ApplyHook();
            ShortcutHandlerHK.ApplyHook();
            #endregion OverWorld

            #region Others
            ProcessManagerHK.ApplyHook();
            RainWorldGameHK.ApplyHook();
            RainWorldHK.ApplyHook();
            #endregion Others
        }
    }
}
