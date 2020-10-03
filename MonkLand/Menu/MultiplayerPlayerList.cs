using Menu;
using Monkland.SteamManagement;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace Monkland
{
    internal class MultiplayerPlayerList : MultiplayerDisplayMenu
    {
        public HashSet<ulong> playerHash = new HashSet<ulong>();
        public static Dictionary<ulong, MenuLabel> playerLabels = new Dictionary<ulong, MenuLabel>();

        public MultiplayerPlayerList(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, Vector2 displayElementSize, float displayElementFade = 10) : base(menu, owner, pos, size, displayElementSize, displayElementFade)
        {
        }

        public static void RemovePlayerLabel(ulong id)
        {
            if (playerLabels.ContainsKey(id))
            {
                MenuLabel label = playerLabels[id];
                (label.owner as MultiplayerPlayerList).playerHash.Remove(id);
                label.owner.subObjects.Remove(label);
                label.RemoveSprites();
                playerLabels.Remove(id);
            }
        }

        public void ClearList()
        {
            playerLabels.Clear();
        }

        public override void Update()
        {
            base.Update();

            foreach (ulong s in MonklandSteamManager.connectedPlayers)
            {
                if (playerHash.Contains(s))
                    continue;

                MenuLabel newLabel = new MenuLabel(this.menu, this, SteamFriends.GetFriendPersonaName((CSteamID)s), new Vector2(5, 0), new Vector2(this.size.x - 10, 20), false);
                playerHash.Add(s);
                playerLabels.Add(s, newLabel);
                this.subObjects.Add(newLabel);
            }

            //Update stuff
            {
                //The total height in pixels that the players take up on the scroll menu
                float messagePixelHeight = 5 + (playerLabels.Count * 25);
                //The max height the scrollbar can display
                float maxDisplayHeight = this.size.y - 30;
                float maxDisplayTransition = this.size.y - 40;

                float difference = messagePixelHeight - maxDisplayHeight;

                if (difference < 0)
                { scrollValue = 0; }

                int i = 0;

                foreach (KeyValuePair<ulong, MenuLabel> kvp in playerLabels)
                {
                    MenuLabel item = kvp.Value;
                    float actualY = 5.01f + ((playerLabels.Count - i - 1) * 25) - (difference * scrollValue);
                    item.pos = new Vector2(item.pos.x, actualY);

                    float targetAlpha = 1;

                    if (actualY < 10)
                    {
                        targetAlpha = actualY / 10f;
                    }
                    else if (actualY > maxDisplayHeight)
                    {
                        targetAlpha = 0;
                    }
                    else if (actualY > maxDisplayTransition)
                    {
                        targetAlpha = 1 - ((actualY - maxDisplayTransition) / 10f);
                    }
                    else
                    {
                        targetAlpha = 1;
                    }

                    item.label.color = MonklandSteamManager.GameManager.readiedPlayers.Contains(kvp.Key) ? Color.green : Color.red;
                    item.label.alpha = targetAlpha;

                    i++;
                }
            }
        }
    }
}