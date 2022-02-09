using System;
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
            SawyerCardInfo returnvalue;
            try
            {
                returnvalue=(SawyerCardInfo) cardInfo;
                return returnvalue;
            }
            catch
            {
                return ScriptableObject.CreateInstance<SawyerCardInfo>();

            }
        }
        
        
        public static T Cast<T>(this object value)
        {
            return (T)value;
        }
        
    }
}