using System.IO;

namespace Monkland.SteamManagement
{
    internal class SpearHandler
    {
        public static Spear Read(Spear spear, ref BinaryReader reader)
        {
            spear.stuckInWall = Vector2NHandler.Read(ref reader);
            WeaponHandler.Read(spear, ref reader);
            spear.alwaysStickInWalls = reader.ReadBoolean();
            spear.pinToWallCounter = reader.ReadInt32();
            spear.spearDamageBonus = reader.ReadSingle();
            spear.stuckInObject = DistHandler.ReadPhysicalObject(ref spear.stuckInObject, ref reader, spear.room);
            spear.stuckInWall = Vector2NHandler.Read(ref reader);
            spear.stuckRotation = reader.ReadSingle();
            return spear;
        }

        public static void Write(Spear spear, ref BinaryWriter writer)
        {
            Vector2NHandler.Write(spear.stuckInWall, ref writer);
            WeaponHandler.Write(spear, ref writer);
            writer.Write(spear.alwaysStickInWalls);
            writer.Write(spear.pinToWallCounter);
            writer.Write(spear.spearDamageBonus);
            DistHandler.Write(spear.stuckInObject, ref writer);
            Vector2NHandler.Write(spear.stuckInWall, ref writer);
            writer.Write(spear.stuckRotation);
        }
    }
}
