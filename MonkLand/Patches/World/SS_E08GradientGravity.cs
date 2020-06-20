using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using RWCustom;
using UnityEngine;
using Monkland.SteamManagement;

namespace Monkland.Patches
{
	public class SS_E08GradientGravity : UpdatableAndDeletable
	{
		public SS_E08GradientGravity(Room room)
		{
			this.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < this.room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < this.room.physicalObjects[i].Count; j++)
				{
					if (this.room.physicalObjects[i][j] is Player && (this.room.physicalObjects[i][j] as Player).playerState.playerNumber == 0)
					{
						this.room.gravity = Mathf.InverseLerp(700f, this.room.PixelHeight - 400f, this.room.physicalObjects[i][j].bodyChunks[0].pos.y);
					}
				}
			}
		}
	}
}
