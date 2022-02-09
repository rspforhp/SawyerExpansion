using System;
using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;
using SawyerExpansion.ClassesWithInstances;
using SawyerExpansion.Utils;

namespace SawyerExpansion
{
    [BepInPlugin(PluginDetails.PluginGuid, PluginDetails.PluginName, PluginDetails.PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        internal static class PluginDetails
        {
            internal const string PluginGuid = "ExpansionTeam.Inscryption.SawyerExpansion";
            internal const string PluginName = "SawyerExpansion";
            internal const string PluginVersion = "0.0.1";
            public static ManualLogSource Log;
            public static string Path;
        }

        private void Awake()
        {
            PluginDetails.Log=this.Logger;
            PluginDetails.Path = this.Info.Location.Replace("SawyerExpansion.dll", "");
            var harmony=new Harmony(PluginDetails.PluginGuid);
            AbilitiesUtils.AddAbilities();
            CardUtils.AddCards();
            harmony.PatchAll();
        }
    }
}