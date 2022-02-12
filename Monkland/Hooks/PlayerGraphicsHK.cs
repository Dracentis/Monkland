using Monkland.SteamManagement;
using RWCustom;
using UnityEngine;

namespace Monkland.Hooks
{
    internal static class PlayerGraphicsHK
    {
        public static void ApplyHook()
        {
            On.PlayerGraphics.ApplyPalette += new On.PlayerGraphics.hook_ApplyPalette(ApplyPaletteHK);
        }

        private static void ApplyPaletteHK(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            Color body;
            if (!MonklandSteamManager.isInLobby)
            {
                body = PlayerGraphics.SlugcatColor(self.player.playerState.slugcatCharacter);
            }
            else
            {
                body = MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)];
            }
            Color eyes = palette.blackColor;
            if (self.malnourished > 0f)
            {
                float num = (!self.player.Malnourished) ? Mathf.Max(0f, self.malnourished - 0.005f) : self.malnourished;
                body = Color.Lerp(body, Color.gray, 0.4f * num);
                eyes = Color.Lerp(eyes, Color.Lerp(Color.white, palette.fogColor, 0.5f), 0.2f * num * num);
            }
            if (self.player.playerState.slugcatCharacter == 3)
            {
                eyes = Color.Lerp(new Color(1f, 1f, 1f), body, 0.3f);
                body = Color.Lerp(palette.blackColor, Custom.HSL2RGB(0.63055557f, 0.54f, 0.5f), Mathf.Lerp(0.08f, 0.04f, palette.darkness));
            }
            for (int i = 0; i < 12; i++) // Hardcoded sLeaser.sprites.Length to prevent ignoring sprite adding mods
            { sLeaser.sprites[i].color = body; }
            if (MonklandSteamManager.isInLobby)
            {
                sLeaser.sprites[11].color = Color.Lerp(MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)], Color.white, 0.3f);
                sLeaser.sprites[10].color = MonklandSteamManager.GameManager.playerColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)];
                sLeaser.sprites[9].color = MonklandSteamManager.GameManager.playerEyeColors[MonklandSteamManager.connectedPlayers.IndexOf(NetworkGameManager.playerID)];
            }
            else
            {
                sLeaser.sprites[11].color = Color.Lerp(PlayerGraphics.SlugcatColor(self.player.playerState.slugcatCharacter), Color.white, 0.3f);
                sLeaser.sprites[10].color = PlayerGraphics.SlugcatColor(self.player.playerState.slugcatCharacter);
                sLeaser.sprites[9].color = eyes;
            }
        }
    }
}