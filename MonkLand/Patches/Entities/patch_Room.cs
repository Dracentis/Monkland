using System;
using System.Collections.Generic;
using System.Text;
using MonoMod;
using Monkland.SteamManagement;
using UnityEngine;
using RWCustom;
using CoralBrain;
using VoidSea;
using ScavTradeInstruction;
using System.Runtime.CompilerServices;

namespace Monkland.Patches
{
    [MonoModPatch("global::Room")]
    class patch_Room : Room
    {
        [MonoModIgnore]
        public patch_Room(RainWorldGame game, World world, AbstractRoom abstractRoom) : base(game, world, abstractRoom)
        {
        }

        public int syncDelay = 50;

        public extern void orig_Update();

        public void Update()
        {
            orig_Update();
            if (MonklandSteamManager.isInGame)
            {
                if (MonklandSteamManager.WorldManager.commonRooms.ContainsKey(this.abstractRoom.name) && this.game.Players[0].realizedObject != null && this.game.Players[0].Room.name == this.abstractRoom.name)
				{
					MonklandSteamManager.EntityManager.Send(this.game.Players[0].realizedObject, MonklandSteamManager.WorldManager.commonRooms[this.abstractRoom.name]);
					for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
					{
						for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
						{
							if (abstractRoom.realizedRoom.physicalObjects[i][j] != null && abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject != null && ((abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == NetworkGameManager.playerID)
							{
								if (abstractRoom.realizedRoom.physicalObjects[i][j] is Rock)
								{
									if ((abstractRoom.realizedRoom.physicalObjects[i][j] as Rock).mode == Weapon.Mode.Thrown)
									{
										MonklandSteamManager.EntityManager.Send(abstractRoom.realizedRoom.physicalObjects[i][j] as Rock, MonklandSteamManager.WorldManager.commonRooms[this.abstractRoom.name]);
									}
									else if (syncDelay == 0)
                                    {
										MonklandSteamManager.EntityManager.Send(abstractRoom.realizedRoom.physicalObjects[i][j] as Rock, MonklandSteamManager.WorldManager.commonRooms[this.abstractRoom.name]);
									}
								}
							}
						}
					}
					if (syncDelay <= 0)
					{
						syncDelay = 50;
					}
					else
					{
						syncDelay--;
					}
                }
			}
        }

		public void Loaded()
		{
			if (this.game == null)
			{
				return;
			}
			if (this.water)
			{
				this.AddWater();
			}
			if (this.abstractRoom.shelter)
			{
				this.shelterDoor = new ShelterDoor(this);
				this.AddObject(this.shelterDoor);
			}
			else if (this.abstractRoom.gate)
			{
				if (this.abstractRoom.name == "GATE_SI_LF" || this.abstractRoom.name == "GATE_HI_CC" || this.abstractRoom.name == "GATE_SI_CC" || this.abstractRoom.name == "GATE_CC_UW" || this.abstractRoom.name == "GATE_UW_SS" || this.abstractRoom.name == "GATE_SH_UW" || this.abstractRoom.name == "GATE_SS_UW")
				{
					this.regionGate = new ElectricGate(this);
					this.AddObject(this.regionGate);
				}
				else
				{
					this.regionGate = new WaterGate(this);
					this.AddObject(this.regionGate);
				}
			}
			List<IntVector2> list = new List<IntVector2>();
			for (int i = 0; i < this.TileWidth; i++)
			{
				for (int j = 0; j < this.TileHeight - 1; j++)
				{
					if (this.GetTile(i, j).Terrain != Room.Tile.TerrainType.Solid && this.GetTile(i, j + 1).Terrain == Room.Tile.TerrainType.Solid && this.GetTile(i, j - 1).Terrain != Room.Tile.TerrainType.Solid && j > this.defaultWaterLevel)
					{
						list.Add(new IntVector2(i, j));
					}
				}
			}
			this.ceilingTiles = list.ToArray();
			if ((!this.abstractRoom.shelter || this.world.brokenShelters[this.abstractRoom.shelterIndex]) && (this.roomSettings.DangerType == RoomRain.DangerType.Rain || this.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain || this.roomSettings.DangerType == RoomRain.DangerType.Flood))
			{
				this.roomRain = new RoomRain(this.game.globalRain, this);
				this.AddObject(this.roomRain);
			}
			for (int k = 0; k < this.roomSettings.effects.Count; k++)
			{
				switch (this.roomSettings.effects[k].type)
				{
					case RoomSettings.RoomEffect.Type.SkyDandelions:
						this.AddObject(new SkyDandelions(this.roomSettings.effects[k], this));
						break;
					case RoomSettings.RoomEffect.Type.Lightning:
					case RoomSettings.RoomEffect.Type.BkgOnlyLightning:
						if (this.lightning == null)
						{
							this.lightning = new Lightning(this, this.roomSettings.effects[k].amount, this.roomSettings.effects[k].type == RoomSettings.RoomEffect.Type.BkgOnlyLightning);
							this.AddObject(this.lightning);
						}
						break;
					case RoomSettings.RoomEffect.Type.GreenSparks:
						this.AddObject(new GreenSparks(this, this.roomSettings.effects[k].amount));
						break;
					case RoomSettings.RoomEffect.Type.VoidMelt:
						this.AddObject(new MeltLights(this.roomSettings.effects[k], this));
						break;
					case RoomSettings.RoomEffect.Type.ZeroG:
					case RoomSettings.RoomEffect.Type.BrokenZeroG:
						{
							bool flag = false;
							int num = 0;
							while (num < this.updateList.Count && !flag)
							{
								if (this.updateList[num] is AntiGravity)
								{
									flag = true;
								}
								num++;
							}
							if (!flag)
							{
								this.AddObject(new AntiGravity(this));
							}
							break;
						}
					case RoomSettings.RoomEffect.Type.SunBlock:
						this.AddObject(new SunBlocker());
						break;
					case RoomSettings.RoomEffect.Type.SSSwarmers:
						{
							bool flag2 = true;
							int num2 = this.updateList.Count - 1;
							while (num2 >= 0 && flag2)
							{
								flag2 = !(this.updateList[num2] is CoralNeuronSystem);
								num2--;
							}
							if (flag2)
							{
								this.AddObject(new CoralNeuronSystem());
							}
							this.waitToEnterAfterFullyLoaded = Math.Max(this.waitToEnterAfterFullyLoaded, 40);
							break;
						}
					case RoomSettings.RoomEffect.Type.SSMusic:
						this.AddObject(new SSMusicTrigger(this.roomSettings.effects[k]));
						break;
					case RoomSettings.RoomEffect.Type.AboveCloudsView:
						this.AddObject(new AboveCloudsView(this, this.roomSettings.effects[k]));
						break;
					case RoomSettings.RoomEffect.Type.RoofTopView:
						this.AddObject(new RoofTopView(this, this.roomSettings.effects[k]));
						break;
					case RoomSettings.RoomEffect.Type.VoidSea:
						this.AddObject(new VoidSeaScene(this));
						break;
					case RoomSettings.RoomEffect.Type.ElectricDeath:
						this.AddObject(new ElectricDeath(this.roomSettings.effects[k], this));
						break;
					case RoomSettings.RoomEffect.Type.VoidSpawn:
						if ((this.game.StoryCharacter != 2 || (this.world.region != null && this.world.region.name == "SB")) && ((this.game.session is StoryGameSession && (this.game.session as StoryGameSession).saveState.theGlow) || this.game.setupValues.playerGlowing))
						{
							this.AddObject(new VoidSpawnKeeper(this, this.roomSettings.effects[k]));
						}
						break;
					case RoomSettings.RoomEffect.Type.BorderPushBack:
						this.AddObject(new RoomBorderPushBack(this));
						break;
					case RoomSettings.RoomEffect.Type.Flies:
					case RoomSettings.RoomEffect.Type.FireFlies:
					case RoomSettings.RoomEffect.Type.TinyDragonFly:
					case RoomSettings.RoomEffect.Type.RockFlea:
					case RoomSettings.RoomEffect.Type.RedSwarmer:
					case RoomSettings.RoomEffect.Type.Ant:
					case RoomSettings.RoomEffect.Type.Beetle:
					case RoomSettings.RoomEffect.Type.WaterGlowworm:
					case RoomSettings.RoomEffect.Type.Wasp:
					case RoomSettings.RoomEffect.Type.Moth:
						if (this.insectCoordinator == null)
						{
							this.insectCoordinator = new InsectCoordinator(this);
							this.AddObject(this.insectCoordinator);
						}
						this.insectCoordinator.AddEffect(this.roomSettings.effects[k]);
						break;
				}
			}
			for (int l = 0; l < this.roomSettings.placedObjects.Count; l++)
			{
				if (this.roomSettings.placedObjects[l].active)
				{
					switch (this.roomSettings.placedObjects[l].type)
					{
						case PlacedObject.Type.LightSource:
							{
								LightSource lightSource = new LightSource(this.roomSettings.placedObjects[l].pos, true, new Color(1f, 1f, 1f), null);
								this.AddObject(lightSource);
								lightSource.setRad = new float?((this.roomSettings.placedObjects[l].data as PlacedObject.LigthSourceData).Rad);
								lightSource.setAlpha = new float?((this.roomSettings.placedObjects[l].data as PlacedObject.LigthSourceData).strength);
								lightSource.fadeWithSun = (this.roomSettings.placedObjects[l].data as PlacedObject.LigthSourceData).fadeWithSun;
								lightSource.colorFromEnvironment = ((this.roomSettings.placedObjects[l].data as PlacedObject.LigthSourceData).colorType == PlacedObject.LigthSourceData.ColorType.Environment);
								lightSource.flat = (this.roomSettings.placedObjects[l].data as PlacedObject.LigthSourceData).flat;
								lightSource.effectColor = Math.Max(-1, (this.roomSettings.placedObjects[l].data as PlacedObject.LigthSourceData).colorType - PlacedObject.LigthSourceData.ColorType.EffectColor1);
								break;
							}
						case PlacedObject.Type.LightFixture:
							switch ((this.roomSettings.placedObjects[l].data as PlacedObject.LightFixtureData).type)
							{
								case PlacedObject.LightFixtureData.Type.RedLight:
									this.AddObject(new Redlight(this, this.roomSettings.placedObjects[l], this.roomSettings.placedObjects[l].data as PlacedObject.LightFixtureData));
									break;
								case PlacedObject.LightFixtureData.Type.HolyFire:
									this.AddObject(new HolyFire(this, this.roomSettings.placedObjects[l], this.roomSettings.placedObjects[l].data as PlacedObject.LightFixtureData));
									break;
								case PlacedObject.LightFixtureData.Type.ZapCoilLight:
									this.AddObject(new ZapCoilLight(this, this.roomSettings.placedObjects[l], this.roomSettings.placedObjects[l].data as PlacedObject.LightFixtureData));
									break;
								case PlacedObject.LightFixtureData.Type.DeepProcessing:
									this.AddObject(new DeepProcessingLight(this, this.roomSettings.placedObjects[l], this.roomSettings.placedObjects[l].data as PlacedObject.LightFixtureData));
									break;
								case PlacedObject.LightFixtureData.Type.SlimeMoldLight:
									this.AddObject(new SlimeMoldLight(this, this.roomSettings.placedObjects[l], this.roomSettings.placedObjects[l].data as PlacedObject.LightFixtureData));
									break;
							}
							break;
						case PlacedObject.Type.CoralStem:
						case PlacedObject.Type.CoralStemWithNeurons:
						case PlacedObject.Type.CoralNeuron:
						case PlacedObject.Type.CoralCircuit:
						case PlacedObject.Type.WallMycelia:
							{
								bool flag3 = true;
								int num3 = this.updateList.Count - 1;
								while (num3 >= 0 && flag3)
								{
									flag3 = !(this.updateList[num3] is CoralNeuronSystem);
									num3--;
								}
								if (flag3)
								{
									this.AddObject(new CoralNeuronSystem());
								}
								this.waitToEnterAfterFullyLoaded = Math.Max(this.waitToEnterAfterFullyLoaded, 80);
								break;
							}
						case PlacedObject.Type.ProjectedStars:
							this.AddObject(new StarMatrix(this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.ZapCoil:
							this.AddObject(new ZapCoil((this.roomSettings.placedObjects[l].data as PlacedObject.GridRectObjectData).Rect, this));
							break;
						case PlacedObject.Type.SuperStructureFuses:
							this.AddObject(new SuperStructureFuses(this.roomSettings.placedObjects[l], (this.roomSettings.placedObjects[l].data as PlacedObject.GridRectObjectData).Rect, this));
							break;
						case PlacedObject.Type.GravityDisruptor:
							this.AddObject(new GravityDisruptor(this.roomSettings.placedObjects[l], this));
							break;
						case PlacedObject.Type.SpotLight:
							this.AddObject(new SpotLight(this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.DeepProcessing:
							this.AddObject(new DeepProcessing(this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.Corruption:
							{
								DaddyCorruption daddyCorruption = null;
								int num4 = this.updateList.Count - 1;
								while (num4 >= 0 && daddyCorruption == null)
								{
									if (this.updateList[num4] is DaddyCorruption)
									{
										daddyCorruption = (this.updateList[num4] as DaddyCorruption);
									}
									num4--;
								}
								if (daddyCorruption == null)
								{
									daddyCorruption = new DaddyCorruption(this);
									this.AddObject(daddyCorruption);
								}
								daddyCorruption.places.Add(this.roomSettings.placedObjects[l]);
								this.waitToEnterAfterFullyLoaded = Math.Max(this.waitToEnterAfterFullyLoaded, 80);
								break;
							}
						case PlacedObject.Type.CorruptionDarkness:
							this.AddObject(new DaddyCorruption.CorruptionDarkness(this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.SSLightRod:
							this.AddObject(new SSLightRod(this.roomSettings.placedObjects[l], this));
							break;
						case PlacedObject.Type.GhostSpot:
							if (this.game.world.worldGhost != null && this.game.world.worldGhost.ghostRoom == this.abstractRoom)
							{
								this.AddObject(new Ghost(this, this.roomSettings.placedObjects[l], this.game.world.worldGhost));
							}
							else if (this.world.region != null)
							{
								int ghostID = (int)GhostWorldPresence.GetGhostID(this.world.region.name);
								if (this.game.session is StoryGameSession && (this.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 0)
								{
									this.AddObject(new GhostHunch(this, ghostID));
								}
							}
							break;
						case PlacedObject.Type.SlimeMold:
							{
								float num5 = this.game.SeededRandom((int)(this.roomSettings.placedObjects[l].pos.x + this.roomSettings.placedObjects[l].pos.y));
								if (num5 > 0.3f)
								{
									this.AddObject(new SlimeMold.CosmeticSlimeMold(this, this.roomSettings.placedObjects[l].pos, Custom.LerpMap(num5, 0.3f, 1f, 30f, 70f), false));
								}
								break;
							}
						case PlacedObject.Type.CosmeticSlimeMold:
							this.AddObject(new SlimeMold.CosmeticSlimeMold(this, this.roomSettings.placedObjects[l].pos, (this.roomSettings.placedObjects[l].data as PlacedObject.ResizableObjectData).Rad, false));
							break;
						case PlacedObject.Type.CosmeticSlimeMold2:
							this.AddObject(new SlimeMold.CosmeticSlimeMold(this, this.roomSettings.placedObjects[l].pos, (this.roomSettings.placedObjects[l].data as PlacedObject.ResizableObjectData).Rad, true));
							break;
						case PlacedObject.Type.SuperJumpInstruction:
							this.AddObject(new SuperJumpInstruction(this, this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.LanternOnStick:
							this.AddObject(new LanternStick(this, this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.ScavengerOutpost:
							this.AddObject(new ScavengerOutpost(this.roomSettings.placedObjects[l], this));
							break;
						case PlacedObject.Type.TradeOutpost:
							this.AddObject(new ScavengerTradeSpot(this, this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.ScavengerTreasury:
							this.AddObject(new ScavengerTreasury(this, this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.ScavTradeInstruction:
							this.AddObject(new ScavengerTradeInstructionTrigger(this, this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.CustomDecal:
							this.AddObject(new CustomDecal(this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.InsectGroup:
							if (this.insectCoordinator == null)
							{
								this.insectCoordinator = new InsectCoordinator(this);
								this.AddObject(this.insectCoordinator);
							}
							this.insectCoordinator.AddGroup(this.roomSettings.placedObjects[l]);
							break;
						case PlacedObject.Type.PlayerPushback:
							this.AddObject(new PlayerPushback(this, this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.MultiplayerItem:
							if (this.game.IsArenaSession)
							{
								this.game.GetArenaGameSession.SpawnItem(this, this.roomSettings.placedObjects[l]);
							}
							break;
						case PlacedObject.Type.GoldToken:
						case PlacedObject.Type.BlueToken:
							if (!(this.game.session is StoryGameSession) || this.world.singleRoomWorld || !(this.game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected((this.roomSettings.placedObjects[l].data as CollectToken.CollectTokenData).tokenString, (this.roomSettings.placedObjects[l].data as CollectToken.CollectTokenData).isBlue))
							{
								this.AddObject(new CollectToken(this, this.roomSettings.placedObjects[l]));
							}
							else
							{
								this.AddObject(new CollectToken.TokenStalk(this, this.roomSettings.placedObjects[l].pos, this.roomSettings.placedObjects[l].pos + (this.roomSettings.placedObjects[l].data as CollectToken.CollectTokenData).handlePos, null, false));
							}
							break;
						case PlacedObject.Type.DeadTokenStalk:
							this.AddObject(new CollectToken.TokenStalk(this, this.roomSettings.placedObjects[l].pos, this.roomSettings.placedObjects[l].pos + (this.roomSettings.placedObjects[l].data as PlacedObject.ResizableObjectData).handlePos, null, false));
							break;
						case PlacedObject.Type.ReliableIggyDirection:
							this.AddObject(new ReliableIggyDirection(this.roomSettings.placedObjects[l]));
							break;
						case PlacedObject.Type.Rainbow:
							if (this.world.rainCycle.CycleStartUp < 1f && (this.game.cameras[0] == null || this.game.cameras[0].ghostMode == 0f) && this.game.SeededRandom(this.world.rainCycle.rainbowSeed + this.abstractRoom.index) < (this.roomSettings.placedObjects[l].data as Rainbow.RainbowData).Chance)
							{
								this.AddObject(new Rainbow(this, this.roomSettings.placedObjects[l]));
							}
							break;
						case PlacedObject.Type.LightBeam:
							this.AddObject(new LightBeam(this.roomSettings.placedObjects[l]));
							break;
					}
				}
			}
			if (this.abstractRoom == null)
			{
				Debug.Log("NULL ABSTRACT ROOM");
			}
			if (this.game.world.worldGhost != null && this.game.world.worldGhost.CreaturesSleepInRoom(this.abstractRoom))
			{
				this.AddObject(new GhostCreatureSedater(this));
			}
			if (this.roomSettings.roomSpecificScript)
			{
				this.AddRoomSpecificScript(this);
			}
			if (this.abstractRoom.firstTimeRealized)
			{
				for (int m = 0; m < this.roomSettings.placedObjects.Count; m++)
				{
					if (this.roomSettings.placedObjects[m].active)
					{
						PlacedObject.Type type = this.roomSettings.placedObjects[m].type;
						switch (type)
						{
							case PlacedObject.Type.DataPearl:
							case PlacedObject.Type.UniqueDataPearl:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new DataPearl.AbstractDataPearl(this.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData, (this.game.StoryCharacter != 1) ? (this.roomSettings.placedObjects[m].data as PlacedObject.DataPearlData).pearlType : DataPearl.AbstractDataPearl.DataPearlType.Misc);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									(abstractPhysicalObject as DataPearl.AbstractDataPearl).hidden = (this.roomSettings.placedObjects[m].data as PlacedObject.DataPearlData).hidden;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.SeedCob:
								{
									AbstractPhysicalObject abstractPhysicalObject = new SeedCob.AbstractSeedCob(this.world, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, false, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
									abstractPhysicalObject.Realize();
									abstractPhysicalObject.realizedObject.PlaceInRoom(this);
									break;
								}
							case PlacedObject.Type.DeadSeedCob:
								{
									AbstractPhysicalObject abstractPhysicalObject = new SeedCob.AbstractSeedCob(this.world, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, true, null);
									this.abstractRoom.entities.Add(abstractPhysicalObject);
									abstractPhysicalObject.Realize();
									abstractPhysicalObject.realizedObject.PlaceInRoom(this);
									break;
								}
							case PlacedObject.Type.WaterNut:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new WaterNut.AbstractWaterNut(this.world, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData, false);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.AddEntity(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.JellyFish:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.JellyFish, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.KarmaFlower:
								if (this.game.StoryCharacter != 2 && (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, true, this.abstractRoom.index, m)))
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.Mushroom:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.SlimeMold:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.FlyLure:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.FlyLure, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
							default:
								switch (type)
								{
									case PlacedObject.Type.NeedleEgg:
										if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
										{
											AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
											(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
											this.abstractRoom.entities.Add(abstractPhysicalObject);
										}
										break;
									default:
										switch (type)
										{
											case PlacedObject.Type.FlareBomb:
												if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
												{
													AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
													(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
													this.abstractRoom.AddEntity(abstractPhysicalObject);
												}
												break;
											case PlacedObject.Type.PuffBall:
												if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
												{
													AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
													(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
													this.abstractRoom.AddEntity(abstractPhysicalObject);
												}
												break;
											case PlacedObject.Type.TempleGuard:
												if (this.game.setupValues.worldCreaturesSpawn)
												{
													AbstractPhysicalObject abstractPhysicalObject = new AbstractCreature(this.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID());
													this.abstractRoom.AddEntity(abstractPhysicalObject);
												}
												break;
											case PlacedObject.Type.DangleFruit:
												if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
												{
													AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
													(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
													this.abstractRoom.entities.Add(abstractPhysicalObject);
												}
												break;
										}
										break;
									case PlacedObject.Type.BubbleGrass:
										if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
										{
											AbstractPhysicalObject abstractPhysicalObject = new BubbleGrass.AbstractBubbleGrass(this.world, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), 1f, this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
											(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
											this.abstractRoom.entities.Add(abstractPhysicalObject);
										}
										break;
									case PlacedObject.Type.Hazer:
									case PlacedObject.Type.DeadHazer:
										if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
										{
											AbstractCreature abstractCreature = new AbstractCreature(this.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Hazer), null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID());
											(abstractCreature.state as VultureGrub.VultureGrubState).origRoom = this.abstractRoom.index;
											(abstractCreature.state as VultureGrub.VultureGrubState).placedObjectIndex = m;
											this.abstractRoom.AddEntity(abstractCreature);
											if (this.roomSettings.placedObjects[m].type == PlacedObject.Type.DeadHazer)
											{
												(abstractCreature.state as VultureGrub.VultureGrubState).Die();
											}
										}
										break;
								}
								break;
							case PlacedObject.Type.FirecrackerPlant:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(this.world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.AddEntity(abstractPhysicalObject);
								}
								break;
							case PlacedObject.Type.VultureGrub:
							case PlacedObject.Type.DeadVultureGrub:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractCreature abstractCreature2 = new AbstractCreature(this.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.VultureGrub), null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID());
									(abstractCreature2.state as VultureGrub.VultureGrubState).origRoom = this.abstractRoom.index;
									(abstractCreature2.state as VultureGrub.VultureGrubState).placedObjectIndex = m;
									this.abstractRoom.AddEntity(abstractCreature2);
									if (this.roomSettings.placedObjects[m].type == PlacedObject.Type.DeadVultureGrub)
									{
										(abstractCreature2.state as VultureGrub.VultureGrubState).Die();
									}
								}
								break;
							case PlacedObject.Type.VoidSpawnEgg:
								if ((this.game.StoryCharacter != 2 || UnityEngine.Random.value < 0.05882353f) && (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m)) && (this.game.setupValues.playerGlowing || (this.game.session is StoryGameSession && (this.game.session as StoryGameSession).saveState.theGlow) || this.world.region.name == "SL"))
								{
									this.AddObject(new VoidSpawnEgg(this, m, this.roomSettings.placedObjects[m]));
								}
								break;
							case PlacedObject.Type.ReliableSpear:
								{
									AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(this.world, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), false);
									this.abstractRoom.entities.Add(abstractPhysicalObject);
									break;
								}
							case PlacedObject.Type.SporePlant:
								if (!(this.game.session is StoryGameSession) || !(this.game.session as StoryGameSession).saveState.ItemConsumed(this.world, false, this.abstractRoom.index, m))
								{
									AbstractPhysicalObject abstractPhysicalObject = new SporePlant.AbstractSporePlant(this.world, null, this.GetWorldCoordinate(this.roomSettings.placedObjects[m].pos), this.game.GetNewID(), this.abstractRoom.index, m, this.roomSettings.placedObjects[m].data as PlacedObject.ConsumableObjectData, false, false);
									(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
									this.abstractRoom.entities.Add(abstractPhysicalObject);
								}
								break;
						}
					}
				}
				if (!this.abstractRoom.shelter && !this.abstractRoom.gate && this.game != null && (!this.game.IsArenaSession || this.game.GetArenaGameSession.GameTypeSetup.levelItems))
				{
					for (int n = (int)((float)this.TileWidth * (float)this.TileHeight * Mathf.Pow(this.roomSettings.RandomItemDensity, 2f) / 5f); n >= 0; n--)
					{
						IntVector2 intVector = this.RandomTile();
						if (!this.GetTile(intVector).Solid)
						{
							bool flag4 = true;
							for (int num6 = -1; num6 < 2; num6++)
							{
								if (!this.GetTile(intVector + new IntVector2(num6, -1)).Solid)
								{
									flag4 = false;
									break;
								}
							}
							if (flag4)
							{
								EntityID newID = this.game.GetNewID(-this.abstractRoom.index);
								AbstractPhysicalObject ent;
								if (UnityEngine.Random.value < ((this.game == null || !this.game.IsStorySession || this.game.StoryCharacter != 2) ? this.roomSettings.RandomItemSpearChance : Mathf.Pow(this.roomSettings.RandomItemSpearChance, 0.85f)))
								{
									ent = new AbstractSpear(this.world, null, new WorldCoordinate(this.abstractRoom.index, intVector.x, intVector.y, -1), newID, this.game != null && this.game.StoryCharacter == 2 && UnityEngine.Random.value < 0.008f);
								}
								else
								{
									ent = new AbstractPhysicalObject(this.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, new WorldCoordinate(this.abstractRoom.index, intVector.x, intVector.y, -1), newID);
								}
								this.abstractRoom.AddEntity(ent);
							}
						}
					}
				}
			}
			this.abstractRoom.firstTimeRealized = false;
			for (int num7 = 0; num7 < this.roomSettings.triggers.Count; num7++)
			{
				if (!(this.game.session is StoryGameSession) || ((this.game.session as StoryGameSession).saveState.cycleNumber >= this.roomSettings.triggers[num7].activeFromCycle && (this.game.StoryCharacter == -1 || this.roomSettings.triggers[num7].slugcats[this.game.StoryCharacter]) && (this.roomSettings.triggers[num7].activeToCycle < 0 || (this.game.session as StoryGameSession).saveState.cycleNumber <= this.roomSettings.triggers[num7].activeToCycle)))
				{
					this.AddObject(new ActiveTriggerChecker(this.roomSettings.triggers[num7]));
				}
			}
			if (this.world.rainCycle.CycleStartUp < 1f && this.roomSettings.CeilingDrips > 0f && this.roomSettings.DangerType != RoomRain.DangerType.None && !this.abstractRoom.shelter)
			{
				this.AddObject(new DrippingSound());
			}
		}

		public void MultiplayerNewToRoom(List<ulong> players)
        {
			if (MonklandSteamManager.isInGame && this.abstractRoom.realizedRoom != null)
			{
				if (this.game.Players[0].realizedObject != null)
					MonklandSteamManager.EntityManager.Send(this.game.Players[0].realizedObject, players);
				for (int i = 0; i < abstractRoom.realizedRoom.physicalObjects.Length; i++)
				{
					for (int j = 0; j < abstractRoom.realizedRoom.physicalObjects[i].Count; j++)
					{
						if (abstractRoom.realizedRoom.physicalObjects[i][j] != null && abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject != null && ((abstractRoom.realizedRoom.physicalObjects[i][j].abstractPhysicalObject as AbstractPhysicalObject) as patch_AbstractPhysicalObject).owner == NetworkGameManager.playerID)
						{
							if (abstractRoom.realizedRoom.physicalObjects[i][j] is Rock)
							{
								MonklandSteamManager.EntityManager.Send(abstractRoom.realizedRoom.physicalObjects[i][j] as Rock, players);
							}
							if (abstractRoom.realizedRoom.physicalObjects[i][j] is Creature)
							{
								foreach (Creature.Grasp grasp in (abstractRoom.realizedRoom.physicalObjects[i][j] as Creature).grasps)
								{
									MonklandSteamManager.EntityManager.SendGrab(grasp);
								}
							}
						}
					}
				}
			}
		}

		public void AddRoomSpecificScript(Room room)
		{
			string name = room.abstractRoom.name;
			switch (name)
			{
				case "SS_E08":
					room.AddObject(new SS_E08GradientGravity(room));
					break;
				case "SU_C04":
					room.AddObject(new RoomSpecificScript.SU_C04StartUp(room));
					break;
				case "SU_A22":
					if (room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.cycleNumber == 1)
					{
						room.AddObject(new RoomSpecificScript.SU_A23FirstCycleMessage(room));
					}
					break;
				case "SL_C12":
					if (room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.regionStates[room.world.region.regionNumber].roomsVisited[room.abstractRoom.index - room.world.firstRoomIndex] && (room.game.session as StoryGameSession).saveState.regionStates[room.world.region.regionNumber].roomsVisited[room.world.GetAbstractRoom("SL_A08").index - room.world.firstRoomIndex])
					{
						room.AddObject(new RoomSpecificScript.SL_C12JetFish(room));
					}
					break;
				case "SB_A14":
					room.AddObject(new RoomSpecificScript.SB_A14KarmaIncrease(room));
					break;
				case "SU_A43":
					room.AddObject(new RoomSpecificScript.SU_A43SuperJumpOnly());
					break;
				case "deathPit":
					room.AddObject(new RoomSpecificScript.DeathPit(room));
					break;
				case "LF_H01":
					if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == 2 && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.GetStorySession.saveState.denPosition == "LF_H01")
					{
						room.AddObject(new HardmodeStart(room));
					}
					break;
				case "LF_A03":
					if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == 2 && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial < 2)
					{
						room.AddObject(new RoomSpecificScript.LF_A03());
					}
					break;
				case "SS_D02":
					if (room.game.IsStorySession && !room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked)
					{
						room.game.GetStorySession.saveState.miscWorldSaveData.memoryArraysFrolicked = true;
						Debug.Log("----MEMORY FROLICK!");
					}
					break;
			}
		}

	}
}
