using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using GBC;
using SawyerExpansion.ExtendClasses;
using SawyerExpansion.Singleton;
using SawyerExpansion.Utils;
using UnityEngine;

namespace SawyerExpansion.ClassesWithInstances
{
    public static class AbilitiesUtils
    {
        public static List<Tuple<Ability, Type>> AddAbilities()
        {
            var allabilities = new List<Tuple<Ability, Type>>();
            allabilities.Add(AddAbilityCRage());
            allabilities.Add(AddAbilityCoal());
            allabilities.Add(AddAbilityHrage());
            allabilities.Add(AddAbilityHeatDependant());
            allabilities.Add(AddAbilityScHeat());
            allabilities.Add(AddAbilityHeatChasing());
            allabilities.Add(AddAbilityHeatSwapper());

            return allabilities;
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
                            this.StartCoroutine(Singleton<PixelPlayerHand>.Instance.AddCardToHand(this.Card));
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
                        if (i == 2)
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
                NewAbility ability = new NewAbility(info, behavior, tex, AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.SwappingPower.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behavior);

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
                NewAbility ability = new NewAbility(info, behavior, tex, AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.HeatChasing.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behavior);

            }
        }

        
        private static Tuple<Ability, Type> AddAbilityHeatDependant()
        {
            {
                
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 2;
                info.rulebookName = "A heating fuel";
                info.rulebookDescription = "When this creature is  on board and you have less than 5 heat, this perishes!";
                var tex = ImageUtils.LoadTexture("heatdependant");
                info.pixelIcon=ImageUtils.ConvertToSprite(tex);
                var behavior = typeof(AbilityBehaviours.HeatDependant);
                NewAbility ability = new NewAbility(info, behavior, tex, AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.HeatDependant.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behavior);

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
                NewAbility ability = new NewAbility(info, behaviour, tex, AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.ScorchingHeat.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behaviour);

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
                NewAbility ability = new NewAbility(info, behavior, tex, AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.HeatingRage.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behavior);
            }
        }
        
        private static Tuple<Ability, Type> AddAbilityCoal()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 2;
                info.rulebookName = "A heating fuel";
                info.rulebookDescription = "When this is burned you are awarded with 3 heat instead of 1!";
                var behavior = typeof(AbilityBehaviours.Coal);
                NewAbility ability = new NewAbility(info, behavior, ImageUtils.LoadTexture("readme"), AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.Coal.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behavior);
            }
        }

        private static Tuple<Ability, Type> AddAbilityCRage()
        {
            {
                AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
                info.powerLevel = 4;
                info.rulebookName = "Heating iron";
                info.rulebookDescription = "While Blast Furnace is on the board, it gains 1 Power for every 2 cards burned!";
                var behavior = typeof(AbilityBehaviours.CoolingRage);
                NewAbility ability = new NewAbility(info, behavior, ImageUtils.LoadTexture("readme"), AbilityIdentifier.GetID(Plugin.PluginDetails.PluginGuid, info.rulebookName));
                AbilityBehaviours.CoolingRage.ability = ability.ability;
                return new Tuple<Ability, Type>(ability.ability, behavior);
            }
        }
    }
}