using RWCustom;
using UnityEngine;

namespace Monkland.UI
{
    public class MUIPlayerTag : MUIHUD
    {
        // private int room; //unused
        // private Vector2 pos;

        private FSprite gradient;
        private AbstractPhysicalObject player;
        private string labelName;
        private FLabel label;
        private Color color;
        private FSprite arrowSprite;
        private float blink;
        private float lastAlpha;
        private float lastBlink;
        private float alpha;
        private int fadeAwayCounter;
        private int counter;
        public bool slatedForDeletion;

        public MUIPlayerTag(AbstractPhysicalObject player, string name, Color color, MultiplayerHUD owner) : base(owner, new Vector2(-1000f, -1000f))
        {
            //MonklandUI.AddMessage($"Added label for player {name}[{AbstractPhysicalObjectHK.GetField(player).owner}]");
            this.player = player;
            this.labelName = name;
            this.gradient = new FSprite("Futile_White", true);
            this.gradient.shader = owner.hud.rainWorld.Shaders["FlatLight"];
            this.owner.hud.fContainers[0].AddChild(this.gradient);
            this.gradient.alpha = 0f;
            this.gradient.x = -1000f;
            this.label = new FLabel("font", labelName);
            this.color = color;
            this.label.color = color;
            this.owner.hud.fContainers[0].AddChild(this.label);
            this.label.alpha = 0f;
            this.label.x = -1000f;
            this.arrowSprite = new FSprite("Multiplayer_Arrow", true);
            this.arrowSprite.color = color;
            this.owner.hud.fContainers[0].AddChild(this.arrowSprite);
            this.arrowSprite.alpha = 0f;
            this.arrowSprite.x = -1000f;
            this.blink = 1f;
        }

        public Player RealizedPlayer
        {
            get
            {
                return this.player.realizedObject as Player;
            }
        }

        public RoomCamera Camera
        {
            get
            {
                return (this.owner.hud.owner as Player).abstractCreature.world.game.cameras[0];
            }
        }

        public override void Update()
        {
            this.lastAlpha = this.alpha;
            this.lastBlink = this.blink;
            this.blink = Mathf.Max(0f, this.blink - 0.0125f);
            if (this.Camera.room == null || this.player.Room != this.Camera.room.abstractRoom || RealizedPlayer == null)
            {
                slatedForDeletion = true;
                return;
            }
            if (this.RealizedPlayer.room == null)
            {
                Vector2? vector = this.Camera.game.shortcuts.OnScreenPositionOfInShortCutCreature(this.Camera.room, RealizedPlayer);
                if (vector != null)
                {
                    this.pos = vector.Value - this.Camera.pos;
                }
            }
            else
            {
                this.pos = Vector2.Lerp(this.RealizedPlayer.bodyChunks[0].pos, this.RealizedPlayer.bodyChunks[1].pos, 0.333333343f) + new Vector2(0f, 60f) - this.Camera.pos;
            }
            this.alpha = Custom.LerpAndTick(this.alpha, Mathf.InverseLerp(80f, 20f, (float)this.fadeAwayCounter), 0.08f, 0.0333333351f);
            /*
            if (this.RealizedPlayer.input[0].x != 0 || this.RealizedPlayer.input[0].y != 0 || this.RealizedPlayer.input[0].jmp || this.RealizedPlayer.input[0].thrw || this.RealizedPlayer.input[0].pckp)
            {
                this.fadeAwayCounter++;
            }
            */
            if (this.counter > 10 && !Custom.DistLess(this.RealizedPlayer.firstChunk.lastPos, this.RealizedPlayer.firstChunk.pos, 3f))
            {
                this.fadeAwayCounter++;
            }
            if (this.fadeAwayCounter > 0)
            {
                this.fadeAwayCounter++;
                if (this.fadeAwayCounter > 240 && this.alpha == 0f && this.lastAlpha == 0f)
                {
                    //this.slatedForDeletion = true;
                }
            }
            else if (this.counter > 200)
            {
                this.fadeAwayCounter++;
            }
            this.counter++;
        }

        public override void Draw(float timeStacker)
        {
            Vector2 vector = this.pos + new Vector2(0.01f, 0.01f);
            float num = (float)this.counter + timeStacker;
            float num2 = Mathf.Pow(Mathf.Lerp(this.lastAlpha, this.alpha, timeStacker), 0.7f);
            this.gradient.x = vector.x;
            this.gradient.y = vector.y + 10f;
            this.gradient.scale = Mathf.Lerp(80f, 110f, num2) / 16f;
            this.gradient.alpha = 0.17f * Mathf.Pow(num2, 2f);
            this.arrowSprite.x = vector.x;
            this.arrowSprite.y = vector.y;
            this.label.x = vector.x;
            this.label.y = vector.y + 20f;
            Color color = this.color;
            if (this.counter % 6 < 2 && this.lastBlink > 0f)
            {
                color = Color.Lerp(color, new Color(1f, 1f, 1f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(this.lastBlink, this.blink, timeStacker)));
            }
            this.label.color = color;
            this.arrowSprite.color = color;
            this.label.alpha = num2;
            this.arrowSprite.alpha = num2;
        }

        public override void ClearSprites()
        {
            this.gradient.RemoveFromContainer();
            this.arrowSprite.RemoveFromContainer();
            this.label.RemoveFromContainer();
        }
    }
}
