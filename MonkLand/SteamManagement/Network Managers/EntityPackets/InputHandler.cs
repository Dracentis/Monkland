using System.IO;

namespace Monkland.SteamManagement
{
    internal class InputHandler
    {
        public static Player.InputPackage Read(Player.InputPackage input, ref BinaryReader reader)
        {
            input.x = reader.ReadInt32();
            input.y = reader.ReadInt32();
            input.jmp = reader.ReadBoolean();
            input.thrw = reader.ReadBoolean();
            input.pckp = reader.ReadBoolean();
            input.mp = reader.ReadBoolean();
            input.gamePad = reader.ReadBoolean();
            input.crouchToggle = reader.ReadBoolean();
            input.analogueDir = Vector2Handler.Read(ref reader);
            input.downDiagonal = reader.ReadInt32();
            return input;
        }

        public static Player Read(Player player, ref BinaryReader reader)
        {
            for (int i = player.input.Length - 1; i > 0; i--)
            {
                player.input[i] = player.input[i - 1];
            }
            player.input[0].x = reader.ReadInt32();
            player.input[0].y = reader.ReadInt32();
            player.input[0].jmp = reader.ReadBoolean();
            player.input[0].thrw = reader.ReadBoolean();
            player.input[0].pckp = reader.ReadBoolean();
            player.input[0].mp = reader.ReadBoolean();
            player.input[0].gamePad = reader.ReadBoolean();
            player.input[0].crouchToggle = reader.ReadBoolean();
            player.input[0].analogueDir = Vector2Handler.Read(ref reader);
            player.input[0].downDiagonal = reader.ReadInt32();
            return player;
        }

        public static void Write(Player.InputPackage input, ref BinaryWriter writer)
        {
            writer.Write(input.x);
            writer.Write(input.y);
            writer.Write(input.jmp);
            writer.Write(input.thrw);
            writer.Write(input.pckp);
            writer.Write(input.mp);
            writer.Write(input.gamePad);
            writer.Write(input.crouchToggle);
            Vector2Handler.Write(input.analogueDir, ref writer);
            writer.Write(input.downDiagonal);
        }

        public static void Write(Player player, ref BinaryWriter writer)
        {
            writer.Write(player.input[0].x);
            writer.Write(player.input[0].y);
            writer.Write(player.input[0].jmp);
            writer.Write(player.input[0].thrw);
            writer.Write(player.input[0].pckp);
            writer.Write(player.input[0].mp);
            writer.Write(player.input[0].gamePad);
            writer.Write(player.input[0].crouchToggle);
            Vector2Handler.Write(player.input[0].analogueDir, ref writer);
            writer.Write(player.input[0].downDiagonal);
        }
    }
}