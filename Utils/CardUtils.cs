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
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.Coal.ability);
                cardinfo.ConvertToSCI().HeatCost = 1;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "Furnace";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("furnace"));
                cardinfo.displayedName = "Furnace";
                cardinfo.metaCategories=new List<CardMetaCategory>(){CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.baseAttack = 0;
                cardinfo.baseHealth = 1;
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.HeatingRage.ability);
                cardinfo.ConvertToSCI().HeatCost = 9;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "Anvil";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("bfurnace"));
                cardinfo.displayedName = "Anvil";
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.metaCategories=new List<CardMetaCategory>() {CardMetaCategory.Rare, CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.appearanceBehaviour=new List<CardAppearanceBehaviour.Appearance>() {CardAppearanceBehaviour.Appearance.RareCardBackground};
                cardinfo.baseAttack = 0;
                cardinfo.baseHealth = 3;
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.CoolingRage.ability);
                cardinfo.ConvertToSCI().HeatCost = 3;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "FireFox";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("firefox"));
                cardinfo.displayedName = "Fire Fox";
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.metaCategories=new List<CardMetaCategory>() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.appearanceBehaviour=new List<CardAppearanceBehaviour.Appearance>() {CardAppearanceBehaviour.Appearance.RareCardBackground};
                cardinfo.baseAttack = 3;
                cardinfo.baseHealth = 1;
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.HeatDependant.ability);
                cardinfo.ConvertToSCI().HeatCost = 5;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "GDragon";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("gorgeousdragon"));
                cardinfo.displayedName = "Gorgeous Dragon";
                cardinfo.appearanceBehaviour=new List<CardAppearanceBehaviour.Appearance>() {CardAppearanceBehaviour.Appearance.RareCardBackground};
                cardinfo.metaCategories=new List<CardMetaCategory>() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare};
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.baseAttack = 1;
                cardinfo.baseHealth = 6;
                cardinfo.abilities.Add(AbilitiesUtils.AbilityBehaviours.ScorchingHeat.ability);
                cardinfo.ConvertToSCI().Temple=Enums.SawyerTemples.Martel;
                cardinfo.ConvertToSCI().HeatCost=7;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "CoalFiend";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("strangebatpixel"));
                cardinfo.displayedName = "Coal Fiend";
                cardinfo.metaCategories=new List<CardMetaCategory>() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.baseAttack = 2;
                cardinfo.baseHealth = 1;
                cardinfo.abilities=new List<Ability>(){AbilitiesUtils.AbilityBehaviours.HeatChasing.ability, Ability.Evolve};
                cardinfo.ConvertToSCI().Temple=Enums.SawyerTemples.Martel;
                cardinfo.ConvertToSCI().HeatCost=4;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "SteamBot";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("steambot"));
                cardinfo.displayedName = "Steam Bot";
                cardinfo.metaCategories=new List<CardMetaCategory>() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable};
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.baseAttack = 1;
                cardinfo.baseHealth = 3;
                cardinfo.abilities=new List<Ability>(){AbilitiesUtils.AbilityBehaviours.SwappingPower.ability};
                cardinfo.ConvertToSCI().Temple=Enums.SawyerTemples.Martel;
                cardinfo.ConvertToSCI().HeatCost=5;
                allcards.Add(cardinfo);
            }
            {
                var cardinfo = ScriptableObject.CreateInstance<SawyerCardInfo>();
                cardinfo.name = "PorcelaineGolem";
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("noart"));
                cardinfo.displayedName = "Porcelaine Golem";
                cardinfo.metaCategories=new List<CardMetaCategory>() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare};
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.baseAttack = 0;
                cardinfo.baseHealth = 1;
                cardinfo.abilities=new List<Ability>(){AbilitiesUtils.AbilityBehaviours.OutOfPorcelain.ability};
                cardinfo.ConvertToSCI().Temple=Enums.SawyerTemples.Martel;
                cardinfo.ConvertToSCI().HeatCost=2;
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