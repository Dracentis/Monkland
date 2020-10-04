using UnityEngine;

namespace Monkland.SteamManagement
{
    internal class NetworkManager
    {
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
            Debug.Log(message);
        }
    }
}