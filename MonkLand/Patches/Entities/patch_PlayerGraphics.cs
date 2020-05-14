using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using RWCustom;
using UnityEngine;
using Monkland.SteamManagement;


namespace Monkland.Patches
{
    [MonoModPatch("global::PlayerGraphics")]
    class patch_PlayerGraphics : PlayerGraphics
    {
        [MonoModIgnore]
        public patch_PlayerGraphics(PhysicalObject ow) : base(ow)
        {
        }

		[MonoModIgnore]
		private Player player;

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color color;
			if (!MonklandSteamManager.isInGame)
			{
				color = PlayerGraphics.SlugcatColor(this.player.playerState.slugcatCharacter);
			}
			else
			{
				color = MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf((this.player.abstractPhysicalObject as patch_AbstractPhysicalObject).owner)];
			}
			Color color2 = palette.blackColor;
			if (this.malnourished > 0f)
			{
				float num = (!this.player.Malnourished) ? Mathf.Max(0f, this.malnourished - 0.005f) : this.malnourished;
				color = Color.Lerp(color, Color.gray, 0.4f * num);
				color2 = Color.Lerp(color2, Color.Lerp(Color.white, palette.fogColor, 0.5f), 0.2f * num * num);
			}
			if (this.player.playerState.slugcatCharacter == 3)
			{
				color2 = Color.Lerp(new Color(1f, 1f, 1f), color, 0.3f);
				color = Color.Lerp(palette.blackColor, Custom.HSL2RGB(0.63055557f, 0.54f, 0.5f), Mathf.Lerp(0.08f, 0.04f, palette.darkness));
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = color;
			}
			if (MonklandSteamManager.isInGame)
			{
				sLeaser.sprites[11].color = Color.Lerp(MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf((this.player.abstractPhysicalObject as patch_AbstractPhysicalObject).owner)], Color.white, 0.3f);
				sLeaser.sprites[10].color = MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf((this.player.abstractPhysicalObject as patch_AbstractPhysicalObject).owner)];
				sLeaser.sprites[9].color = MonklandSteamManager.GameManager.playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf((this.player.abstractPhysicalObject as patch_AbstractPhysicalObject).owner)];
			}
			else
			{
				sLeaser.sprites[11].color = Color.Lerp(PlayerGraphics.SlugcatColor(this.player.playerState.slugcatCharacter), Color.white, 0.3f);
				sLeaser.sprites[10].color = PlayerGraphics.SlugcatColor(this.player.playerState.slugcatCharacter);
				sLeaser.sprites[9].color = color2;
			}
		}
	}
}
