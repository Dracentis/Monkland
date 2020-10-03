using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod;
using UnityEngine;
using Monkland.SteamManagement;
using Steamworks;
using Monkland.UI;
using Menu;
using System.Runtime.CompilerServices;

namespace Monkland.Patches.Menus
{
    [MonoMod.MonoModPatch("global::Menu.EndgameTokens")]
    class patch_EndgameTokens : Menu.EndgameTokens
    {
        [MonoModIgnore]
        public patch_EndgameTokens(Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, KarmaLadder ladder) : base(menu, owner, pos, container, ladder)
        {
        }

        [MonoModIgnore]
        private bool addPassageButtonWhenTokenBecomesVisible;

        [MonoModIgnore]
        private bool pingAchivements;

        [MonoModIgnore]
        public extern void OriginalConstructor(Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, KarmaLadder ladder);
        [MonoModConstructor, MonoModOriginalName("OriginalConstructor")]
        public void ctor_EndgameTokens(Menu.Menu menu, MenuObject owner, Vector2 pos, FContainer container, KarmaLadder ladder)
        {
            //Created Delegate to call Menu.PositionedMenuObject constructor
            Type[] constructorSignature = new Type[3];
            constructorSignature[0] = typeof(Menu.Menu);
            constructorSignature[1] = typeof(Menu.MenuObject);
            constructorSignature[2] = typeof(Vector2);
            RuntimeMethodHandle handle = typeof(Menu.PositionedMenuObject).GetConstructor(constructorSignature).MethodHandle;
            RuntimeHelpers.PrepareMethod(handle);
            IntPtr ptr = handle.GetFunctionPointer();
            Action<Menu.Menu, Menu.MenuObject, Vector2> funct = (Action<Menu.Menu, Menu.MenuObject, Vector2>)Activator.CreateInstance(typeof(Action<Menu.Menu, Menu.MenuObject, Vector2>), this, ptr);
            funct(menu, owner, pos);//Menu.PositionedMenuObject constructor

            //Original Code:
            this.tokens = new List<EndgameTokens.Token>();
            bool flag = false;
            this.addPassageButtonWhenTokenBecomesVisible = false;
            int num = 0;
            for (int i = 0; i < ladder.endGameMeters.Count; i++)
            {
                if (ladder.endGameMeters[i].fullfilledNow)
                {
                    this.addPassageButtonWhenTokenBecomesVisible = true;
                }
                if (ladder.endGameMeters[i].tracker.GoalFullfilled && !ladder.endGameMeters[i].tracker.consumed)
                {
                    if (ladder.endGameMeters[i].tracker.GoalAlreadyFullfilled && !flag)
                    {
                        flag = true;
                    }
                    this.tokens.Add(new EndgameTokens.Token(menu, this, default(Vector2), ladder.endGameMeters[i], container, num));
                    this.subObjects.Add(this.tokens[this.tokens.Count - 1]);
                    num++;
                }
                if (ladder.endGameMeters[i].fullfilledNow)
                {
                    this.forceShowTokenAdd = true;
                }
            }
            // New Code:
            if (menu is SleepAndDeathScreen) //To avoid calling menu as SleepAndDeathScreen when menu is MultiplayerSleepAndDeathScreen
            {
                if ((menu as SleepAndDeathScreen).winState != null)
                {
                    for (int j = 0; j < (menu as SleepAndDeathScreen).winState.endgameTrackers.Count; j++)
                    {
                        if (!(menu as SleepAndDeathScreen).winState.endgameTrackers[j].GoalAlreadyFullfilled && (menu as SleepAndDeathScreen).winState.endgameTrackers[j].GoalFullfilled)
                        {
                            this.pingAchivements = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    (menu as SleepAndDeathScreen).AddPassageButton(false);
                    this.addPassageButtonWhenTokenBecomesVisible = false;
                }
            }
            else
            {
                if ((menu as MultiplayerSleepAndDeathScreen).winState != null)
                {
                    for (int j = 0; j < (menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers.Count; j++)
                    {
                        if (!(menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers[j].GoalAlreadyFullfilled && (menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers[j].GoalFullfilled)
                        {
                            this.pingAchivements = true;
                            break;
                        }
                    }
                }
            }
        }

        public extern void orig_Update();

        public override void Update()
        {
            if (this.menu is SleepAndDeathScreen)
            {
                orig_Update();
            }
            else
            {
                //Created Delegate to call base.Update
                Type[] constructorSignature = new Type[0];
                RuntimeMethodHandle handle = typeof(Menu.PositionedMenuObject).GetMethod("Update").MethodHandle;
                RuntimeHelpers.PrepareMethod(handle);
                IntPtr ptr = handle.GetFunctionPointer();
                Action funct = (Action)Activator.CreateInstance(typeof(Action), this, ptr);
                funct();//PositionedMenuObject.Update

                this.lastBlackFade = this.blackFade;
                if (this.blackFade > 0f)
                {
                    this.blackFade = Mathf.Min(1f, this.blackFade + 0.025f);
                }
                if (this.addPassageButtonWhenTokenBecomesVisible)
                {
                    bool flag = true;
                    for (int i = 0; i < this.tokens.Count; i++)
                    {
                        if (this.tokens[i].endgameMeter.fullfilledNow && this.tokens[i].endgameMeter.showAsFullfilled < 0.9f)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        (this.menu as MultiplayerSleepAndDeathScreen).AddPassageButton(true);
                        this.addPassageButtonWhenTokenBecomesVisible = false;
                    }
                }
                if (this.pingAchivements && this.AllAnimationsDone)
                {
                    this.pingAchivements = false;
                    for (int j = 0; j < (this.menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers.Count; j++)
                    {
                        if (!(this.menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers[j].GoalAlreadyFullfilled && (this.menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers[j].GoalFullfilled)
                        {
                            this.menu.manager.CueAchievement(WinState.PassageAchievementID((this.menu as MultiplayerSleepAndDeathScreen).winState.endgameTrackers[j].ID), 6f);
                        }
                    }
                }
            }
        }

        [MonoModIgnore]
        private float blackFade;

        [MonoModIgnore]
        private float lastBlackFade;
    }
}