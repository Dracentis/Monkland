using Menu;
using System.Collections.Generic;
using UnityEngine;

namespace Monkland.Menus
{
    internal class MultiplayerChat : RectangularMenuObject, Slider.ISliderOwner
    {
        public float scrollValue = 0;
        public RoundedRect backgroundRect;

        public static HashSet<string> newMessages = new HashSet<string>();

        public static List<string> chatStrings = new List<string>();
        public List<MenuLabel> chatMessages = new List<MenuLabel>();

        public VerticalSlider slider;

        public MultiplayerChat(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
        {
            this.ClearMessages();
            backgroundRect = new RoundedRect(menu, owner, pos, size, true);
            this.subObjects.Add(backgroundRect);

            slider = new VerticalSlider(menu, this, "Slider", new Vector2(-20f, 9f), new Vector2(30f, this.size.y - 40f), Slider.SliderID.LevelsListScroll, true);
            this.subObjects.Add(slider);
        }

        public void SliderSetValue(Slider slider, float setValue)
        {
            this.scrollValue = setValue;
        }

        public float ValueOfSlider(Slider slider)
        {
            return scrollValue;
        }

        public void ClearMessages()
        {
            foreach (MenuLabel ml in chatMessages)
            {
                ml.RemoveSprites();
                this.subObjects.Remove(ml);
            }
            newMessages.Clear();
            chatStrings.Clear();
            chatMessages.Clear();
        }

        public static void AddChat(string message)
        {
            newMessages.Add(message);
        }

        public void RemoveMessage(string message)
        {
            if (chatStrings.Contains(message))
            {
                this.subObjects.Remove(chatMessages[chatStrings.IndexOf(message)]);
                chatMessages[chatStrings.IndexOf(message)].RemoveSprites();
                chatMessages.RemoveAt(chatStrings.IndexOf(message));
                chatStrings.Remove(message);
            }
        }

        public override void Update()
        {
            base.Update();

            // Loop through new messages and add them to the chat
            foreach (string ms in newMessages)
            {
                if (chatStrings.Contains(ms))
                {
                    RemoveMessage(ms);
                }

                MenuLabel newLabel = new MenuLabel(this.menu, this, ms, new Vector2(5.01f, 0f), new Vector2(this.size.x - 10f, 20f), false);
                // chatStrings must always correspond with chatMessages
                chatStrings.Add(ms);
                chatMessages.Add(newLabel);
                this.subObjects.Add(newLabel);
            }
            newMessages.Clear();

            #region Update Position
            //The total height in pixels that the players take up on the scroll menu
            float messagePixelHeight = 5 + (chatMessages.Count * 25);
            //The max height the scrollbar can display
            float maxDisplayHeight = this.size.y - 30;
            float maxDisplayTransition = this.size.y - 40;

            float difference = messagePixelHeight - maxDisplayHeight;

            if (difference < 0)
            {
                scrollValue = 0;
            }

            int i = 0;

            foreach (MenuLabel item in chatMessages)
            {
                float actualY = 5.01f + ((chatMessages.Count - i - 1) * 25) - (difference * scrollValue);
                item.pos = new Vector2(Mathf.Round(item.pos.x) + 0.01f, actualY);

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

                item.label.alpha = targetAlpha;

                i++;
            }
            #endregion Update Position
        }
    }
}