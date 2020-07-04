using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Monkland.Patches;
using Menu;
using MonoMod;
using UnityEngine;

namespace Monkland.Patches {
    [MonoMod.MonoModPatch("global::Menu.MainMenu")]
    class patch_MainMenu : Menu.MainMenu {

        [MonoMod.MonoModIgnore]
        private extern MenuScene.SceneID BackgroundScene();

        [MonoMod.MonoModIgnore]
        public patch_MainMenu(ProcessManager manager, bool showRegionSpecificBkg) : base(manager, showRegionSpecificBkg) { }
        
        [MonoMod.MonoModIgnore]
        public extern void OriginalConstructor(ProcessManager manager, bool showRegionSpecificBkg);

        [MonoMod.MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_MainMenu(ProcessManager manager, bool showRegionSpecificBkg)
        {
            //Created Delegate to call Menu.Menu constructor
            Type[] constructorSignature = new Type[2];
            constructorSignature[0] = typeof(ProcessManager);
            constructorSignature[1] = typeof(ProcessManager.ProcessID);
            RuntimeMethodHandle handle = typeof(Menu.Menu).GetConstructor(constructorSignature).MethodHandle;
            RuntimeHelpers.PrepareMethod(handle);
            IntPtr ptr = handle.GetFunctionPointer();
            Action<ProcessManager, ProcessManager.ProcessID> funct = (Action<ProcessManager, ProcessManager.ProcessID>)Activator.CreateInstance(typeof(Action<ProcessManager, ProcessManager.ProcessID>), this, ptr);
            funct(manager, ProcessManager.ProcessID.MainMenu);//Menu.Menu constructor


            //Original code:
            bool flag = manager.rainWorld.progression.IsThereASavedGame(0);
            this.pages.Add(new Page(this, null, "main", 0));
            this.scene = new InteractiveMenuScene(this, this.pages[0], (!showRegionSpecificBkg || !flag) ? MenuScene.SceneID.MainMenu : this.BackgroundScene());
            this.pages[0].subObjects.Add(this.scene);
            float num = 0.3f;
            float num2 = 0.5f;
            if (this.scene != null)
            {
                switch (this.scene.sceneID)
                {
                    case MenuScene.SceneID.Landscape_CC:
                        num = 0.65f;
                        num2 = 0.65f;
                        break;
                    case MenuScene.SceneID.Landscape_DS:
                        num = 0.5f;
                        break;
                    case MenuScene.SceneID.Landscape_GW:
                        num = 0.45f;
                        num2 = 0.6f;
                        break;
                    case MenuScene.SceneID.Landscape_LF:
                        num = 0.65f;
                        num2 = 0.4f;
                        break;
                    case MenuScene.SceneID.Landscape_SB:
                        num = 0f;
                        num2 = 0f;
                        break;
                    case MenuScene.SceneID.Landscape_SH:
                        num = 0.2f;
                        num2 = 0.2f;
                        break;
                    case MenuScene.SceneID.Landscape_SI:
                        num = 0.55f;
                        num2 = 0.75f;
                        break;
                    case MenuScene.SceneID.Landscape_SS:
                        num = 0f;
                        num2 = 0f;
                        break;
                    case MenuScene.SceneID.Landscape_SU:
                        num = 0.6f;
                        break;
                    case MenuScene.SceneID.Landscape_UW:
                        num = 0f;
                        num2 = 0f;
                        break;
                }
            }
            if (num2 > 0f)
            {
                this.gradientsContainer = new GradientsContainer(this, this.pages[0], new Vector2(0f, 0f), num2);
                this.pages[0].subObjects.Add(this.gradientsContainer);
                if (num > 0f)
                {
                    this.gradientsContainer.subObjects.Add(new DarkGradient(this, this.gradientsContainer, new Vector2(683f, 580f), 600f, 350f, num));
                }
            }
            float num3 = (base.CurrLang != InGameTranslator.LanguageID.Italian) ? 110f : 150f;
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("SINGLE PLAYER"), "SINGLE PLAYER", new Vector2(683f - num3 / 2f, 370f), new Vector2(num3, 30f)));
            //New Code:
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], "MULTIPLAYER", "COOP", new Vector2(683f - num3 / 2f, 330f), new Vector2(num3, 30f)));//Added Multiplayer Button
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("REGIONS"), "REGIONS", new Vector2(683f - num3 / 2f, 290f), new Vector2(num3, 30f)));//and shifted all other buttons down
            (this.pages[0].subObjects[this.pages[0].subObjects.Count - 1] as SimpleButton).buttonBehav.greyedOut = !manager.rainWorld.progression.miscProgressionData.IsThereAnyDiscoveredShetlers;
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("ARENA"), "ARENA", new Vector2(683f - num3 / 2f, 250f), new Vector2(num3, 30f)));
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("OPTIONS"), "OPTIONS", new Vector2(683f - num3 / 2f, 210f), new Vector2(num3, 30f)));
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("EXIT"), "EXIT", new Vector2(683f - num3 / 2f, 170f), new Vector2(num3, 30f)));
            //Original Code:
            this.pages[0].subObjects.Add(new MenuIllustration(this, this.pages[0], string.Empty, "MainTitleShadow", new Vector2(378f, 440f), true, false));
            this.pages[0].subObjects.Add(new MenuIllustration(this, this.pages[0], string.Empty, "MainTitleBevel", new Vector2(378f, 440f), true, false));
            (this.pages[0].subObjects[this.pages[0].subObjects.Count - 1] as MenuIllustration).sprite.shader = manager.rainWorld.Shaders["MenuText"];
            (this.pages[0].subObjects[this.pages[0].subObjects.Count - 1] as MenuIllustration).sprite.color = new Color(0f, 1f, 1f);
            for (int i = 0; i < this.pages[0].subObjects.Count; i++)
            {
                if (this.pages[0].subObjects[i] is SimpleButton)
                {
                    this.pages[0].subObjects[i].nextSelectable[0] = this.pages[0].subObjects[i];
                    this.pages[0].subObjects[i].nextSelectable[2] = this.pages[0].subObjects[i];
                }
            }
            this.mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
            (this.pages[0].selectables[0] as MenuObject).nextSelectable[1] = (this.pages[0].selectables[this.pages[0].selectables.Count - 1] as MenuObject);
            (this.pages[0].selectables[this.pages[0].selectables.Count - 1] as MenuObject).nextSelectable[3] = (this.pages[0].selectables[0] as MenuObject);
            if (manager.rainWorld.progression.gameTinkeredWith)
            {
                float num4 = 10f;
                for (float num5 = -310f; num5 < 768f; num5 += 25f)
                {
                    while (num4 < 1366f)
                    {
                        this.pages[0].subObjects.Add(new MenuLabel(this, this.pages[0], "The save file or world folder has been tinkered with.", new Vector2(num4, num5), new Vector2(600f, 50f), false));
                        num4 += 320f;
                    }
                    num4 = 0f;
                }
            }
            

        }

        public extern void orig_Singal(MenuObject sender, string message);
        
        public void Singal(MenuObject sender, string message) {

            if (message == "COOP")
            {
                base.PlaySound(SoundID.MENU_Switch_Page_In);
                ((patch_ProcessManager)this.manager).ImmediateSwitchCustom(new LobbyFinderMenu(this.manager));//opens lobby finder menu menu
            }
            else
            {
                orig_Singal(sender, message);//calls orginal
            }
        }

    }
}
