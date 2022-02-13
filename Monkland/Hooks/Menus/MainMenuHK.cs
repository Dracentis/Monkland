using Menu;
using Monkland.Menus;
using UnityEngine;

namespace Monkland.Hooks.Menus
{
    public static class MainMenuHK
    {
        public static void ApplyHook()
        {
            On.Menu.MainMenu.ctor += new On.Menu.MainMenu.hook_ctor(CtorHK);
            On.Menu.MainMenu.Singal += new On.Menu.MainMenu.hook_Singal(SingalHK);
        }

        private static void CtorHK(On.Menu.MainMenu.orig_ctor orig, MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig(self, manager, showRegionSpecificBkg);

            SimpleButton singleplayerButton = null;

            // Move the buttons on the main menu to make room for the multiplayer button
            for (int i = 0; i < self.pages[0].subObjects.Count; i++)
            {
                if (self.pages[0].subObjects[i] is SimpleButton button)
                {
                    if (button.signalText != "SINGLE PLAYER") { button.pos.y -= 40f; }
                    else { singleplayerButton = button; }
                }
            }
            // Add multiplayer button below singleplayer button
            int index = self.pages[0].subObjects.IndexOf(singleplayerButton) + 1;
            self.pages[0].subObjects.Insert(index, new SimpleButton(self, self.pages[0], "MULTIPLAYER", "COOP", new Vector2(singleplayerButton.pos.x, singleplayerButton.pos.y - 40f), singleplayerButton.size));
        }

        private static void SingalHK(On.Menu.MainMenu.orig_Singal orig, MainMenu self, MenuObject sender, string message)
        {
            // If multplayer button is press open lobby finder menu
            if (message == "COOP")
            {
                self.PlaySound(SoundID.MENU_Switch_Page_In);
                ProcessManagerHK.ImmediateSwitchCustom(self.manager, new LobbyFinderMenu(self.manager)); //opens LobbyFinderMenu menu
                return;
            }
            orig(self, sender, message);
        }
    }
}