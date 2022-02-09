using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using DiskCardGame;
using GBC;
using HarmonyLib;
using BepInEx;

namespace SawyerExpansion.Utils
{
    public static class ImageUtils
    {
    
        public static Sprite[] heatTextures;

        
            public static Texture2D LoadTexture(string NameOfFile)
            {
                byte[] imgBytes =
                    System.IO.File.ReadAllBytes(Plugin.PluginDetails.Path+"Artwork\\" + NameOfFile + ".png");
                if (imgBytes.Length == 0)
                    throw new NullReferenceException("Image with this name was not found");
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(imgBytes);
                tex.filterMode = FilterMode.Point;
                return tex;
            }

            public static Sprite ConvertToSprite(Texture2D tex, int divppu=1)
            {
                var sprite = Sprite.Create( tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f/divppu);
                return sprite;
            }
            
        
    }
}