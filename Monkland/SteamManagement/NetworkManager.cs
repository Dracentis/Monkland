using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkManager
    {

        public const int GAME_CHANNEL = 0;
        public const int WORLD_CHANNEL = 1;
        public const int PLAYER_CHANNEL = 2;

        public virtual void Reset()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void RegisterHandlers()
        {
        }

        public virtual void PlayerJoined(ulong steamID)
        {
        }

        public virtual void PlayerLeft(ulong steamID)
        {
        }

        public void Log(string message)
        {
            MonklandSteamManager.Log(message);
        }
    }
}