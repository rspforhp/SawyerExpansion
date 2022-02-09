using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
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

            return allabilities;
        }

        internal static class AbilityBehaviours
        {
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
                        return coal;
                    }
                }

                public static Ability coal;
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
                AbilityBehaviours.Coal.coal = ability.ability;
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