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
          ImageUtils.heatTextures=new Sprite[1];
            for (int i = 1; i < 11; i++)
            {
                {
                    Array.Resize(ref ImageUtils.heatTextures, ImageUtils.heatTextures.Length+1);
                    {
                        var tex =ImageUtils.LoadTexture(i+"_Heat");
                        ImageUtils.heatTextures[i]=Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                    } 
                }
            }
        }
    }
}