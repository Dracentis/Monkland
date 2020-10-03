using Menu;
using UnityEngine;

namespace Monkland.Hooks.Menus
{
    public static class MainMenuHK
    {
        public static void Patch()
        {
            // EndgameTokensHK.SubPatch(); // These are not needed
            // FoodMeterHK.SubPatch();
            HUDHK.SubPatch();
            RainMeterHK.SubPatch();

            On.Menu.MainMenu.ctor += new On.Menu.MainMenu.hook_ctor(CtorHK);
            On.Menu.MainMenu.Singal += new On.Menu.MainMenu.hook_Singal(SingalHK);
        }

        private static void CtorHK(On.Menu.MainMenu.orig_ctor orig, MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig.Invoke(self, manager, showRegionSpecificBkg);

            // Move these
            SimpleButton sb = null;
            for (int i = 0; i < self.pages[0].subObjects.Count; i++)
            {
                if (self.pages[0].subObjects[i] is SimpleButton b)
                {
                    if (b.signalText == "SINGLE PLAYER") { sb = b; }
                    else { b.pos.y -= 40f; }
                }
            }
            if (sb != null)
            {
                self.pages[0].subObjects.Add(new SimpleButton(self, self.pages[0], "MULTIPLAYER", "COOP", new Vector2(sb.pos.x, sb.pos.y - 40f), sb.size));
            }
        }

        private static void SingalHK(On.Menu.MainMenu.orig_Singal orig, MainMenu self, MenuObject sender, string message)
        {
            if (message == "COOP")
            {
                self.PlaySound(SoundID.MENU_Switch_Page_In);
                ProcessManagerHK.ImmediateSwitchCustom(self.manager, new LobbyFinderMenu(self.manager)); //opens lobby finder menu menu
                return;
            }
            orig.Invoke(self, sender, message);
        }
    }
}