using System.Collections;
using System.Collections.Generic;
using GBC;
using Pixelplacement;
using SawyerExpansion.Utils;
using UnityEngine;

namespace SawyerExpansion.Singleton
{
    	public class SawyerResourceManager : Singleton<SawyerResourceManager>
		{
			private void Awake()
			{
				PlayerHeat = 1;
				PlayerOres=new Dictionary<Enums.OresEnum, int>();
				PlayerOres.Add(Enums.OresEnum.Copper,0);
				PlayerOres.Add(Enums.OresEnum.Tin,0);
				PlayerOres.Add(Enums.OresEnum.Bronze,0);
				PlayerOres.Add(Enums.OresEnum.Iron,0);
			}

			public IEnumerator AddHeat(int amount)
			{
				PlayerHeat += amount;
				yield return this.ShowAddHeat(amount);
				yield break;
			}
			
			public IEnumerator AddOre(int amount, Enums.OresEnum typeofore)
			{
				PlayerOres[typeofore] += amount;
				Plugin.PluginDetails.Log.LogInfo(amount +" " + typeofore);
				Plugin.PluginDetails.Log.LogInfo("current amount of "+typeofore+" is "+PlayerOres[typeofore]);
				//yield return this.ShowAddOre(amount, typeofore); not yet implemented
				yield break;
			}
			
			public IEnumerator ShowAddHeat(int amount)
			{
				int num;
				for (int i = PlayerHeat - amount; i < PlayerHeat; i = num + 1)
				{
					this.ShowHeatCount(i);
					yield return new WaitForSeconds(0.05f);
					num = i;
				}
				this.ShowHeatCount(PlayerHeat-1);
				yield break;
			}
			
			public IEnumerator ShowAddOre(int amount, Enums.OresEnum typeofore)
			{
				int num;
				for (int i = PlayerOres[typeofore] - amount; i < PlayerOres[typeofore]; i = num + 1)
				{
					this.ShowOreCount(i, typeofore);
					yield return new WaitForSeconds(0.05f);
					num = i;
				}
				this.ShowOreCount(PlayerOres[typeofore]-1, typeofore);
				yield break;
			}
			
			public IEnumerator SpendHeat(int amount)
			{
				PlayerHeat -= amount;
				yield return this.ShowSpendHeat(amount);
				yield break;
			}
			
			public IEnumerator SpendOre(int amount, Enums.OresEnum typeofore)
			{
				PlayerOres[typeofore] -= amount;
				Plugin.PluginDetails.Log.LogInfo(amount +" " + typeofore);
				Plugin.PluginDetails.Log.LogInfo("current amount of "+typeofore+" is "+PlayerOres[typeofore]);
				//yield return this.ShowSpendOre(amount, typeofore); not yet implemented
				yield break;
			}
			public IEnumerator ShowSpendHeat(int amount)
			{
				int num;
				for (int i = PlayerHeat + amount; i > PlayerHeat; i = num - 1)
				{
					this.ShowHeatCount(i);
					yield return new WaitForSeconds(0.05f);
					num = i;
				}
				this.ShowHeatCount(PlayerHeat-1);
				yield break;
			}
			
			public IEnumerator ShowSpendOre(int amount, Enums.OresEnum typeofore )
			{
				int num;
				for (int i = PlayerOres[typeofore] + amount; i > PlayerOres[typeofore]; i = num - 1)
				{
					this.ShowOreCount(i, typeofore);
					yield return new WaitForSeconds(0.05f);
					num = i;
				}
				this.ShowOreCount(PlayerOres[typeofore]-1, typeofore);
				yield break;
			}

			public void ShowHeatCount(int numHeat)
			{
				AudioController.Instance.PlaySound2D("chipBlip2", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(Mathf.Min(0.8f + (float)numHeat * 0.05f, 1.2f)), null, null, null, false);
				HeatCount.DisplayValue(numHeat);
				this.BounceRenderer(this.HeatsParent);
			}
			
			public void ShowOreCount(int numHeat, Enums.OresEnum typeofore)
			{
				AudioController.Instance.PlaySound2D("chipBlip2", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(Mathf.Min(0.8f + (float)numHeat * 0.05f, 1.2f)), null, null, null, false);
				OreCount.DisplayValue(numHeat);
				this.BounceRenderer(this.OresParent);
			}
			
			private void BounceRenderer(Transform rendererTransform)
			{
				Vector3 vector = new Vector3(rendererTransform.localPosition.x, 0f);
				Tween.LocalPosition(rendererTransform, vector + Vector3.up * 0.02f, 0.025f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);
				Tween.LocalPosition(rendererTransform, vector, 0.025f, 0.075f, Tween.EaseIn, Tween.LoopType.None, null, null, true);
			}
			
			public Transform HeatsParent;
			public PixelNumeral HeatCount;
			
			public Transform OresParent;
			public PixelNumeral OreCount;
			public int PlayerHeat { get; protected set; }
			public Dictionary<Enums.OresEnum, int> PlayerOres { get; protected set; }
		}
}