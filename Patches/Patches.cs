using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;
using Pixelplacement.TweenSystem;
using SawyerExpansion.ExtendClasses;
using SawyerExpansion.Singleton;
using SawyerExpansion.Utils;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


namespace SawyerExpansion.Patches
{
    public static class Patches
    {
        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.CanPlay))]
        internal class IsThereEnoughResources
        {
            public static bool Prefix(ref PlayableCard __instance, ref bool __result)
            {
                bool ores =true;
                if (__instance.Info.ConvertToSCI().OreCost.Item1 != Enums.OresEnum.None)
                    ores = __instance.Info.ConvertToSCI().OreCost.Item2 <=
                           Singleton<SawyerResourceManager>.Instance.PlayerOres[__instance.Info.ConvertToSCI().OreCost.Item1];
                __result = __instance.Info.BloodCost <= Singleton<BoardManager>.Instance.AvailableSacrificeValue &&
                           __instance.Info.BonesCost <= Singleton<ResourcesManager>.Instance.PlayerBones &&
                           __instance.EnergyCost <= Singleton<ResourcesManager>.Instance.PlayerEnergy &&
                           __instance.Info.ConvertToSCI().HeatCost <= Singleton<SawyerResourceManager>.Instance.PlayerHeat - 1 &&
                           ores &&
                           __instance.GemsCostRequirementMet() &&
                           Singleton<BoardManager>.Instance.SacrificesCreateRoomForCard(__instance,
                               Singleton<BoardManager>.Instance.PlayerSlotsCopy);
                return false;
            }
        }
        
        [HarmonyPatch(typeof(HintsHandler), nameof(HintsHandler.OnNonplayableCardClicked))]
        internal class NotEnoughOfResource
        {
            public static bool Prefix(PlayableCard card, List<PlayableCard> cardsInHand)
            {
                if (card.Info.ConvertToSCI().HeatCost > Singleton<SawyerResourceManager>.Instance.PlayerHeat - 1)
                {
                    card.StartCoroutine(Singleton<TextBox>.Instance.ShowUntilInput(
                        "You do not have enough Heat to play " + card.Info.DisplayedNameLocalized +
                        ". Gain Heat by burning cards in hand. It costs " + card.Info.ConvertToSCI().HeatCost + " but you have " +
                        (Singleton<SawyerResourceManager>.Instance.PlayerHeat - 1), TextBox.Style.Magic));
                    return false;
                }
                if (card.Info.ConvertToSCI().OreCost.Item2 > Singleton<SawyerResourceManager>.Instance.PlayerOres[card.Info.ConvertToSCI().OreCost.Item1])
                {
                    card.StartCoroutine(Singleton<TextBox>.Instance.ShowUntilInput(
                        "You do not have enough Ores to play " + card.Info.DisplayedNameLocalized +
                        ". Gain Ores by placing other cards on board and waiting them to mine them. It costs " + card.Info.ConvertToSCI().OreCost.Item2 +" of type "+ card.Info.ConvertToSCI().OreCost.Item1 + " but you have " +
                        (Singleton<SawyerResourceManager>.Instance.PlayerOres[card.Info.ConvertToSCI().OreCost.Item1]), TextBox.Style.Magic));
                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.GetCostSpriteForCard))]
        internal class MakeCostRender
        {
            public static bool Prefix(ref CardDisplayer __instance, ref Sprite __result, CardInfo card)
            {
                if (card.ConvertToSCI().HeatCost > 0 && ImageUtils.heatTextures != null && ImageUtils.heatTextures.Length != 0)
                {
	                Plugin.PluginDetails.Log.LogInfo(card.ConvertToSCI().HeatCost);
                    Plugin.PluginDetails.Log.LogInfo(ImageUtils.heatTextures[card.ConvertToSCI().HeatCost]);
                    __result = ImageUtils.heatTextures[card.ConvertToSCI().HeatCost];
                }

                if (card.BonesCost > 0)
                {
                    __result = __instance.boneCostTextures[card.BonesCost - 1];
                }

                if (card.GemsCost.Count > 0 && __instance.gemCostTextures != null &&
                    __instance.gemCostTextures.Length != 0)
                {
                    int num;
                    if (card.GemsCost.Count == 1)
                    {
                        num = (int) card.GemsCost[0];
                    }
                    else if (card.GemsCost.Count == 2)
                    {
                        if (card.GemsCost.Contains(GemType.Green) && card.GemsCost.Contains(GemType.Orange))
                        {
                            num = 3;
                        }
                        else if (card.GemsCost.Contains(GemType.Orange) && card.GemsCost.Contains(GemType.Blue))
                        {
                            num = 4;
                        }
                        else
                        {
                            num = 5;
                        }
                    }
                    else
                    {
                        num = 6;
                    }

                    __result = __instance.gemCostTextures[num];
                }

                if (card.EnergyCost > 0 && __instance.energyCostTextures != null &&
                    __instance.energyCostTextures.Length != 0)
                {
                    __result = __instance.energyCostTextures[card.EnergyCost - 1];
                }

                if (card.BloodCost > 0)
                {
                    __result = __instance.costTextures[card.BloodCost];
                }

                if (__result == null)
                    __result = default(Sprite);
                if (__result != null || __result != default(Sprite))
                {
                }

                return false;
            }
        }
        
        [HarmonyPatch(typeof(PlayerHand), nameof(PlayerHand.SelectSlotForCard))]
        internal class PayForACard
		{
			public static void Prefix(ref PlayerHand __instance, out PlayerHand __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator enumerator, PlayerHand __state, PlayableCard card)
			{
				__state.CardsInHand.ForEach(delegate(PlayableCard x) { x.SetEnabled(false); });
				yield return new WaitWhile(() => __state.ChoosingSlot);
				__state.OnSelectSlotStartedForCard(card);
				if (Singleton<RuleBookController>.Instance != null)
				{
					Singleton<RuleBookController>.Instance.SetShown(false, true);
				}

				Singleton<BoardManager>.Instance.CancelledSacrifice = false;
				__state.choosingSlotCard = card;
				if (card != null && card.Anim != null)
				{
					card.Anim.SetSelectedToPlay(true);
				}

				Singleton<BoardManager>.Instance.ShowCardNearBoard(card, true);
				if (Singleton<TurnManager>.Instance.SpecialSequencer != null)
				{
					yield return Singleton<TurnManager>.Instance.SpecialSequencer.CardSelectedFromHand(card);
				}

				bool cardWasPlayed = false;
				bool requiresSacrifices = card.Info.BloodCost > 0;
				if (requiresSacrifices)
				{
					List<CardSlot> validSlots =
						Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll((CardSlot x) => x.Card != null);
					yield return Singleton<BoardManager>.Instance.ChooseSacrificesForCard(validSlots, card);
				}

				if (!Singleton<BoardManager>.Instance.CancelledSacrifice)
				{
					List<CardSlot> validSlots2 =
						Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll((CardSlot x) => x.Card == null);
					yield return Singleton<BoardManager>.Instance.ChooseSlot(validSlots2, !requiresSacrifices);
					CardSlot lastSelectedSlot = Singleton<BoardManager>.Instance.LastSelectedSlot;
					if (lastSelectedSlot != null)
					{
						cardWasPlayed = true;
						card.Anim.SetSelectedToPlay(false);
						yield return __state.PlayCardOnSlot(card, lastSelectedSlot);
						if (card.Info.BonesCost > 0)
						{
							yield return Singleton<ResourcesManager>.Instance.SpendBones(card.Info.BonesCost);
						}

						if (card.EnergyCost > 0)
						{
							yield return Singleton<ResourcesManager>.Instance.SpendEnergy(card.EnergyCost);
						}

						if (card.Info.ConvertToSCI().HeatCost > 0)
						{
							yield return Singleton<SawyerResourceManager>.Instance.SpendHeat(card.Info.ConvertToSCI().HeatCost);
						}
						
						if (card.Info.ConvertToSCI().OreCost.Item1 !=Enums.OresEnum.None)
						{
							yield return Singleton<SawyerResourceManager>.Instance.SpendOre(card.Info.ConvertToSCI().OreCost.Item2, card.Info.ConvertToSCI().OreCost.Item1);
						}
					}
				}

				if (!cardWasPlayed)
				{
					Singleton<BoardManager>.Instance.ShowCardNearBoard(card, false);
				}

				__state.choosingSlotCard = null;
				if (card != null && card.Anim != null)
				{
					card.Anim.SetSelectedToPlay(false);
				}

				__state.CardsInHand.ForEach(delegate(PlayableCard x) { x.SetEnabled(true); });
				yield break;
			}
		}
        
        [HarmonyPatch(typeof(PixelPlayableCard), nameof(PixelPlayableCard.OnCursorEnter))]
        internal class RMBToBurn
        {
	        public static void Postfix(ref PixelPlayableCard __instance)
	        {
		        if (!__instance.gameObject.GetComponent<ClassesWithInstances.BurnTheCard>())
		        {
			        var interactable = __instance.gameObject.AddComponent<ClassesWithInstances.BurnTheCard>();
			        interactable.coll2D = __instance.gameObject.GetComponent<BoxCollider2D>();
		        }
	        }
        }
        
        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.PlayerTurn))]
        internal class GainOre
        {
	        public static void Postfix(ref TurnManager __instance)
	        {
		        foreach (var card in Singleton<BoardManager>.Instance.CardsOnBoard.FindAll(card => !card.OpponentCard))
		        {
			        var typeofore=(Enums.OresEnum)Enum.GetValues(typeof(Enums.OresEnum)).GetValue(Mathf.Clamp(card.Attack+SeededRandom.Range(-1,0,Environment.TickCount),1,4));
			        if(card.Attack>0)
				        card.StartCoroutine(Singleton<SawyerResourceManager>.Instance.AddOre(1, typeofore));
		        }
	        }
        }
        
        		public class TowerEntrance
		{
			
			private static Scene thenextscene;
		
			private static Scene startingislandscene;

			
			[HarmonyPatch(typeof(OverworldPlayerController), nameof(OverworldPlayerController.Start))]
			public class InstantiateTheTowerWhenThePlayerIsCreated
			{
				public static void Postfix()
				{
					if (SceneManager.GetActiveScene() != thenextscene)
					{
						var Sigo = GameObject.Find("Map/NavigationGrid/StartingIsland");
						var zone = GameObject.Instantiate(Sigo);
						zone.layer = 17;
						zone.transform.position = new Vector3(1.942f, -0.668f, 0);
						var ZoneEntrance = zone.gameObject.GetComponent<ZoneEntrance>();
						var NavGridZone = zone.gameObject.GetComponent<NavigationZone2D>();
						var navGridGameObject = GameObject.Find("Map/NavigationGrid");
						var navGrid = navGridGameObject.GetComponent<NavigationGrid>();
						var resizedarray = ResizeArray(navGrid.zones, 9, 4);
						zone.name = "NextSceneZone";
						navGrid.zones = resizedarray;
						NavGridZone.blockedDirections = new List<LookDirection>();
						NavGridZone.navigationEvents[0].action = delegate
						{
							var newPosition = new Vector3(4.202f, -0.668f, 0);
							float moveDuration =
								Vector3.Distance(Singleton<OverworldPlayerController>.Instance.transform.position,
									newPosition) / 1.5f;
							Singleton<OverworldPlayerController>.Instance.movingBetweenZones = true;
							Singleton<OverworldPlayerController>.Instance.StartCoroutine(
								Singleton<OverworldPlayerController>.Instance.CurrentZone.CompleteDepartSequences(
									delegate
									{
										Singleton<OverworldPlayerController>.Instance.StartMove(newPosition,
											moveDuration);
									}));
							CustomCoroutine.WaitThenExecute(1.5f, delegate { ZoneEntrance.EnterZone(); });
						};
						NavGridZone.navigationEvents[0].direction = LookDirection.East;
						navGrid.zones[7, 3] = NavGridZone;
					}

				}
			}

			static T[,] ResizeArray<T>(T[,] original, int rows, int cols)
			{
				var newArray = new T[rows, cols];
				int minRows = Math.Min(rows, original.GetLength(0));
				int minCols = Math.Min(cols, original.GetLength(1));
				for (int i = 0; i < minRows; i++)
				for (int j = 0; j < minCols; j++)
					newArray[i, j] = original[i, j];
				return newArray;
			}

				public static IEnumerator ChangeSceneAndDeleteAllIdONTneEd()
		{
			SceneLoader.Load(SaveData.Data.overworldScene);
			yield return new WaitForSeconds(0.3f);
			thenextscene=SceneManager.CreateScene("TheNextAct2Scene");
			
			if (GameObject.Find("GBCCameras"))
			{
				GameObject obje = GameObject.Find("GBCCameras");
				SceneManager.MoveGameObjectToScene(obje, thenextscene);
			}
			if (GameObject.Find("Map"))
			{
				GameObject obje = GameObject.Find("Map");
				SceneManager.MoveGameObjectToScene(obje, thenextscene);
			}
			if (GameObject.Find("GBCSingletons"))
			{
				GameObject obje = GameObject.Find("GBCSingletons");
				SceneManager.MoveGameObjectToScene(obje, thenextscene);
			}
			if (GameObject.Find("CustomCoroutineRunner"))
			{
				GameObject obje = GameObject.Find("GBCSingletons");
				SceneManager.MoveGameObjectToScene(obje, thenextscene);
			}
			yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
			SceneManager.SetActiveScene(thenextscene);
			//Log.LogInfo("scene changed");
			if(GameObject.Find("Map/Base"))
				GameObject.Destroy(GameObject.Find("Map/Base"));
			var newPosition= new Vector3(0f, 0.4f, 0); float moveDuration = 0; Singleton<OverworldPlayerController>.Instance.movingBetweenZones = true;
			Singleton<OverworldPlayerController>.Instance.StartCoroutine(
				Singleton<OverworldPlayerController>.Instance.CurrentZone.CompleteDepartSequences(delegate
				{
					Singleton<OverworldPlayerController>.Instance.StartMove(newPosition, moveDuration);
				}));
			GameObject.FindObjectOfType<NavigationGrid>().zones=new NavigationZone[16,16];
			var zones=GameObject.FindObjectOfType<NavigationGrid>().zones;
			foreach (var zonez in GameObject.FindObjectsOfType<NavigationZone2D>())
			{
				GameObject.Destroy(zonez.gameObject);
			}
			foreach (var zonez in GameObject.FindObjectsOfType<ZoneEntrance>())
			{
				GameObject.Destroy(zonez.gameObject);
			}
			//
			var player = Singleton<OverworldPlayerController>.Instance;
			{
				
					var tex=ImageUtils.LoadTexture("bridgedtu");
					var StartingIsland=new GameObject();
					StartingIsland.transform.position=new Vector3(0,0.4f,0);
					StartingIsland.name = "bridge";
					StartingIsland.layer = 17;
					var entrance=StartingIsland.AddComponent<ZoneEntrance>();
					var zone = StartingIsland.AddComponent<NavigationZone2D>();
					var t=new GameObject();
					var z=t.AddComponent<AlternateInputInteractable>();
							
					GameObject.DontDestroyOnLoad(t);
					StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
					zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
					{
						new NavigationZone2D.NavigationEvent()
						{
							
							action = delegate {  },
							direction = LookDirection.North
						}
					};
					zones[1, 2 ]= zone;
					player.CurrentZone = zone;
					player.startingZone = zone;
					player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgedtu");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0,0.254f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[1, 3 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgecross");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[1, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgeltr");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.15f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[2, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgeltr");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.3f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[3, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgeltr");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.45f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[4, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgeltr");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.6f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[5, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgeltr");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.75f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[6, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgeltr");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.9f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[7, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgecross");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(1.05f,0.1f,0);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[8, 4 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgedtu");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(1.05f,0.25f,-0.1f);
				StartingIsland.name = "bridge";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
							
						action = delegate {  },
						direction = LookDirection.North
					}
				};
				zones[8, 3 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				
				var tex=ImageUtils.LoadTexture("bridgedtu");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(1.05f,0.4f,-0.1f);
				StartingIsland.name = "smithisland";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				var t=new GameObject();
				var z=t.AddComponent<AlternateInputInteractable>();
							
				GameObject.DontDestroyOnLoad(t);
				StartingIsland.AddComponent<ZoneEntrance>();
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
						action = delegate { entrance.EnterZone(); },
						direction = LookDirection.North
					}
				};
				zones[8, 2 ]= zone;
				player.movingBetweenZones = false;
			}
			{
				var tex=ImageUtils.LoadTexture("heatFull_diorama");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0.93f,0.7f,0);
				StartingIsland.name = "smithisland";
				StartingIsland.layer = 17;
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;

			}
			{
				var tex=ImageUtils.LoadTexture("neutral_startisland");
				var StartingIsland=new GameObject();
				StartingIsland.transform.position=new Vector3(0,0.7f,0);
				StartingIsland.name = "StartingIslandz";
				StartingIsland.layer = 17;
				var entrance=StartingIsland.AddComponent<ZoneEntrance>();
				var zone = StartingIsland.AddComponent<NavigationZone2D>();
				StartingIsland.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
				zone.navigationEvents=new List<NavigationZone2D.NavigationEvent>()
				{
					new NavigationZone2D.NavigationEvent()
					{
						action = delegate { entrance.EnterZone(); },
						direction = LookDirection.North
					}
				};
				zones[1, 1] = zone;
				player.movingBetweenZones = false;
			}


		}
			
			public static GameObject thenextsceneobject;

			public class SceneTransitionVolumez: MonoBehaviour
			{
				private void OnCollisionEnter2D(Collision2D other)
				{
					if (other.gameObject.GetComponent<PlayerMovementController>())
					{
						OnPlayerCollision();
					}
				}

				public void OnPlayerCollision()
				{
					Singleton<PlayerMovementController>.Instance.enabled = true;
					base.StartCoroutine(this.TransitionSequence());
				}

				public IEnumerator TransitionSequence()
				{
					Singleton<GBC.CameraEffects>.Instance.FadeOut();
					AudioController.Instance.PlaySound2D("chipDelay_1", MixerGroup.GBCSFX, 1f, 0f, null, null, null, null, false);
					AudioController.Instance.FadeOutLoop(0.4f, Array.Empty<int>());
					yield return new WaitForSeconds(0.7f);
					this.ChangeScene();
					yield break;
				}
			
				public void ChangeScene()
				{
					GameObject.FindObjectOfType<TweenEngine>().StartCoroutine(ChangeSceneAndDeleteAllIdONTneEd());
				}
			
			}
			
					[HarmonyPatch(typeof(GBC.CollectionUI), nameof(GBC.CollectionUI.Start))]
		public class z
		{
			public static void Postfix(ref CollectionUI __instance)
			{
				foreach (var VARIABLE in __instance.gameObject.GetComponentsInChildren<GBC.GenericUIButton>())
				{
					if (VARIABLE.gameObject.name == "Tab_1")
					{
						if (!GameObject.Find("HeatTab"))
						{
							var z = GameObject.Instantiate(VARIABLE);
							z.gameObject.layer = 24;
							z.gameObject.name = "HeatTab";
							z.gameObject.transform.parent = VARIABLE.transform.parent;
							z.gameObject.transform.localPosition =
								VARIABLE.transform.localPosition + Vector3.up * 0.145f;
							var tex = ImageUtils.LoadTexture("heattab_collectionui");
							foreach (var a in z.gameObject.GetComponentsInChildren<SpriteRenderer>())
							{
								if (a.name != z.gameObject.name)
								{
									a.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
										new Vector2(0.5f, 0.5f), 100.0f);
								}
							}

							var button = z.gameObject.GetComponent<GenericUIButton>();
							button.inputKey = KeyCode.Alpha5;
							button.coll2D = button.gameObject.GetComponent<BoxCollider2D>();
							Singleton<CollectionUI>.Instance.tabButtons.Add(button);
							var SmithCardsz = Singleton<CollectionUI>.Instance.collectionCards.FindAll(info =>
								info.ConvertToSCI()?.Temple != Enums.SawyerTemples.None).Distinct().ToList();
							List<CardInfo> SmithCards = new List<CardInfo>();
							foreach (var b in SmithCardsz)
							{
								if (!SmithCards.Contains(CardLoader.AllData.Find(info => info.name == b.name)))
								{
									SmithCards.Add(CardLoader.AllData.Find(info => info.name == b.name));
								}
							}

							button.OnButtonDown = new Action<GenericUIButton>(uiButton => ShowPage(SmithCards));
						}

					}
				}
			}
		}

		[HarmonyPatch(typeof(GBC.CollectionUI), nameof(GBC.CollectionUI.ShowTabForPage))]
		public class ThisTabIsUniqueOkayForArrows
		{
			public static bool Prefix(ref CollectionUI __instance, int pageIndex)
			{
				for (int i = 0; i < __instance.tabPageIndices.Length; i++)
				{
					int num = __instance.tabPageIndices[i];
					int num2 = (i == __instance.tabPageIndices.Length - 1)
						? int.MaxValue
						: __instance.tabPageIndices[i + 1];
					if (pageIndex >= num && pageIndex < num2)
					{
						if (i != 4)
						{
							__instance.ShowActiveTab(i);
						}
						else
						{
							__instance.OnTabSelected(i);
						}


						break;
					}
				}

				return false;
			}
		}

		[HarmonyPatch(typeof(GBC.CollectionUI), nameof(GBC.CollectionUI.OnTabSelected))]
		public class ThisTabIsUniqueOkay
		{
			public static bool Prefix(ref CollectionUI __instance, int tabIndex)
			{
				if (tabIndex >= 4)
				{
					__instance.tabButtons[tabIndex]?.OnButtonDown?.Invoke(__instance.tabButtons[tabIndex]);
					return false;
				}

				return true;

			}
		}

		public static void ShowPage(List<CardInfo> cardstoputinapage)
		{
			foreach (GenericUIButton genericUIButton in Singleton<CollectionUI>.Instance.tabButtons)
			{
				genericUIButton.GetComponent<SpriteRenderer>().sprite =
					Singleton<CollectionUI>.Instance.defaultTabSprite;
			}

			Singleton<CollectionUI>.Instance.tabButtons[4].GetComponent<SpriteRenderer>().sprite =
				Singleton<CollectionUI>.Instance.selectedTabSprite;
			var pageIndex = 666;
			Singleton<CollectionUI>.Instance.currentPage = pageIndex;
			List<CardInfo> list = cardstoputinapage;
			for (int i = 0; i < Singleton<CollectionUI>.Instance.pageCards.Count; i++)
			{
				PixelSelectableCard pixelSelectableCard = Singleton<CollectionUI>.Instance.pageCards[i];
				if (i < list.Count)
				{
					pixelSelectableCard.gameObject.SetActive(true);
					Singleton<CollectionUI>.Instance.AssignCard(pixelSelectableCard, list[i]);
				}
				else
				{
					pixelSelectableCard.gameObject.SetActive(false);
				}
			}

			PixelSelectableCard pixelSelectableCard2 =
				Singleton<CollectionUI>.Instance.pageCards.Find((PixelSelectableCard x) =>
					Singleton<InteractionCursor>.Instance.CurrentInteractable == x);
			if (pixelSelectableCard2 != null)
			{
				Singleton<CollectionUI>.Instance.DisplayCardInPreviewPanel(pixelSelectableCard2);
			}
			else if (Singleton<CollectionUI>.Instance.cardDisplayMode == CollectionUI.CardDisplayMode.Inventory)
			{
				Singleton<CollectionUI>.Instance.DisplayCardInPreviewPanel(
					Singleton<CollectionUI>.Instance.pageCards[0]);
			}
			else if (Singleton<CollectionUI>.Instance.cardDisplayMode == CollectionUI.CardDisplayMode.Showcase)
			{
				Singleton<CollectionUI>.Instance.previewPanel.DisplayCard(null, null);
				foreach (PixelSelectableCard pixelSelectableCard3 in Singleton<CollectionUI>.Instance.pageCards)
				{
					if (pixelSelectableCard3.gameObject.activeInHierarchy)
					{
						Singleton<CollectionUI>.Instance.DisplayCardInPreviewPanel(pixelSelectableCard3);
						break;
					}
				}
			}

			if (Singleton<CollectionUI>.Instance.pageNumberText != null)
			{
				if (Localization.CurrentLanguage == Language.English)
				{
					Singleton<CollectionUI>.Instance.pageNumberText.SetText(
						string.Format("PAGE {0}", (pageIndex + 1).ToString()), false);
					return;
				}

				Singleton<CollectionUI>.Instance.pageNumberText.SetText((pageIndex + 1).ToString(), false);
			}
		}

		[HarmonyPatch(typeof(CollectionUI), nameof(CollectionUI.CreatePages))]
		public class bb
		{
			public static bool Prefix(ref CollectionUI __instance, List<CardInfo> cards,
				ref List<List<CardInfo>> __result)
			{

				var cardz = cards.FindAll(info => info.ConvertToSCI().Temple == Enums.SawyerTemples.None);
				cardz.Sort(delegate(CardInfo a, CardInfo b)
				{
					int num2 = a.temple - b.temple;
					if (num2 != 0)
					{
						return num2;
					}

					int num3 = a.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 0;
					int num4 = (b.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 0) - num3;
					if (num4 != 0)
					{
						return num4;
					}

					int num5 = a.CostTier - b.CostTier;
					if (num5 != 0)
					{
						return num5;
					}

					int num6 = a.BonesCost - b.BonesCost;
					if (num6 != 0)
					{
						return num6;
					}

					int num7 = ((a.GemsCost.Count == 1) ? a.GemsCost[0] : GemType.Green) -
					           ((b.GemsCost.Count == 1) ? b.GemsCost[0] : GemType.Green);
					if (num7 != 0)
					{
						return num7;
					}

					int num8 = a.DisplayedNameEnglish.CompareTo(b.DisplayedNameEnglish);
					if (num8 == 0)
					{
						return a.name.CompareTo(b.name);
					}

					return num8;
				});
				cardz = new List<CardInfo>(cardz);
				List<CardInfo> toRemove = new List<CardInfo>();
				for (int i = 1; i < cardz.Count; i++)
				{
					if (cardz[i].name == cardz[i - 1].name)
					{
						toRemove.Add(cardz[i]);
					}
				}

				cardz.RemoveAll((CardInfo x) => toRemove.Contains(x));
				List<List<CardInfo>> list = new List<List<CardInfo>>();
				list.Add(new List<CardInfo>());
				for (int j = 0; j < cardz.Count; j++)
				{
					List<CardInfo> list2 = list[list.Count - 1];
					if (j == 0)
					{
						int temple = (int) cardz[j].temple;
						for (int k = 0; k < temple; k++)
						{
							list.Add(new List<CardInfo>());
							__instance.tabPageIndices[k + 1] = list.IndexOf(list2) + 1;
							list2 = list[list.Count - 1];
						}
					}

					list2.Add(cardz[j]);
					bool flag = j == cardz.Count - 1;
					if (!flag)
					{
						int temple2 = (int) cardz[j].temple;
						int temple3 = (int) cardz[j + 1].temple;
						int num = temple3 - temple2 - 1;
						for (int l = 0; l < num; l++)
						{
							list.Add(new List<CardInfo>());
							__instance.tabPageIndices[temple2 + 1 + l] = list.IndexOf(list2) + 1;
							list2 = list[list.Count - 1];
						}

						bool flag2 = !flag && temple2 != temple3;
						if (list2.Count >= 8 || flag2)
						{
							list.Add(new List<CardInfo>());
							if (flag2)
							{
								__instance.tabPageIndices[temple3] = list.IndexOf(list2) + 1;
							}
						}
					}
				}

				for (int m = 0; m < __instance.tabPageIndices.Length; m++)
				{
					if (m > 0 && __instance.tabPageIndices[m] == 0)
					{
						list.Add(new List<CardInfo>());
						__instance.tabPageIndices[m] = list.Count - 1;
					}
				}

				__result = list;
				return false;
			}
		}

		[HarmonyPatch(typeof(CollectionUI), nameof(CollectionUI.RefreshPage))]
		public class fixautocomplete
		{
			public static bool Prefix(ref CollectionUI __instance)
			{
				if (__instance.currentPage == 666)
				{
					var SmithCardsz = Singleton<CollectionUI>.Instance.collectionCards.FindAll(info =>
						info.ConvertToSCI().Temple != Enums.SawyerTemples.None).Distinct().ToList();
					List<CardInfo> SmithCards = new List<CardInfo>();
					foreach (var b in SmithCardsz)
					{
						if (!SmithCards.Contains(CardLoader.AllData.Find(info => info.name == b.name)))
						{
							SmithCards.Add(CardLoader.AllData.Find(info => info.name == b.name));
						}
					}

					ShowPage(SmithCards);
					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(CollectionUI), nameof(CollectionUI.Initialize))]
		public class v
		{
			public static bool Prefix(ref CollectionUI __instance, List<CardInfo> cards,
				CollectionUI.CardDisplayMode mode = CollectionUI.CardDisplayMode.Inventory)
			{
				__instance.cardDisplayMode = mode;
				__instance.currentPage = 0;
				__instance.tabPageIndices = new int[5];
				__instance.collectionCards = cards;
				__instance.collectionPages = __instance.CreatePages(cards);
				List<CardInfo> list = __instance.collectionPages.Find((List<CardInfo> x) => x.Count > 0);
				if (list != null)
				{
					__instance.ShowPage(__instance.collectionPages.IndexOf(list));
				}
				else
				{
					__instance.ShowPage(__instance.currentPage);
				}

				return false;
			}
		}

			
			public static IEnumerator StartingIslandPlus()
		{
			SceneLoader.Load("GBC_Mycologist_Hut");
			yield return new WaitForSeconds(0.3f);
			startingislandscene=SceneManager.CreateScene("StartingIsland");
			if(GameObject.FindObjectOfType<PlayerMovementController>())
			{
				GameObject obje = GameObject.FindObjectOfType<PlayerMovementController>().gameObject;
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			if (GameObject.Find("GBCCameras"))
			{
				GameObject obje = GameObject.Find("GBCCameras");
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			if (GameObject.Find("GBCSingletons"))
			{
				GameObject obje = GameObject.Find("GBCSingletons");
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			if (GameObject.Find("CustomCoroutineRunner"))
			{
				GameObject obje = GameObject.Find("GBCSingletons");
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
			{
				var bg=new GameObject("Background");
				bg.layer = 17;
				bg.transform.position=new Vector3(0,-0.4f,0);
				var tex = ImageUtils.LoadTexture("startingislandbg");
				var spriterenderer = bg.AddComponent<SpriteRenderer>();
				spriterenderer.sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size=new Vector2(5.68f, 2.4f);
				var collider=bg.AddComponent<PolygonCollider2D>();
				
				var test = collider.points;
				test=new Vector2[16];
				test[0]=new Vector2(-3.006f, 0.7491f+0.34f);
				test[1]=new Vector2(-3.0264f, -0.1817f+0.34f);
				test[2]=new Vector2(-2.161f, -0.1302f+0.34f);
				test[3]=new Vector2(-1.862f, -0.0271f+0.34f);
				test[4]=new Vector2(-1.5554f, 0.0368f+0.34f);
				test[5]=new Vector2(-1.0663f, 0.2244f+0.34f);
				test[6]=new Vector2(-0.6717f, 0.3174f+0.34f);
				test[7]=new Vector2(-0.1341f,  0.3297f+0.34f);
				test[8]=new Vector2(0.1425f, 0.2344f+0.34f);
				test[9]=new Vector2(0.5307f, 0.2053f+0.34f);
				test[11-1]=new Vector2(0.9148f, 0.2484f+0.34f);
				test[12-1]=new Vector2(1.6837f, 0.1356f+0.34f);
				test[13-1]=new Vector2(2.0032f, -0.0274f+0.34f);
				test[14-1]=new Vector2(2.2385f, -0.1777f+0.34f);
				test[15-1]=new Vector2(3.1501f, -0.1953f+0.34f);
				test[16-1]=new Vector2(3.177f, 0.7242f+0.34f);
				collider.points = test;
			}
			{
				var floor = new GameObject("MartelStone");
				floor.layer = 17;
				floor.transform.position = new Vector3(-0.29f, 0.195f , -0.2f);
				var tex = ImageUtils.LoadTexture("martel_deckstone");
				var spriterenderer = floor.AddComponent<SpriteRenderer>();
				spriterenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
					new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size = new Vector2(5.68f, 2.4f);
				{
					var h = new GameObject("MartelStone");
					h.layer = 17;
					h.transform.position = new Vector3(-0.29f, 0.114f , -0.2f);
					var collied=h.AddComponent<BoxCollider2D>();
					collied.size=new Vector2(0.15f, 0.23f);
				}

			}
			{
				var floor = new GameObject("Floor");
				floor.layer = 17;
				floor.transform.position = new Vector3(0, -0.07f, -0.1f);
				var tex = ImageUtils.LoadTexture("startingislandground");
				var spriterenderer = floor.AddComponent<SpriteRenderer>();
				spriterenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
					new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size = new Vector2(5.68f, 2.4f);
			}
			Singleton<PlayerMovementController>.Instance.transform.position=Vector3.zero;
			{
				var floor = new GameObject("Exit");
				floor.layer = 17;
				floor.transform.position = new Vector3(0, -1.65f, 0f);
				floor.AddComponent<BoxCollider2D>().size=new Vector2(10,0.25f);
				var z = floor.AddComponent<SceneTransitionVolumez>();
				
			}
			{
				var floor = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs\\gbcinterior\\temples\\DeckPileVolume"));
				floor.layer = 17;
				floor.name = "MartelDeck";
				floor.transform.position =new Vector3(-0.29f, -0.1f , -0.2f);
				var deck=floor.GetComponent<PickupCardPileVolume>();
				var coal = CardLoader.AllData.Find(info => info.name == "Coal");
				deck.cards=new List<CardInfo>(){coal,coal,coal,coal,coal,coal,coal,coal,coal,coal};
				deck.textLines=new List<string>(){"Do you want to pick the deck of heat"};
				deck.button.uiButton.OnButtonDown= delegate(GenericUIButton button)
				{
					{
						{
							CustomCoroutine.Instance.StartCoroutine(ShowAndDontKillPlayer(deck ,deck.textLines));
						}
					}

				};

			}
			GameObject.FindObjectOfType<PlayerMovementController>().SetEnabled(true);
			SceneManager.SetActiveScene(startingislandscene);
			Singleton<GBC.CameraEffects>.Instance.FadeIn();

			yield break;
		}
			public static IEnumerator ShowAndDontKillPlayer(PickupCardPileVolume deck, List<string> textlines)
			{
				foreach (string text in deck.textLines)
				{
					deck.StartCoroutine(deck.ShowSingleTextLine(text, deck.textLines.IndexOf(text) == deck.textLines.Count - 1)) ;
				}
				yield return new WaitUntil((() => !Singleton<PlayerMovementController>.Instance.enabled));
			
				Singleton<PlayerMovementController>.Instance.SetEnabled(true);
				EventTrigger.TriggerEvent triggerEvent = deck.postTextEvent;
				if (triggerEvent != null)
				{
					triggerEvent.Invoke(null);
				}

			}

			public class MartelNpc : MonoBehaviour
			{
				private void OnCollisionEnter2D(Collision2D other)
				{
					if (other.gameObject.GetComponent<PlayerMovementController>())
					{
						GameObject.FindObjectOfType<TweenEngine>().StartCoroutine(GoIntoAbattle());
					}
				}
			}
	public static IEnumerator GoIntoAbattle()
		{
			var encounter = new EncounterData();
			encounter.Blueprint=new EncounterBlueprintData();
			var turns=encounter.Blueprint.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();
			turns.Add(new List<EncounterBlueprintData.CardBlueprint>(){ new EncounterBlueprintData.CardBlueprint{card = CardLoader.GetCardByName("Squirrel")}, new EncounterBlueprintData.CardBlueprint{card = CardLoader.GetCardByName("Squirrel")}, new EncounterBlueprintData.CardBlueprint{card = CardLoader.GetCardByName("Squirrel")}, new EncounterBlueprintData.CardBlueprint{card = CardLoader.GetCardByName("Squirrel")}});
			encounter.opponentTurnPlan = new List<List<CardInfo>>();
			encounter.opponentTurnPlan.Add(new List<CardInfo>(){CardLoader.GetCardByName("Squirrel"),CardLoader.GetCardByName("Squirrel"),CardLoader.GetCardByName("Squirrel"),CardLoader.GetCardByName("Squirrel")});
			
					{
					}
					////Log.LogInfo(BlueprintSubfolderName);
						////Log.LogInfo(blueprintId);
						////Log.LogInfo(encounter);
					
					////Log.LogInfo("test2");
					if (!SaveData.Data.deck.IsValidGBCDeck())
					{
						int num = 0;
						while (!SaveData.Data.deck.IsValidGBCDeck())
						{
							if (SaveData.Data.collection.Cards.Count > num)
							{
								SaveData.Data.deck.AddCard(SaveData.Data.collection.Cards[num]);
							}
							else
							{
								SaveData.Data.deck.AddCard(CardLoader.GetCardByName("Squirrel"));
							}

							num++;
						}
					}

					var encmanager = new GameObject();
					var gbcEncounterManager = encmanager.AddComponent<GBCEncounterManager>();
					GBCEncounterManager.Instance = gbcEncounterManager;
					GBCEncounterManager.Instance.LoadBattleScene();
					////Log.LogInfo("test666");
					//Instantiate()
					yield return new WaitUntil((() => Singleton<TurnManager>.Instance));
					{
						var resourcemanager=GameObject.FindObjectOfType<PixelResourcesManager>().gameObject;
						//Destroy(GameObject.FindObjectOfType<PixelResourcesManager>());
						//TPTOHEREFUCKER
						var bonesicon = resourcemanager.gameObject.GetComponentInChildren<GBC.PixelNumeral>().gameObject;
						var heat=GameObject.Instantiate(bonesicon);
						heat.transform.parent = bonesicon.transform.parent;
						heat.transform.localPosition = bonesicon.transform.localPosition + Vector3.left*0.24f+ Vector3.down*0.002f;
						heat.name = "HeatIcon";
						var tex = ImageUtils.LoadTexture("heaticon");
						heat.gameObject.AddComponent<SpriteRenderer>().sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
						heat.gameObject.layer = 22;
						
						var newresm=resourcemanager.AddComponent<SawyerResourceManager>();
						newresm.HeatCount=heat.GetComponent<PixelNumeral>();
						newresm.HeatsParent = heat.transform;

					}

					GameObject gameObject = new GameObject();
					////Log.LogInfo("test5");
					gameObject.name = "Opponent";
					Opponent opponent;
					////Log.LogInfo("test6");
					opponent = gameObject.AddComponent<PixelOpponent>();
					////Log.LogInfo("test7");
					string text = encounter.aiId;
					////Log.LogInfo("test8");
					if (string.IsNullOrEmpty(text))
					{
						text = "AI";
					}

					opponent.AI = (Activator.CreateInstance(CustomType.GetType("DiskCardGame", text)) as AI);
					////Log.LogInfo("test9");
					opponent.NumLives = 666;
					////Log.LogInfo("test10");
					opponent.OpponentType = Opponent.Type.Default;
					////Log.LogInfo("test11");
					opponent.TurnPlan = opponent.ModifyTurnPlan(encounter.opponentTurnPlan);
					////Log.LogInfo("test12");
					opponent.Blueprint = encounter.Blueprint;
					////Log.LogInfo("test13");
					opponent.Difficulty = encounter.Difficulty;
					////Log.LogInfo("test15");
					opponent.ExtraTurnsToSurrender = 3;
					////Log.LogInfo("test16");
					var turnManager = GameObject.FindObjectOfType<TurnManager>();
					turnManager.opponent = opponent;
					////Log.LogInfo("test17");
					////Log.LogInfo("test4");
					turnManager.StartCoroutine(Singleton<TurnManager>.Instance.GameSequence(encounter));
					Singleton<TurnManager>.Instance.SpecialSequencer = null;
					//note seems like Singleton<GameFlowManager>.Instance is null
					////Log.LogInfo("test3");
					yield return new WaitUntil(() =>
						Singleton<TurnManager>.Instance == null || Singleton<TurnManager>.Instance.GameEnded);
					bool playerDefeated = false;
					encmanager = new GameObject();
					gbcEncounterManager = encmanager.AddComponent<GBCEncounterManager>();
					GBCEncounterManager.Instance = gbcEncounterManager;
					if (Singleton<TurnManager>.Instance != null)
					{
						playerDefeated = !Singleton<TurnManager>.Instance.PlayerWon;
						Singleton<InteractionCursor>.Instance.SetHidden(true);
						PauseMenu.pausingDisabled = false;
						Singleton<GBC.CameraEffects>.Instance.FadeOut();
						//AudioController.Instance.FadeOutLoop(1f, Array.Empty<int>());
						yield return new WaitForSeconds(1f);
						if (!playerDefeated)
						{
							//win

							
							var t=new GameObject();
							var z=t.AddComponent<AlternateInputInteractable>();
							
							GameObject.DontDestroyOnLoad(t);
							z.StartCoroutine(ChangeSceneAndDeleteAllIdONTneEd());
						}
						else
						{
		
							var t=new GameObject();
							var z=t.AddComponent<AlternateInputInteractable>();
							
							GameObject.DontDestroyOnLoad(t); 
							z.StartCoroutine(ChangeSceneAndDeleteAllIdONTneEd()); 
						}

					}
		}

			
					public static IEnumerator SmithIsland()
		{
			SceneLoader.Load("GBC_Mycologist_Hut");
			yield return new WaitForSeconds(0.3f);
			startingislandscene=SceneManager.CreateScene("StartingIsland");
			if(GameObject.FindObjectOfType<PlayerMovementController>())
			{
				GameObject obje = GameObject.FindObjectOfType<PlayerMovementController>().gameObject;
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			if (GameObject.Find("GBCCameras"))
			{
				GameObject obje = GameObject.Find("GBCCameras");
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			if (GameObject.Find("GBCSingletons"))
			{
				GameObject obje = GameObject.Find("GBCSingletons");
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			if (GameObject.Find("CustomCoroutineRunner"))
			{
				GameObject obje = GameObject.Find("GBCSingletons");
				SceneManager.MoveGameObjectToScene(obje, startingislandscene);
			}
			yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
			{
				var bg=new GameObject("Background");
				bg.layer = 17;
				bg.transform.position=new Vector3(0,-0.4f,0);
				var tex =  ImageUtils.LoadTexture("smithinsides");
				var spriterenderer = bg.AddComponent<SpriteRenderer>();
				spriterenderer.sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size=new Vector2(5.68f, 2.4f);
				//var collider=bg.AddComponent<PolygonCollider2D>();
			}
			{
				var bg=new GameObject("Anvill");
				bg.layer = 17;
				bg.transform.position=new Vector3(0,-0.4f,-0.1f);
				var colliderobj=new GameObject("AnvilCollider");
				colliderobj.transform.position=new Vector3(0,0.12f,-0.1f);
				var tex =  ImageUtils.LoadTexture("smithinsidesanvil");
				var spriterenderer = bg.AddComponent<SpriteRenderer>();
				spriterenderer.sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size=new Vector2(5.68f, 2.4f);
				var collider=colliderobj.AddComponent<BoxCollider2D>();
				collider.size=new Vector2(0.4f,0.15f);
			}
			{
				var bg = new GameObject("Martel");
				bg.layer = 17;
				bg.transform.position = new Vector3(0, -0.4f, -0.2f);
				var tex = ImageUtils.LoadTexture("smithinsidessmithhimself");
				var spriterenderer = bg.AddComponent<SpriteRenderer>();
				spriterenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
					new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size = new Vector2(5.68f, 2.4f);
				var collider = bg.AddComponent<BoxCollider2D>();
				collider.size = new Vector2(0.15f, 0.15f);
				var npc = bg.AddComponent<MartelNpc>();
				//var collider=bg.AddComponent<PolygonCollider2D>();
			}
			{
				var bg=new GameObject("Background");
				bg.layer = 17;
				bg.transform.position=new Vector3(0,-0.4f,0);
				var tex =  ImageUtils.LoadTexture("smithinsides");
				var spriterenderer = bg.AddComponent<SpriteRenderer>();
				spriterenderer.sprite=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
				spriterenderer.size=new Vector2(5.68f, 2.4f);
				//var collider=bg.AddComponent<PolygonCollider2D>();
			}
			Singleton<PlayerMovementController>.Instance.transform.position=Vector3.zero;
			{
				var floor = new GameObject("Exit");
				floor.layer = 17;
				floor.transform.position = new Vector3(0, -1.65f, 0f);
				floor.AddComponent<BoxCollider2D>().size=new Vector2(10,0.25f);
				var z = floor.AddComponent<SceneTransitionVolumez>();
			}
			GameObject.FindObjectOfType<PlayerMovementController>().SetEnabled(true);
			SceneManager.SetActiveScene(startingislandscene);
			Singleton<GBC.CameraEffects>.Instance.FadeIn();

			yield break;
		}

			
			[HarmonyPatch(typeof(ZoneEntrance), nameof(ZoneEntrance.EnterZone))]
			public class ReplaceHowZoneEntranceWorksForTheTower
			{
				public static bool Prefix(ref ZoneEntrance __instance)
				{

					if (__instance != null)
					{
						if (__instance.gameObject.name == "NextSceneZone")
						{

							var t = new GameObject();
							var z = t.AddComponent<AlternateInputInteractable>();

							GameObject.DontDestroyOnLoad(t);
							z.StartCoroutine(ChangeSceneAndDeleteAllIdONTneEd());


							return false;
						}
						if (__instance.gameObject.name == "StartingIslandz")
						{

							var t = new GameObject();
							var z = t.AddComponent<AlternateInputInteractable>();

							GameObject.DontDestroyOnLoad(t);
							z.StartCoroutine(StartingIslandPlus());


							return false;
						}
						if (__instance.gameObject.name == "smithisland")
						{

							var t = new GameObject();
							var z = t.AddComponent<AlternateInputInteractable>();

							GameObject.DontDestroyOnLoad(t);
							z.StartCoroutine(SmithIsland());


							return false;
						}
					}

					return true;
				}
			}
		}

    }
}