﻿using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using SawyerExpansion.ExtendClasses;
using SawyerExpansion.Singleton;
using SawyerExpansion.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SawyerExpansion.ClassesWithInstances
{
    public static class AbilitiesUtils
    {
        public static void AddAbilities()
        {
            //allabilities.Add(AddAbilityCRage());
            AddAbilityCoal();
            //allabilities.Add(AddAbilityHrage());
            AddAbilityHeatDependant();
            //allabilities.Add(AddAbilityScHeat());
            //allabilities.Add(AddAbilityHeatChasing());
            //allabilities.Add(AddAbilityHeatSwapper());
            //allabilities.Add(AddAbilityPorcelain());
        }

        internal static class AbilityBehaviours
        {
            public class SwappingPower : AbilityBehaviour
		{
			public override Ability Ability
			{
				get
				{
					return ability;
				}
			}

			private void Start()
			{
				int attack = base.Card.Info.Attack;
				int health = base.Card.Info.Health;
				CardModificationInfo cardModificationInfo = new CardModificationInfo();
				cardModificationInfo.nonCopyable = true;
				cardModificationInfo.singletonId = "zeroout";
				cardModificationInfo.attackAdjustment = -attack;
				cardModificationInfo.healthAdjustment = -health;
				base.Card.AddTemporaryMod(cardModificationInfo);
				this.mod = new CardModificationInfo();
				this.mod.nonCopyable = true;
				this.mod.singletonId = "statswap";
				this.mod.attackAdjustment = attack;
				this.mod.healthAdjustment = health;
				base.Card.AddTemporaryMod(this.mod);
			}

			public static Ability ability;

			public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
			{
				return true;
			}
			
			private CardModificationInfo mod;


			private bool swapped;

			public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
			{
				if (wasSacrifice && Card.InHand&&killer.InHand)
				{
					yield return base.PreSuccessfulTriggerSequence();
					yield return new WaitForSeconds(0.5f);
					if (base.Card.Info.name == "SwapBot")
					{
						this.swapped = !this.swapped;
						if (this.swapped)
						{
							base.Card.SwitchToAlternatePortrait();
						}
						else
						{
							base.Card.SwitchToDefaultPortrait();
						}
					}
                    int health = base.Card.Health;
					base.Card.HealDamage(base.Card.Status.damageTaken);
					int attack = base.Card.Attack;
					this.mod.attackAdjustment = health;
					this.mod.healthAdjustment = attack;
					base.Card.OnStatsChanged();
					base.Card.Anim.StrongNegationEffect();
					yield return new WaitForSeconds(0.25f);
					if (base.Card.Health <= 0)
					{
						yield return base.Card.Die(false, null, true);
					}
					else
					{
						yield return base.LearnAbility(0.25f);
					}
					yield break;

				}
				yield break;
			}
		}
            public class HeatChasing : AbilityBehaviour
            {
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;
                public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
                {
                    return true;
                }
			

                public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
                {
				
                    if (wasSacrifice && !Card.InHand&&killer.InHand)
                    {
                        {
                            this.StartCoroutine(Singleton<PixelPlayerHand>.Instance.AddCardToHand(this.Card, Vector3.zero, 0));
                            Card.UnassignFromSlot();
                            Card.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                        }

                    }
                    yield break;
                }
            }
            public class CoolingRage : AbilityBehaviour
            {
                
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;
                public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
                {
                    return true;
                }

                private int i;

                public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
                {
				
                    if (wasSacrifice && !Card.InHand&&killer.InHand)
                    {
                        i++;
                        if (i == 3)
                        {
                            this.Card.AddTemporaryMod(new CardModificationInfo(1,0));
                            i = 0;
                        }

                    }
                    yield break;
                }
            }
            public class Coal : AbilityBehaviour
            {
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;
          
                public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
                {
                    return true;
                }

                public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
                {
                   Plugin.PluginDetails.Log.LogInfo(wasSacrifice);
                   Plugin.PluginDetails.Log.LogInfo(this.Card.InHand);
                    if (wasSacrifice && this.Card.InHand&&killer==null)
                    {
                        yield return Singleton<SawyerResourceManager>.Instance.AddHeat(2);
                    }
                }
            }
            public class HeatingRage : AbilityBehaviour
            {
        
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;
                public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
                {
                    return true;
                }

                public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
                {
                    if (wasSacrifice && Card.InHand&&killer.InHand)
                    {
                        if(this.Card.Attack!=4&&this.Card.Health!=4)
                            this.Card.AddTemporaryMod(new CardModificationInfo(1,1));
                    }
                    yield break;
                }
            }
            public class HeatDependant : AbilityBehaviour
            {
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;
                public override bool RespondsToResolveOnBoard()
                {
                    return true;
                }

                public override IEnumerator OnResolveOnBoard()
                {
                    if (Singleton<SawyerResourceManager>.Instance.PlayerHeat-this.Card.Info.ConvertToSCI().HeatCost < 6)
                    {
                        this.Card.Anim.PlayDeathAnimation(true);
                        this.Card.StartCoroutine(this.Card.Die(false, this.Card.slot.opposingSlot.Card));

                    }
                    yield break;
                }

                public override bool RespondsToTurnEnd(bool playerTurnEnd)
                {
                    return playerTurnEnd;
                }

                public override IEnumerator OnTurnEnd(bool playerTurnEnd)
                {
                    if (Singleton<SawyerResourceManager>.Instance.PlayerHeat < 6)
                    {
                        this.Card.Anim.PlayDeathAnimation(true);
                        this.Card.StartCoroutine(this.Card.Die(false, this.Card.slot.opposingSlot.Card));
					
                    }
                    yield break;
                }
            }
            public class ScorchingHeat : AbilityBehaviour
            {
        
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;

                public override bool RespondsToTurnEnd(bool playerTurnEnd)
                {
                    return playerTurnEnd;
                }

                public override IEnumerator OnTurnEnd(bool playerTurnEnd)
                {
                    foreach (var card in Singleton<PixelBoardManager>.Instance.CardsOnBoard.FindAll(card => card!=this.Card))
                    {
                        this.Card.StartCoroutine(card.TakeDamage(1, this.Card));
                    }
                    yield break;
                }
            }
            public class OutOfPorcelain : AbilityBehaviour
            {
        
                public override Ability Ability
                {
                    get
                    {
                        return ability;
                    }
                }

                public static Ability ability;

                public string singletonId = "MadeOutOfPorcelain";
                
                public override bool RespondsToTurnEnd(bool playerTurnEnd)
                {
                    return playerTurnEnd;
                }

                public override IEnumerator OnTurnEnd(bool playerTurnEnd)
                {
                    var card=this.Card as PixelPlayableCard;
                     (card.Anim as PixelCardAnimationController).SetShaking(true);
                     yield return new WaitForSeconds(0.4f);
                     (card.Anim as PixelCardAnimationController).SetShaking(false);
                     var cardmod=new CardModificationInfo();
                    cardmod.singletonId = singletonId;
                    cardmod.abilities=new List<Ability>(){AbilitiesUtil.allData[SeededRandom.Range(0, AbilitiesUtil.allData.Count, SaveManager.saveFile.randomSeed++)].ability};
                    card.AddTemporaryMod(cardmod);
                    yield break;
                }
            }

        }
        private static Tuple<Ability, Type> AddAbilityPorcelain()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 4;
                info.rulebookName = "Porcelain - material";
                info.rulebookDescription = "When player turn ends, gain a random sigil!";
                var tex = ImageUtils.LoadTexture("readme");
                info.pixelIcon=ImageUtils.ConvertToSprite(tex);
                var behavior = typeof(AbilityBehaviours.OutOfPorcelain);
                var ability=AbilityManager.Add(Plugin.PluginDetails.PluginGuid, info, behavior, tex);
                AbilityBehaviours.OutOfPorcelain.ability = ability.Id;
                return new Tuple<Ability, Type>(ability.Id, behavior);

            }
        }

        
        private static Tuple<Ability, Type> AddAbilityHeatSwapper()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 4;
                info.rulebookName = "Heat swap";
                info.rulebookDescription = "When any card is burned while this is in hand swap the stats!";
                var tex = ImageUtils.LoadTexture("readme");
                info.pixelIcon=ImageUtils.ConvertToSprite(tex);
                var behavior = typeof(AbilityBehaviours.SwappingPower);
                var ability=AbilityManager.Add(Plugin.PluginDetails.PluginGuid, info, behavior, tex);
                AbilityBehaviours.SwappingPower.ability = ability.Id;
                return new Tuple<Ability, Type>(ability.Id, behavior);

            }
        }

        private static Tuple<Ability, Type> AddAbilityHeatChasing()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 1;
                info.rulebookName = "Heat chaser";
                info.rulebookDescription = "When this is on the field and a card is burned this goes back into a hand!";
                var tex = ImageUtils.LoadTexture("readme");
                info.pixelIcon=ImageUtils.ConvertToSprite(tex);
                var behavior = typeof(AbilityBehaviours.HeatChasing);
                var ability=AbilityManager.Add(Plugin.PluginDetails.PluginGuid, info, behavior, tex);
                AbilityBehaviours.HeatChasing.ability = ability.Id;
                return new Tuple<Ability, Type>(ability.Id, behavior);

            }
        }

        
        private static void AddAbilityHeatDependant()
        {
            {
                var a=AbilityManager.New(Plugin.PluginDetails.PluginGuid,"Heat Dependant",
                    "When this creature is  on board and you have less than 5 heat, this perishes!",
                    typeof(AbilityBehaviours.HeatDependant), "Artwork\\" + "heatdependant" + ".png");
                AbilityBehaviours.HeatDependant.ability = a.ability;
            }
        }

        private static Tuple<Ability, Type> AddAbilityScHeat()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 3;
                info.rulebookName = "Erupting";
                info.rulebookDescription = "When this creature is  on board, all creatures except this take 1 damage at the end of a turn!";
                var tex = ImageUtils.LoadTexture("readme");
                var behaviour = typeof(AbilityBehaviours.ScorchingHeat);
                var ability=AbilityManager.Add(Plugin.PluginDetails.PluginGuid, info, behaviour, tex);

                AbilityBehaviours.ScorchingHeat.ability = ability.Id;
                return new Tuple<Ability, Type>(ability.Id, behaviour);

            }
        }

    

        
        private static Tuple<Ability, Type> AddAbilityHrage()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 2;
                info.rulebookName = "A heating rage";
                info.rulebookDescription = "When any card is burned while this is in hand this grows to a point of any stat reaching 3(cant go to 4)!";
                var tex = ImageUtils.LoadTexture("heatingrage");
                info.pixelIcon=ImageUtils.ConvertToSprite(tex);
                var behavior = typeof(AbilityBehaviours.HeatingRage);
                var ability=AbilityManager.Add(Plugin.PluginDetails.PluginGuid, info, behavior, tex);

                AbilityBehaviours.HeatingRage.ability = ability.Id;
                return new Tuple<Ability, Type>(ability.Id, behavior);
            }
        }
        
        private static void AddAbilityCoal()
        {
            var a=AbilityManager.New(Plugin.PluginDetails.PluginGuid,"Flammable",
                "Gives 2 more heat when burned!",
                typeof(AbilityBehaviours.Coal), "Artwork\\" + "flammable" + ".png");
            AbilityBehaviours.Coal.ability = a.ability;
        }

        private static Tuple<Ability, Type> AddAbilityCRage()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 4;
                info.rulebookName = "Heating iron";
                info.rulebookDescription = "While Blast Furnace is on the board, it gains 1 Power for every 3 cards burned!";
                var behavior = typeof(AbilityBehaviours.CoolingRage);
                Texture2D tex = default;
                var ability=AbilityManager.Add(Plugin.PluginDetails.PluginGuid, info, behavior, tex);
                AbilityBehaviours.CoolingRage.ability = ability.Id;
                return new Tuple<Ability, Type>(ability.Id, behavior);
            }
        }
    }
}