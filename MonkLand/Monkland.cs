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
            this.Version = VERSION + $".{version}";
            this.ModID = "Monkland";
        }

        public const string VERSION = "0.4";

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

        // Code for AutoUpdate support
        // Should be put in the main PartialityMod class.
        // Comments are optional.

        // Update URL - don't touch!
        // You can go to this in a browser (it's safe), but you might not understand the result.
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/3/3";
        // Version - increase this by 1 when you upload a new version of the mod.
        // The first upload should be with version 0, the next version 1, the next version 2, etc.
        // If you ever lose track of the version you're meant to be using, ask Pastebin.
        public int version = 2;
        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "13Mr+YOzb1iLnJvzkuP4NEZEWwOtWKWvWAN0HdsQ5SF2+RG7k8FbtmQut+2+69ideiJHDW66jWBcGGvfiQ0+5yLAUBpGSckC7V79yZgFQT39lvgU0ykAjonkA+ZTODFnehubyCkrrrzwno4boZghEZmDS2YsSyDJ6RLJyD2/WeCokcTj1vIHZhY9DzkooFtejz9yI/PCZtq8tfq2AzSiQPS+0xGQs3fnAkOGoV1WZ/inW5/rRyjD5HICr8t79UmcopfRK383YBrf2G96HeVYvY2vwSS/BW/m32rTLOZHr+XX7SIZshz7BLK6xEssy4qXjskvAUshqNudxtQnIkShGJuKWF1V2vvwqgY/IZiAbDXdBOUaSd09ldHBlTz9EfzBcgqffVRaUTzS71yGLISyrLriezozlK1YZW9vvijpbD0rmDaJ4aq9s6EzhdgVkTEuChtm/Fj9pgsswjvkbgHw1t9QZWqu4pweNd3IE/Lktst8HBKLiw1aRaffbZIhh1apbyjF8iflD8sNzbIHEfEvc35MEwIFqibJVnVxppBa15HpOxeXOzwuTjFaLSURRvbOEFPmpyd1Nm4nMzZZHHPjQXT7oYQAxjSCfqnLAdYsEnNo/2172jJGLfBWWGFTavqiCYqLhjtYkPfRgpcdw4FldgjX4w7RGMD/Ra5VXvmDMTE=";
        // ------------------------------------------------
    }
}
