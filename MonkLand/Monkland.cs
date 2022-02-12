using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using Monkland.Hooks;
using Monkland.Hooks.Menus;

namespace Monkland
{
    public class Monkland : PartialityMod
    {
        public const string VERSION = "0.5.1"; // Version number
        public const bool DEVELOPMENT = true; // Is this build for development
        public static Monkland instance; // For future Config Machine support

        public Monkland()
        {
            instance = this;
            ModID = "Monkland";
            Version = VERSION;
            author = "Dracentis, Garrakx, the1whoscreamsiguess, notfood"; // other authors added
        }
        public override void OnEnable()
        {
            base.OnEnable();
            // Hooking is done here

            RainWorldHK.ApplyHook();
            RainWorldGameHK.ApplyHook();
            ProcessManagerHK.ApplyHook();
            SaveStateHK.ApplyHook();

            PlayerGraphicsHK.ApplyHook();

            #region User Interface
            MainMenuHK.ApplyHook();
            #endregion User Interface
        }
    }
}