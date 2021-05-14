
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

namespace Monkland.Hooks.OverWorld
{


	// Token: 0x02000262 RID: 610
	public class BattleRoomRain : UpdatableAndDeletable, IDrawable
	{
		// Token: 0x06000E40 RID: 3648 RVA: 0x00096624 File Offset: 0x00094824
		public BattleRoomRain(Room rm)
		{
			if (rm.waterObject != null)
			{
				rm.waterObject.fWaterLevel = rm.waterObject.originalWaterLevel + this.flood;
			}
			this.dangerType = DangerType.FloodAndRain;
			if (rm.abstractRoom.shelter)
			{
				this.dangerType = BattleRoomRain.DangerType.Flood;
			}
			this.splashTiles = new List<IntVector2>();
			this.rainReach = new int[rm.TileWidth];
			this.shelterTex = new Texture2D(rm.TileWidth, rm.TileHeight);
			for (int i = 0; i < rm.TileWidth; i++)
			{
				bool flag = true;
				for (int j = rm.TileHeight - 1; j >= 0; j--)
				{
					if (flag && rm.GetTile(i, j).Solid)
					{
						flag = false;
						if (j < rm.TileHeight - 1)
						{
							this.splashTiles.Add(new IntVector2(i, j));
						}
						this.rainReach[i] = j;
					}
					this.shelterTex.SetPixel(i, j, (!flag) ? new Color(0f, 0f, 0f) : new Color(1f, 0f, 0f));
				}
			}
			if (rm.water)
			{
				for (int k = 0; k < rm.TileWidth; k++)
				{
					if (!rm.GetTile(k, rm.defaultWaterLevel).Solid)
					{
						this.shelterTex.SetPixel(k, rm.defaultWaterLevel, (this.shelterTex.GetPixel(k, rm.defaultWaterLevel).r <= 0.5f) ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 1f));
						int num = rm.defaultWaterLevel;
						while (num < rm.TileHeight && (float)num < (float)rm.defaultWaterLevel + 20f)
						{
							if (rm.GetTile(k, num).Solid)
							{
								break;
							}
							this.shelterTex.SetPixel(k, num + 1, (this.shelterTex.GetPixel(k, num + 1).r <= 0.5f) ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 1f));
							num++;
						}
					}
				}
			}
			else
			{
				bool flag2 = false;
				for (int l = 0; l < rm.TileWidth; l++)
				{
					if (!rm.GetTile(l, 0).Solid)
					{
						flag2 = true;
						this.shelterTex.SetPixel(l, 0, (this.shelterTex.GetPixel(l, 0).r <= 0.5f) ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 1f));
					}
				}
				if (flag2)
				{
					rm.deathFallGraphic = new DeathFallGraphic();
					rm.AddObject(rm.deathFallGraphic);
				}
			}
			this.shelterTex.wrapMode = TextureWrapMode.Clamp;
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture("RainMask_" + rm.abstractRoom.name, this.shelterTex);
			this.shelterTex.Apply();
			this.splashes = this.splashTiles.Count * 2 / rm.cameraPositions.Length;
			if (this.intensity > 0f)
			{
				this.lastIntensity = 0f;
			}
			else
			{
				this.lastIntensity = 1f;
			}
			this.bulletDrips = new List<BulletDrip>();
			if (this.dangerType != BattleRoomRain.DangerType.Flood)
			{
				this.normalRainSound = new DisembodiedDynamicSoundLoop(this);
				this.normalRainSound.sound = SoundID.Normal_Rain_LOOP;
				this.normalRainSound.VolumeGroup = 3;
				this.heavyRainSound = new DisembodiedDynamicSoundLoop(this);
				this.heavyRainSound.sound = SoundID.Heavy_Rain_LOOP;
				this.heavyRainSound.VolumeGroup = 3;
			}
			this.deathRainSound = new DisembodiedDynamicSoundLoop(this);
			this.deathRainSound.sound = ((this.dangerType == BattleRoomRain.DangerType.Flood) ? SoundID.Death_Rain_Heard_From_Underground_LOOP : SoundID.Death_Rain_LOOP);
			this.deathRainSound.VolumeGroup = 3;
			this.rumbleSound = new DisembodiedDynamicSoundLoop(this);
			this.rumbleSound.sound = SoundID.Death_Rain_Rumble_LOOP;
			this.rumbleSound.VolumeGroup = 3;
			if (this.dangerType != BattleRoomRain.DangerType.Rain)
			{
				this.floodingSound = new DisembodiedDynamicSoundLoop(this);
				this.floodingSound.sound = SoundID.Flash_Flood_LOOP;
				this.floodingSound.VolumeGroup = ((this.dangerType != BattleRoomRain.DangerType.Flood) ? 0 : 3);
			}
			this.distantDeathRainSound = new DisembodiedDynamicSoundLoop(this);
			this.distantDeathRainSound.sound = ((this.dangerType == BattleRoomRain.DangerType.Flood) ? SoundID.Death_Rain_Approaching_Heard_From_Underground_LOOP : SoundID.Death_Rain_Approaching_LOOP);
			this.SCREENSHAKESOUND = new DisembodiedDynamicSoundLoop(this);
			this.SCREENSHAKESOUND.sound = SoundID.Screen_Shake_LOOP;
		}

		public float RainUnderCeilings
		{
			get
			{
				return Mathf.InverseLerp(0.5f, 1f, this.intensity);
			}
		}

		public float SplashSize
		{
			get
			{
				return Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, this.intensity), 2f);
			}
		}

		public float InsidePushAround
		{
			get
			{
				if (this.dangerType == BattleRoomRain.DangerType.Rain)
				{
					//return this.globalRain.InsidePushAround * this.room.roomSettings.RainIntensity;
				}
				return 0f;
			}
		}

		public float OutsidePushAround
		{
			get
			{
				if (this.dangerType == BattleRoomRain.DangerType.Rain || this.dangerType == BattleRoomRain.DangerType.FloodAndRain)
				{
					//return this.globalRain.OutsidePushAround * this.room.roomSettings.RainIntensity;
				}
				return 0f;
			}
		}

		public float FloodLevel
		{
			get
			{
				if (this.room == null || this.room.waterObject == null)
				{
					return -100f;
				}
				if (this.dangerType == BattleRoomRain.DangerType.Flood || this.dangerType == BattleRoomRain.DangerType.FloodAndRain)
				{
					return this.room.waterObject.originalWaterLevel + this.flood;
				}
				return this.room.waterObject.originalWaterLevel;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);

			this.floodSpeed = Mathf.Min(0.8f, this.floodSpeed + 0.0025f);
			this.flood += this.floodSpeed;

			if (this.dangerType == BattleRoomRain.DangerType.Rain || this.dangerType == BattleRoomRain.DangerType.FloodAndRain)
			{
				this.intensity = Mathf.Lerp(this.intensity, 200f, 0.2f);
			}
			this.intensity = Mathf.Min(this.intensity, this.room.roomSettings.RainIntensity);
			this.visibilitySetter = 0;
			if (this.intensity == 0f && this.lastIntensity > 0f)
			{
				this.visibilitySetter = -1;
			}
			else if (this.intensity > 0f && this.lastIntensity == 0f)
			{
				this.visibilitySetter = 1;
			}
			this.lastIntensity = this.intensity;
			/*
			if (this.globalRain.AnyPushAround)
			{
				this.ThrowAroundObjects();
			}
			*/
			/*
			if (this.bulletDrips.Count < (int)((float)this.room.TileWidth* this.room.roomSettings.RainIntensity))
			{
				this.bulletDrips.Add(new BulletDrip(this));
				this.room.AddObject(this.bulletDrips[this.bulletDrips.Count - 1]);
			}
			else if (this.bulletDrips.Count > (int)((float)this.room.TileWidth *  this.room.roomSettings.RainIntensity))
			{
				this.bulletDrips[0].Destroy();
				this.bulletDrips.RemoveAt(0);
			}
			*/

			if (this.flood > 0f)
			{
				if (this.room.waterObject != null)
				{
					this.room.waterObject.fWaterLevel = Mathf.Lerp(this.room.waterObject.fWaterLevel, this.FloodLevel, 0.2f);
					this.room.waterObject.GeneralUpsetSurface(Mathf.InverseLerp(0f, 0.5f, 200f) * 4f);
				}
				else
				{
					this.room.AddWater();
				}
			}
			if (this.dangerType != BattleRoomRain.DangerType.Flood)
			{
				this.normalRainSound.Volume = ((this.intensity <= 0f) ? 0f : (0.1f + 0.9f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0.001f, 0.7f, this.intensity) * 3.14159274f)), 1.5f)));
				this.normalRainSound.Update();
				this.heavyRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.12f, 0.1f, this.intensity), 0.85f) * Mathf.Pow(1f - this.deathRainSound.Volume, 0.3f);
				this.heavyRainSound.Update();
			}
			this.deathRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.35f, 0.75f, this.intensity), 0.8f);
			this.deathRainSound.Update();
			this.rumbleSound.Volume = 0.2f * this.room.roomSettings.RumbleIntensity;
			this.rumbleSound.Update();
			this.distantDeathRainSound.Volume = Mathf.InverseLerp(1400f, 0f, (float)this.room.world.rainCycle.TimeUntilRain) * this.room.roomSettings.RainIntensity;
			this.distantDeathRainSound.Update();
			if (this.dangerType != BattleRoomRain.DangerType.Rain)
			{
				this.floodingSound.Volume = Mathf.InverseLerp(0.01f, 0.1f, this.floodSpeed);
				this.floodingSound.Update();
			}
			if (this.room.game.cameras[0].room == this.room)
			{
				this.SCREENSHAKESOUND.Volume = this.room.game.cameras[0].ScreenShake * (1f - this.rumbleSound.Volume);
			}
			else
			{
				this.SCREENSHAKESOUND.Volume = 0f;
			}
			this.SCREENSHAKESOUND.Update();
		}

		// Token: 0x06000E48 RID: 3656 RVA: 0x00097078 File Offset: 0x00095278
		public void ThrowAroundObjects()
		{
			if (this.room.roomSettings.RainIntensity == 0f)
			{
				return;
			}
			for (int i = 0; i < this.room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < this.room.physicalObjects[i].Count; j++)
				{
					for (int k = 0; k < this.room.physicalObjects[i][j].bodyChunks.Length; k++)
					{
						BodyChunk bodyChunk = this.room.physicalObjects[i][j].bodyChunks[k];
						IntVector2 tilePosition = this.room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, Random.value), Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, Random.value)));
						float num = this.InsidePushAround;
						bool flag = false;
						if (this.rainReach[Custom.IntClamp(tilePosition.x, 0, this.room.TileWidth - 1)] < tilePosition.y)
						{
							flag = true;
							num = Mathf.Max(this.OutsidePushAround, this.InsidePushAround);
						}
						if (this.room.water)
						{
							num *= Mathf.InverseLerp(this.room.FloatWaterLevel(bodyChunk.pos.x) - 100f, this.room.FloatWaterLevel(bodyChunk.pos.x), bodyChunk.pos.y);
						}
						if (num > 0f)
						{
							if (bodyChunk.ContactPoint.y < 0)
							{
								int num2 = 0;
								if (this.rainReach[Custom.IntClamp(tilePosition.x - 1, 0, this.room.TileWidth - 1)] >= tilePosition.y && !this.room.GetTile(tilePosition + new IntVector2(-1, 0)).Solid)
								{
									num2--;
								}
								if (this.rainReach[Custom.IntClamp(tilePosition.x + 1, 0, this.room.TileWidth - 1)] >= tilePosition.y && !this.room.GetTile(tilePosition + new IntVector2(1, 0)).Solid)
								{
									num2++;
								}
								bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(-30f, 30f, Random.value) + (float)(num2 * 16)) * Random.value * ((!flag) ? 4f : 9f) * num / bodyChunk.mass;
							}
							else
							{
								BodyChunk bodyChunk2 = bodyChunk;
								bodyChunk2.vel.y = bodyChunk2.vel.y - Mathf.Pow(Random.value, 5f) * 16.5f * num / bodyChunk.mass;
							}
							if (bodyChunk.owner is Creature)
							{
								if (Mathf.Pow(Random.value, 1.2f) * 2f * (float)bodyChunk.owner.bodyChunks.Length < num)
								{
									(bodyChunk.owner as Creature).Stun(Random.Range(1, 1 + (int)(9f * num)));
								}
								if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
								{
									(bodyChunk.owner as Creature).rainDeath += num / 20f;
								}
								if (num > 0.5f && (bodyChunk.owner as Creature).rainDeath > 1f && Random.value < 0.025f)
								{
									(bodyChunk.owner as Creature).Die();
								}
							}
							bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(90f, 270f, Random.value)) * Random.value * 5f * this.InsidePushAround;
						}
					}
				}
			}
		}

		// Token: 0x06000E49 RID: 3657 RVA: 0x0009747C File Offset: 0x0009567C
		public void CreatureSmashedInGround(Creature crit, float speed)
		{
			if (speed < 2.5f)
			{
				return;
			}
			float from = this.InsidePushAround;
			BodyChunk bodyChunk = crit.bodyChunks[Random.Range(0, crit.bodyChunks.Length)];
			IntVector2 tilePosition = this.room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, Random.value), Mathf.Lerp(-bodyChunk.rad, bodyChunk.rad, Random.value)));
			if (this.rainReach[Custom.IntClamp(tilePosition.x, 0, this.room.TileWidth - 1)] < tilePosition.y)
			{
				from = Mathf.Max(this.OutsidePushAround, this.InsidePushAround);
			}
			crit.rainDeath += Mathf.InverseLerp(-2.5f, -15f, speed) * Mathf.Lerp(from, 1f, 0.5f) * 0.65f / (float)bodyChunk.owner.bodyChunks.Length;
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x00097578 File Offset: 0x00095778
		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1 + this.splashes];
			sLeaser.sprites[0] = new FSprite("RainMask_" + rCam.room.abstractRoom.name, true);
			sLeaser.sprites[0].scaleX = this.room.game.rainWorld.options.ScreenSize.x / (float)this.shelterTex.width;
			sLeaser.sprites[0].scaleY = 768f / (float)this.shelterTex.height;
			sLeaser.sprites[0].anchorX = 0f;
			sLeaser.sprites[0].anchorY = 0f;
			sLeaser.sprites[0].shader = this.room.game.rainWorld.Shaders["DeathRain"];
			for (int i = 1; i < this.splashes + 1; i++)
			{
				sLeaser.sprites[i] = new FSprite((i >= this.splashes / 2) ? "TallSplash" : "90DegreeSplash", true);
				sLeaser.sprites[i].anchorY = ((i >= this.splashes / 2) ? 0.1f : 0.2f);
			}
			this.AddToContainer(sLeaser, rCam, null);
		}

		// Token: 0x06000E4B RID: 3659 RVA: 0x000976D0 File Offset: 0x000958D0
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Shader.SetGlobalFloat("_rainDirection", 1f);
			Shader.SetGlobalVector("_RainSpriteRect", new Vector4(camPos.x / (20f * (float)this.room.TileWidth), camPos.y / (20f * (float)this.room.TileHeight), 1366f / (20f * (float)this.room.TileWidth), 768f / (20f * (float)this.room.TileHeight)));
			Shader.SetGlobalFloat("_rainIntensity", Mathf.InverseLerp(0f, 0.5f, this.intensity));
			if (this.dangerType == BattleRoomRain.DangerType.Rain )
			{
				Shader.SetGlobalFloat("_rainEverywhere", Mathf.Max(0.1f, this.RainUnderCeilings));
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = (this.intensity > 0f);
			}
			sLeaser.sprites[0].shader = ((this.intensity <= 0f) ? this.room.game.rainWorld.Shaders["Basic"] : this.room.game.rainWorld.Shaders["DeathRain"]);
			if (this.intensity == 0f)
			{
				return;
			}
			for (int j = 1; j < this.splashes; j++)
			{
				sLeaser.sprites[j].isVisible = false;
				for (int k = 0; k < 5; k++)
				{
					Vector2 testPos = this.room.MiddleOfTile(this.splashTiles[Random.Range(0, this.splashTiles.Count)]);
					if (testPos.y > this.room.FloatWaterLevel(testPos.x) && rCam.IsViewedByCameraPosition(rCam.currentCameraPosition, testPos))
					{
						sLeaser.sprites[j].y = testPos.y + 10f - camPos.y;
						sLeaser.sprites[j].x = testPos.x + Mathf.Lerp(-10f, 10f, Random.value) - camPos.x;
						sLeaser.sprites[j].isVisible = true;
						break;
					}
				}
				sLeaser.sprites[j].scaleY = this.SplashSize;
				if (j < this.splashes / 2)
				{
					sLeaser.sprites[j].rotation = Mathf.Lerp(-45f, 45f, Random.value);
					sLeaser.sprites[j].scaleX = ((Random.value >= 0.5f) ? 1f : -1f) * this.SplashSize;
				}
				else
				{
					sLeaser.sprites[j].rotation = Mathf.Lerp(-25f, 25f, Random.value);
					sLeaser.sprites[j].scaleX = Mathf.Lerp(-1f, 1f, Random.value) * this.SplashSize;
				}
				sLeaser.sprites[j].color = Color.Lerp(this.pal.fogColor, new Color(1f, 1f, 1f), Random.value * this.intensity);
			}
			if (base.slatedForDeletetion || this.room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x00097A61 File Offset: 0x00095C61
		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			this.pal = palette;
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x00097A6C File Offset: 0x00095C6C
		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
			newContatiner = rCam.ReturnFContainer("Items");
			for (int i = 1; i < this.splashes; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}

		// Token: 0x04000C70 RID: 3184
		//public GlobalRain globalRain;

		// Token: 0x04000C71 RID: 3185
		public RoomPalette pal;

		// Token: 0x04000C72 RID: 3186
		public List<IntVector2> splashTiles;

		// Token: 0x04000C73 RID: 3187
		public int[] rainReach;

		// Token: 0x04000C74 RID: 3188
		public int splashes;

		// Token: 0x04000C75 RID: 3189
		public Texture2D shelterTex;

		// Token: 0x04000C76 RID: 3190
		public float lastIntensity;

		// Token: 0x04000C77 RID: 3191
		public float intensity;

		// Token: 0x04000C78 RID: 3192
		public int visibilitySetter;

		// Token: 0x04000C79 RID: 3193
		public List<BulletDrip> bulletDrips;

		// Token: 0x04000C7A RID: 3194
		public DisembodiedDynamicSoundLoop normalRainSound;

		// Token: 0x04000C7B RID: 3195
		public DisembodiedDynamicSoundLoop heavyRainSound;

		// Token: 0x04000C7C RID: 3196
		public DisembodiedDynamicSoundLoop deathRainSound;

		// Token: 0x04000C7D RID: 3197
		public DisembodiedDynamicSoundLoop rumbleSound;

		// Token: 0x04000C7E RID: 3198
		public DisembodiedDynamicSoundLoop floodingSound;

		// Token: 0x04000C7F RID: 3199
		public DisembodiedDynamicSoundLoop distantDeathRainSound;

		// Token: 0x04000C80 RID: 3200
		public DisembodiedDynamicSoundLoop SCREENSHAKESOUND;

		// Token: 0x04000C81 RID: 3201
		public BattleRoomRain.DangerType dangerType;
		private float flood;
		private float floodSpeed;

		// Token: 0x02000263 RID: 611
		public enum DangerType
		{
			// Token: 0x04000C83 RID: 3203
			Rain,
			// Token: 0x04000C84 RID: 3204
			Flood,
			// Token: 0x04000C85 RID: 3205
			FloodAndRain,
			// Token: 0x04000C86 RID: 3206
			None,
			// Token: 0x04000C87 RID: 3207
			Thunder
		}
	}

}
