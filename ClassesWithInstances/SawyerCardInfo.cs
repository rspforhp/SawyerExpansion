using System;
using System.Collections.Generic;
using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;
using SawyerExpansion.Utils;

namespace SawyerExpansion.ClassesWithInstances
{
  
   
        public class SawyerCardInfo : CardInfo
        {

           
            internal Dictionary<string, object> Fields=new Dictionary<string, object>();

            private void Awake()
            {
                object isUnused;
                if (!Fields.TryGetValue("Heat", out isUnused))
                {
                    Fields.Add("Heat", default);
                    Fields.Add("Temple", Enums.SawyerTemples.None);
                    Fields.Add("Ore", default); 
                }
               
            }
            
            
            public Enums.SawyerTemples Temple
            {
                get
                {
                    return (Enums.SawyerTemples)Fields["Temple"];
                }
                set
                {
                    Fields["Temple"] = value;
                }
            }
            
            public Tuple<Enums.OresEnum, int> OreCost
            {
                get
                {
                    return (Tuple<Enums.OresEnum, int>)Fields["Ore"];
                }
                set
                {
                    Fields["Ore"] = value;
                }
            }

            public int HeatCost
            {
                get
                {
                    return (int)Fields["Heat"];
                }
                set
                {
                    Fields["Heat"] = value;
                }
            }
        }
    }