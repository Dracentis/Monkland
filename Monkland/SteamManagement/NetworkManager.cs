using Steamworks;
using System.IO;

namespace Monkland.SteamManagement
{
    internal class NetworkManager
    {
        protected ulong playerID
        {
            get
            {
                return MonklandSteamworks.playerID;
            }
        }

        protected ulong managerID
        {
            get
            {
                return MonklandSteamworks.managerID;
            }
        }

        protected bool isManager { get { return playerID == managerID; } }

        protected byte handler = 0;
        protected int channel = 0;

        public virtual void Reset()
        {
        }

        public virtual void Update()
        {
        }

        public void RegisterHandlers()
        {
            handler = MonklandSteamworks.instance.RegisterHandler(channel, HandlePackets);
        }

        public virtual void HandlePackets(BinaryReader br, CSteamID sentPlayer)
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
            MonklandSteamworks.Log(message);
        }
    }
}