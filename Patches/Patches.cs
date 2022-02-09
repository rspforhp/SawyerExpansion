using System.Collections.Generic;
using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;
using SawyerExpansion.ExtendClasses;
using SawyerExpansion.Singleton;


namespace SawyerExpansion.Patches
{
    public static class Patches
    {
        [HarmonyPatch(typeof(HintsHandler), nameof(HintsHandler.OnNonplayableCardClicked))]
        public class NotEnoughOfResource
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
    }
}