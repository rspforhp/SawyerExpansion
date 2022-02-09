using System;
using APIPlugin;
using System.Collections.Generic;
using DiskCardGame;
using SawyerExpansion.ClassesWithInstances;
using SawyerExpansion.ExtendClasses;
using UnityEngine;

namespace SawyerExpansion.Utils
{
    public static class CardUtils
    {

   
        
        public static List<CardInfo> AddCards()
        {
            var allcards=new List<CardInfo>();
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "Coal";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("coal"));
                cardinfo.displayedName = "Coal Piece";
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.metaCategories=new List<CardMetaCategory>(){CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.appearanceBehaviour=new List<CardAppearanceBehaviour.Appearance>() {CardAppearanceBehaviour.Appearance.TerrainBackground};
                cardinfo.baseAttack = 0;
                cardinfo.baseHealth = 1;
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.CoolingRage.ability);
                cardinfo.ConvertToSCI().HeatCost = 1;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "BlastFurnace";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("bfurnace"));
                cardinfo.displayedName = "BlastFurnace";
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.metaCategories=new List<CardMetaCategory>() {CardMetaCategory.Rare, CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.appearanceBehaviour=new List<CardAppearanceBehaviour.Appearance>() {CardAppearanceBehaviour.Appearance.RareCardBackground};
                cardinfo.baseAttack = 0;
                cardinfo.baseHealth = 1;
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.CoolingRage.ability);
                cardinfo.ConvertToSCI().HeatCost = 3;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                allcards.Add(cardinfo);
            }
            foreach (var card in allcards)
            {
                NewCard.Add(card);
            }
            return allcards;
        }
    }
}