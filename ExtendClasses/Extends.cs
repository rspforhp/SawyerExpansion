using System;
using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;
using JetBrains.Annotations;
using SawyerExpansion.ClassesWithInstances;
using SawyerExpansion.Utils;

namespace SawyerExpansion.ExtendClasses
{
    public static class Extends
    {
        
        [CanBeNull]
        public static SawyerCardInfo ConvertToSCI(this CardInfo cardInfo)
        {
            SawyerCardInfo returnvalue;
            try
            {
                returnvalue=(SawyerCardInfo) cardInfo;
                return returnvalue;
            }
            catch
            {
                var value=ScriptableObject.CreateInstance<SawyerCardInfo>();
                return value;

            }
        }
        
        
        public static T Cast<T>(this object value)
        {
            return (T)value;
        }
        
    }
}