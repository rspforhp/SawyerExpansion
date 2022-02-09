using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;
using SawyerExpansion.ClassesWithInstances;

namespace SawyerExpansion.ExtendClasses
{
    public static class Extends
    {
        public static SawyerCardInfo ConvertToSCI(this CardInfo cardInfo)
        {
            return (SawyerCardInfo) cardInfo;
        }
    }
}