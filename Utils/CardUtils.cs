using System;
using APIPlugin;
using System.Collections.Generic;
using System.Reflection;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using SawyerExpansion.ClassesWithInstances;
using SawyerExpansion.ExtendClasses;
using UnityEngine;

namespace SawyerExpansion.Utils
{
    public static class CardUtils
    {

   

        public static CardInfo New(
            string modPrefix,
            string name,
            string displayName,
            int attack,
            int health,
            string description = null)
        {
            CardInfo instance = ScriptableObject.CreateInstance<SawyerCardInfo>();
            instance.name = !name.StartsWith(modPrefix) ? modPrefix + "_" + name : name;
            instance.SetBasic(displayName, attack, health, description);
            Assembly callingAssembly = Assembly.GetCallingAssembly();
           
            instance.SetExtendedProperty("CallStackModGUID", (object) TypeManager.GetModIdFromCallstack(callingAssembly));
            CardManager.Add(modPrefix, instance);
            return instance;
        }

        
        public static List<CardInfo> AddCards()
        {
            var allcards=new List<CardInfo>();
            {
                var cardinfo=New(Plugin.PluginDetails.PluginGuid, "Coal", "Coal Piece", 0,1).AddAbilities(AbilitiesUtils.AbilityBehaviours.Coal.ability).AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground).AddMetaCategories(CardMetaCategory.GBCPack,CardMetaCategory.GBCPlayable).SetPixelPortrait("Artwork\\coal.png");
                cardinfo.temple = CardTemple.Wizard;
                cardinfo.ConvertToSCI().HeatCost = 1;
                cardinfo.ConvertToSCI().Temple = Enums.SawyerTemples.Martel;
                Plugin.PluginDetails.Log.LogInfo("Added card with id "+cardinfo.name);
            }
            /*
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
                cardinfo.pixelPortrait =Utils.ImageUtils.ConvertToSprite(Utils.ImageUtils.LoadTexture("wip"));
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
            */
            foreach (var card in allcards)
            {
                CardManager.Add(Plugin.PluginDetails.PluginGuid,card);
            }
            return allcards;
        }
    }
}