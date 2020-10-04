using Monkland.Hooks;
using Monkland.Hooks.Entities;
using Monkland.Hooks.Menus;
using Monkland.Hooks.OverWorld;
using Monkland.UI;
using Partiality.Modloader;
using UnityEngine;

namespace Monkland
{
    public class Monkland : PartialityMod
    {
        public Monkland()
        {
            this.Version = MonklandUI.VERSION;
            this.ModID = "Monkland";
        }

        // public static Monkland instance;

        public override void OnEnable()
        {
            base.OnEnable();
            ApplyAllHooks();

        }

        public static void ApplyAllHooks()
        {
            Debug.Log("Applying monkland");
            /*
             * Entities
             */
            AbstractPhysicalObjectHK.ApplyHook();
            CreatureHK.ApplyHook();
            PlayerGraphicsHK.ApplyHook();
            PlayerHK.ApplyHook();
            RockHK.ApplyHook();
            RoomHK.ApplyHook();
            SpearHK.ApplyHook();
            WeaponHK.ApplyHook();

            /*
             * Menus
             */
            MainMenuHK.ApplyHook();
            HUDHK.ApplyHook();
            RainMeterHK.ApplyHook();

            /*
             * OverWorld
             */
            AbstractRoomHK.ApplyHook();
            OverWorldHK.ApplyHook();
            ShortcutHandlerHK.ApplyHook();

            /*
             * Other
             */
            ProcessManagerHK.ApplyHook();
            RainWorldGameHK.ApplyHook();
            RainWorldHK.ApplyHook();
        }
    }
}