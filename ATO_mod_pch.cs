using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;
using UnityEngine;



using System.Reflection;

using System.Collections;

namespace ATO_mod_pch
{
    [BepInPlugin("me.pxh1998.plugin.ato", "ATO_mod_pch", "1.0.0")]
    public class ATO_mod_pch : BaseUnityPlugin
    {

        //初始化
        /*      private const string MyGUID = "me.pxh1998.plugin.test";
                private const string PluginName = "Thronefall_pch";
                private const string VersionString = "1.0.0";*/
        public static Harmony Harmony = new Harmony("me.pxh1998.plugin.ftk");

        public static ATO_mod_pch Instance;

		//获取怪物牌时生成一个flag, 使用完重置
		public static bool isNpcCard = false;
		public static string heroClass = string.Empty;   //英雄职业

		//工具方法代码
		public Hero GetRandomHero(Hero[] arr)
		{
			System.Random ran = new System.Random();
			int n = ran.Next(arr.Length - 1);
			return arr[n];
		}


		//测试新类实例, 注释上方的不要动

		// 在插件启动时会直接调用Awake()方法
		void Awake()
        {
            // 使用Debug.Log()方法来将文本输出到控制台
            ATO_mod_pch.Harmony.PatchAll();
            Debug.Log("Hello, world! " + Application.unityVersion);
            Instance = this;
        }
        // 在所有插件全部启动完成后会调用Start()方法，执行顺序在Awake()后面；
        void Start()
        {
            //Debug.Log("这里是Start()方法中的内容!");
        }

        // 插件启动后会一直循环执行Update()方法，可用于监听事件或判断键盘按键，执行顺序在Start()后面
        void Update()
        {
            var key = new BepInEx.Configuration.KeyboardShortcut(KeyCode.F9);

            if (key.IsDown())
            {
                Debug.Log("当你看到这条消息时，表示按下了F9");

                // 调用远程方法，向所有连接的玩家广播
            }
        }
        // 在插件关闭时会调用OnDestroy()方法
        void OnDestroy()
        {
            //Debug.Log("当你看到这条消息时，就表示我已经被关闭一次了!");
            //SyncAttactAoeRPC(DummyDamageInfo.Serialize(combatAoeReworksDDI_list[0]));
        }



	}


	/*-----------------------------------------------------------------------Patch部分--------------------------------------------------------------------------*/

	//每回合回能+1

	[HarmonyPatch(typeof(Character), "GetEnergyTurn")]
	public class Character_GetEnergyTurn_Patch
	{
		[HarmonyPrefix]
		public static bool Character_GetEnergyTurn_Prefix(Character __instance, ref int __result)
		{
			int num = __instance.GetAuraStatModifiers(__instance.EnergyTurn, Enums.CharacterStat.Energy) + __instance.GetItemStatModifiers(Enums.CharacterStat.EnergyTurn);
			if (num < 0)
			{
				num = 0;
			}

			__result = num + 1;
			return false;

		}

	}

	//测试从卡牌生成时修改附魔  可以改变卡牌显示   需要搭配前置补丁  和  后置补丁   配和给怪物牌    修改附魔持续回合

	[HarmonyPatch(typeof(CardData), "SetTarget")]
	public class CardData_SetTarget_Patch_Pre
	{
		[HarmonyPrefix]
		public static bool CardData_SetTarget_Patch_Prefix()
		{
			return true;
		}

	}


	[HarmonyPatch(typeof(MatchManager), "GetCardData")]
	public class MatchManager_GetCardData_Patch_Post         //暂时未启用
	{
		[HarmonyPostfix]
		public static void MatchManager_GetCardData_Patch_Postfix(Globals __instance, ref CardData __result)
		{
			//Traverse CardDataTraverse = Traverse.Create(__result);
			//ItemData item = CardDataTraverse.Field("item").GetValue<ItemData>();
			return;

			if (__result != null)
			{
				string itemClass = Enum.GetName(typeof(Enums.CardClass), __result.CardClass).ToLower();

				// 无限附魔
				if (__result.ItemEnchantment != null && itemClass != "monster" && __result.ItemEnchantment.DestroyAfterUses >= 1)
				{
					__result.ItemEnchantment.DestroyAfterUses = 66;
				}

				if (__result.ItemEnchantment != null && itemClass != "monster")
				{
					if (__result.ItemEnchantment.DestroyStartOfTurn == true || __result.ItemEnchantment.DestroyEndOfTurn == true && __result.ItemEnchantment.DestroyAfterUses == 0)
					{
						__result.ItemEnchantment.DestroyEndOfTurn = false;
						__result.ItemEnchantment.DestroyStartOfTurn = false;
						//__result.ItemEnchantment.DestroyAfterUses = -1;
						//Debug.Log("卡牌: " + __result.Item.Id.ToString());
					}
				}

				//如果是给玩家发送怪物牌   修改参数
				if (ATO_mod_pch.isNpcCard)
				{
					__result.CardClass = Enums.CardClass.Warrior;
					__result.Playable = true;
				}
			}
		}

	}

	[HarmonyPatch(typeof(Globals), "GetCardData")]
	public class Globals_GetCardData_Patch_Post
	{
		[HarmonyPostfix]
		public static void Globals_GetCardData_Patch_Postfix(Globals __instance, ref CardData __result)
		{
			//Traverse CardDataTraverse = Traverse.Create(__result);
			//ItemData item = CardDataTraverse.Field("item").GetValue<ItemData>();

			if(__result != null)
            {
				string itemClass = Enum.GetName(typeof(Enums.CardClass), __result.CardClass).ToLower();

				// 无限附魔
				if (__result.ItemEnchantment != null && itemClass != "monster" && __result.ItemEnchantment.DestroyAfterUses >= 3)
				{
					__result.ItemEnchantment.DestroyAfterUses = 77;
				}
				if (__result.ItemEnchantment != null && itemClass != "monster")  //大部分附魔次数小于3的强力附魔 修改为每回合可以触发3次
				{
					if(__result.ItemEnchantment.DestroyAfterUses < 3 && __result.ItemEnchantment.DestroyAfterUses > 0)
                    {
						__result.ItemEnchantment.DestroyAfterUses = 0;
						__result.ItemEnchantment.TimesPerTurn = 3;
                    }
				}
				if (__result.ItemEnchantment != null && itemClass != "monster")   //持续一回合的附魔改为无限
				{
					if (__result.ItemEnchantment.DestroyStartOfTurn == true || __result.ItemEnchantment.DestroyEndOfTurn == true && __result.ItemEnchantment.DestroyAfterUses == 0)
					{
						__result.ItemEnchantment.DestroyEndOfTurn = false;
						__result.ItemEnchantment.DestroyStartOfTurn = false;
						//__result.ItemEnchantment.DestroyAfterUses = 0;
						//Debug.Log("卡牌: " + __result.Item.Id.ToString());
					}
				}
				//怪物卡部分  放到 [HarmonyPatch(typeof(CardItem), "SetCard")]里. 因为需要对使用角色进行判断.





				//如果是给玩家发送怪物牌   修改参数 (这一段可以在战斗中改变描述, 但是添加到牌库的怪物牌描述不会变)
					if (ATO_mod_pch.isNpcCard)
                {
					//__result.CardClass = Enums.CardClass.MagicKnight;
					//string targetDisplayText = __result.Target;
					//__result.Target = targetDisplayText.Replace("英雄", "怪物");
					Debug.Log("[Globals][GetCardData]卡牌target : " + __result.Target);
					__result.Playable = true;
                }
			}
        }

	}

	//测试显示卡牌的内容 后置补丁              修改怪物卡的附魔效果, 一部分放在这个patch里.

	[HarmonyPatch(typeof(CardItem), "SetCard")]
	public class CardItem_SetCard_Patch
	{
		[HarmonyPostfix]
		public static void CardItem_SetCard_Patch_Postfix(ref CardItem __instance)
		{
			Traverse CardItemTraverse = Traverse.Create(__instance);
			CardData cardData = CardItemTraverse.Field("cardData").GetValue<CardData>();
			Hero theHero = CardItemTraverse.Field("theHero").GetValue<Hero>();
			//卡牌类型(底部文本)   例如: [寒冰法术][法术]
			TMP_Text typeTextTM = CardItemTraverse.Field("typeTextTM").GetValue<TMP_Text>();
			//测试文本
			TMP_Text requireTextTM = CardItemTraverse.Field("requireTextTM").GetValue<TMP_Text>();
			TMP_Text targetTextTM = CardItemTraverse.Field("targetTextTM").GetValue<TMP_Text>();

			if (cardData != null)
			{
				if (requireTextTM != null)
				{
					//Debug.Log("[SetCard.requireTextTM]测试文本: " + requireTextTM.text);
				}
				//修改卡牌描述
				if (theHero != null && cardData.CardClass == Enums.CardClass.Monster)
				{
					string targetDisplayText = cardData.Target;
					cardData.Target = targetDisplayText.Replace("英雄", "怪物");
					cardData.Playable = true;
					if (targetTextTM != null)
					{
						targetDisplayText = targetTextTM.text;
						targetTextTM.text = targetDisplayText.Replace("英雄", "怪物");
					}
				}
				if (targetTextTM != null && !ATO_mod_pch.isNpcCard)
				{
					//targetTextTM.text.Replace("英雄", "怪物");
					Debug.Log("[SetCard.targetTextTM]测试文本: " + targetTextTM.text);
					if (theHero != null)
					{
						Debug.Log("[SetCard.targetTextTM]测试文本: " + targetTextTM.text + " 角色: " + theHero.ClassName + " 英雄名字: " + theHero.SourceName + " 英雄职业: " + theHero.HeroData.HeroClass);
						Debug.Log("[SetCard.addTagFromHero]测试文本: " + " 英雄职业: " + theHero.HeroData.HeroClass + " 英雄副职业: " + theHero.HeroData.HeroSubClass.HeroClassSecondary + " 英雄ID: " + theHero.Id + " 英雄名字: " + theHero.SourceName);
					}
				}

				//使玩家使用怪物卡附魔时, 附魔增益效果目标从怪物 -> 英雄    (已测试)
				if (theHero != null && cardData.ItemEnchantment != null && cardData.CardClass == Enums.CardClass.Monster)
				{
					if (cardData.ItemEnchantment.ItemTarget == Enums.ItemTarget.LowestFlatHpEnemy)
                    {
						cardData.ItemEnchantment.ItemTarget = Enums.ItemTarget.LowestFlatHpHero;
						Debug.Log("[SetCard.cardData.ItemEnchantment.ItemTarget]测试文本 卡牌: " + cardData.Id);
					}
				}

				//给怪物卡按照英雄职业添加标签.
				if(theHero != null && cardData.CardClass == Enums.CardClass.Monster)
                {
					CardItem_SetCard_Patch.addTagFromHero(theHero, ref cardData);
					Debug.Log("[SetCard.addTagFromHero]测试文本: " +  " 英雄职业: " + theHero.HeroData.HeroClass + " 英雄副职业: " + theHero.HeroData.HeroSubClass.HeroClassSecondary);
					__instance.typeText.gameObject.SetActive(true);
				}


				//获取卡牌类型
				string itemType = Enum.GetName(typeof(Enums.CardType), cardData.CardType).ToLower();

				if (cardData.CardType != Enums.CardType.None && cardData.CardClass != Enums.CardClass.Item)
				{
					StringBuilder stringBuilder = new StringBuilder();

					switch (itemType)
					{
						default:
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2>{0}</size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "cold_spell":
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#00FFFF>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "fire_spell":
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=red>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "lightning_spell":
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#FFFF66>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "mind_spell":  //心灵法术
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#FF66B2>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "shadow_spell":  //暗影法术
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#000000>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "holy_spell":  //神圣法术
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#FFFF99>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "curse_spell":  //诅咒法术
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#B266FF>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;

						case "healing_spell":  //治疗法术
							stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#00FF00>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardData.CardType), "")));
							break;
					}

					if (cardData.CardTypeAux.Length != 0)
					{
						foreach (Enums.CardType cardtype in cardData.CardTypeAux)
						{
							string itemTypeAux = Enum.GetName(typeof(Enums.CardType), cardtype).ToLower();
							switch (itemTypeAux)
							{
								default:
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2>{0}</size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "cold_spell":
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#00FFFF>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "fire_spell":
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=red>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "lightning_spell":
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#FFFF66>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "mind_spell":  //心灵法术
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#FF66B2>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "shadow_spell":  //暗影法术
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#000000>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "holy_spell":  //神圣法术
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#FFFF99>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "curse_spell":  //诅咒法术
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#B266FF>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;

								case "healing_spell":  //治疗法术
									stringBuilder.Append(string.Format(" <size=-.2>[</size><size=-.2><color=#00FF00>{0}</color></size> <size=-.2>]</size>", Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), cardtype), "")));
									break;
							}
						}

					}
					if (stringBuilder != null && typeTextTM != null)
					{
						typeTextTM.text = stringBuilder.ToString();
						//Debug.Log("[SetCard]卡牌类型: " + typeTextTM.text);
						__instance.typeTextImage.gameObject.SetActive(true);
					}
				}
				else
				{
					typeTextTM.text = "";
					//if (this.typeTextImage.gameObject.activeSelf)
					//{
					//	this.typeTextImage.gameObject.SetActive(false);
					//}
				}
			}
		}

		//使获得的怪物卡按照英雄职业添加对应标签
		public static void addTagFromHero(Hero theHero, ref CardData cardData)
        {
			if(cardData.CardType != Enums.CardType.Enchantment)
            {
				bool specialHero = false;

				//特殊处理部分
				if (theHero.Id.Contains("warden"))             // 布理 典狱长 战士 额外添加 [技能]
                {
					specialHero = true;
					cardData.CardType = Enums.CardType.Melee_Attack;
					cardData.CardTypeAux = new Enums.CardType[3];
					cardData.CardTypeAux[0] = Enums.CardType.Attack;
					cardData.CardTypeAux[1] = Enums.CardType.Defense;
					cardData.CardTypeAux[2] = Enums.CardType.Skill;
				}
				else if (theHero.Id.Contains("minstrel"))      //古斯塔夫 流浪乐师 游侠  额外添加[吟唱法术][法术]  移除[技能]
				{
					specialHero = true;
					cardData.CardType = Enums.CardType.Ranged_Attack;
					cardData.CardTypeAux = new Enums.CardType[4];
					cardData.CardTypeAux[0] = Enums.CardType.Attack;
					cardData.CardTypeAux[1] = Enums.CardType.Small_Weapon;
					cardData.CardTypeAux[2] = Enums.CardType.Song;
					cardData.CardTypeAux[3] = Enums.CardType.Spell;
				}
				else if (theHero.Id.Contains("warlock"))      //塞克  术士  法师   额外添加[暗影法术][诅咒法术]   移除[火焰法术][闪电法术]
                {
					specialHero = true;
					cardData.CardType = Enums.CardType.Spell;
					cardData.CardTypeAux = new Enums.CardType[4];
					cardData.CardTypeAux[0] = Enums.CardType.Cold_Spell;
					cardData.CardTypeAux[1] = Enums.CardType.Shadow_Spell;
					cardData.CardTypeAux[2] = Enums.CardType.Curse_Spell;
					cardData.CardTypeAux[3] = Enums.CardType.Book;
				}
				else if (theHero.Id.Contains("alchemist"))      //伯纳德  炼金术士  治疗者   额外添加[瓶子]   移除[神圣法术]
				{
					specialHero = true;
					cardData.CardType = Enums.CardType.Healing_Spell;
					cardData.CardTypeAux = new Enums.CardType[4];
					cardData.CardTypeAux[0] = Enums.CardType.Spell;
					cardData.CardTypeAux[1] = Enums.CardType.Defense;
					cardData.CardTypeAux[2] = Enums.CardType.Skill;
					cardData.CardTypeAux[3] = Enums.CardType.Flask;
				}

				if (specialHero)
                {
					return;
                }
				
				if (theHero.HeroData.HeroSubClass.HeroClassSecondary == Enums.HeroClass.None)
                {
					switch (theHero.HeroData.HeroClass)
					{
						default:
							break;

						case Enums.HeroClass.Warrior:   //战士        [近战攻击][攻击][防御][技能]     
							cardData.CardType = Enums.CardType.Melee_Attack;
							cardData.CardTypeAux = new Enums.CardType[3];
							cardData.CardTypeAux[0] = Enums.CardType.Attack;
							cardData.CardTypeAux[1] = Enums.CardType.Defense;
							cardData.CardTypeAux[2] = Enums.CardType.Skill;
							break;

						case Enums.HeroClass.Scout:     //游侠        远程攻击/攻击/小型武器/技能
							cardData.CardType = Enums.CardType.Ranged_Attack;
							cardData.CardTypeAux = new Enums.CardType[3];
							cardData.CardTypeAux[0] = Enums.CardType.Attack;
							cardData.CardTypeAux[1] = Enums.CardType.Small_Weapon;
							cardData.CardTypeAux[2] = Enums.CardType.Skill;
							break;

						case Enums.HeroClass.Mage:      //法师        法术/寒冰法术/火焰法术/闪电法术/书
							cardData.CardType = Enums.CardType.Spell;
							cardData.CardTypeAux = new Enums.CardType[4];
							cardData.CardTypeAux[0] = Enums.CardType.Cold_Spell;
							cardData.CardTypeAux[1] = Enums.CardType.Fire_Spell;
							cardData.CardTypeAux[2] = Enums.CardType.Lightning_Spell;
							cardData.CardTypeAux[3] = Enums.CardType.Book;
							break;

						case Enums.HeroClass.Healer:    //治疗者      治疗法术/法术/神圣法术/防御/技能
							cardData.CardType = Enums.CardType.Healing_Spell;
							cardData.CardTypeAux = new Enums.CardType[4];
							cardData.CardTypeAux[0] = Enums.CardType.Spell;
							cardData.CardTypeAux[1] = Enums.CardType.Defense;
							cardData.CardTypeAux[2] = Enums.CardType.Skill;
							cardData.CardTypeAux[3] = Enums.CardType.Holy_Spell;
							break;
					}
				}
			}
        }


	}








	//修改持续一回合的附魔   DestroyEndOfTurn和DestroyStartOfTurn   需要patch卡牌来源  (这破方法调用的是卡牌源数值而非动态修改后的)  目前无法判断附魔来源, 怪给自身或玩家上的附魔也会变为无限时间(已解决 已测试).

	[HarmonyPatch(typeof(Globals), "GetItemData")]
	public class Globals_GetItemData_Patch_Pre
	{
		[HarmonyPrefix]
		public static bool Globals_GetItemData_Patch_Prefix(Globals __instance, string id, ref ItemData __result)
		{
			Traverse GlobalsTraverse = Traverse.Create(__instance);
			Dictionary<string, ItemData> _ItemDataSource = GlobalsTraverse.Field("_ItemDataSource").GetValue<Dictionary<string, ItemData>>();


			if (_ItemDataSource != null && _ItemDataSource.ContainsKey(id))
			{
				__result = _ItemDataSource[id];
				if(__instance.GetCardData(__result.Id, false) != null)
                {
					if(__instance.GetCardData(__result.Id, false).CardClass != Enums.CardClass.Monster && __result.IsEnchantment)
                    {
						//Debug.Log("身上的附魔为: " + __result.Id);
						__result.DestroyEndOfTurn = false;
						__result.DestroyStartOfTurn = false;
					}

                }
				return false;
			}
			__result = null;
			return false;
		}

	}



	//[HarmonyPatch(typeof(Globals), "GetCardData")]
	//public class Globals_GetCardData_Patch_prefix
	//{
	//	[HarmonyPrefix]
	//	public static bool Globals_GetCardData_Prefix(Globals __instance, ref string id, bool instantiate = true, ref CardData __result)
	//	{
	//		id = Functions.Sanitize(id, true).ToLower().Trim().Split("_", StringSplitOptions.None)[0];
	//		if (!(id != "") || !this._Cards.ContainsKey(id))
	//		{
	//			return null;
	//		}
	//		if (instantiate)
	//		{
	//			CardData cardData = Object.Instantiate<CardData>(this._Cards[id]);
	//			this._InstantiatedCardData.Add(cardData);
	//			return cardData;
	//		}
	//		__result = __instance._Cards[id];
	//		return false
	//	}
	//}






	// 无限附魔次数测试   (可以设置是否包括装备 敌人是否生效)           //暂时弃用, 不如另一个  而且存在报错隐患

	[HarmonyPatch(typeof(ItemCombatIcon), "SetTimesExecuted")]

    public class ItemCombatIcon_SetTimesExecuted_Patch
    {
        [HarmonyPrefix]
        public static bool ItemCombatIcon_SetTimesExecuted_Prefix(ref ItemCombatIcon __instance, int times, bool doAnim = true)
        {
			return true;
			//Traverse.Create(__instance).Method("CardData").GetValue<object>();
			Traverse playerTraverse = Traverse.Create(__instance);
			CardData cardData = playerTraverse.Field("cardData").GetValue<CardData>();
			Hero theHero = playerTraverse.Field("theHero").GetValue<Hero>();
			NPC theNPC = playerTraverse.Field("theNPC").GetValue<NPC>();

			//是否启用无限附魔
			bool pxh_Infinite_Enchantment = false;

			//获取卡牌类型
			string itemType = Enum.GetName(typeof(Enums.CardType), cardData.CardType).ToLower();

			//次数类附魔
			//cardData.Item.DestroyAfterUses = 1;

			if (__instance.animatedUse == null)
			{
				return false;
			}
			__instance.animatedUse.gameObject.SetActive(false);
			if (times < 0)
			{
				times = 0;
			}
			if (cardData != null)
			{
				int num = 0;
				if (cardData.Item != null)
				{
					if (cardData.Item.TimesPerCombat > 0)
					{
						num = cardData.Item.TimesPerCombat;
					}
					else
					{
						num = cardData.Item.TimesPerTurn;
					}
				}
				else if (cardData.ItemEnchantment != null && MatchManager.Instance != null && cardData.ItemEnchantment.DestroyAfterUses > 0)
				{
					num = cardData.ItemEnchantment.DestroyAfterUses;
					if (theHero != null)
					{
						if (num != 1)
						{
							//cardData.ItemEnchantment.DestroyAfterUses = 66;
							//num = 66;
						}
						times = MatchManager.Instance.EnchantmentExecutedTimes(theHero.Id, cardData.ItemEnchantment.Id);
						//cardData.Item.DestroyEndOfTurn = false;
						//cardData.Item.DestroyStartOfTurn = false;
					}
					else if (theNPC != null)
					{
						times = MatchManager.Instance.EnchantmentExecutedTimes(theNPC.Id, cardData.ItemEnchantment.Id);
					}
				}

				if (pxh_Infinite_Enchantment && itemType != "weapon" && itemType != "armor" && itemType != "jewelry" && itemType != "accesory")
				{
					num = 99;
				}


				if (num == 0)
				{
					if (__instance.timesExecuted.gameObject.activeSelf)
					{
						__instance.timesExecuted.gameObject.SetActive(false);
						return false;
					}
				}
				else
				{
					if (!__instance.timesExecuted.gameObject.activeSelf)
					{
						__instance.timesExecuted.gameObject.SetActive(true);
					}
					StringBuilder stringBuilder = new StringBuilder();
					if (times == num)
					{
						stringBuilder.Append("<color=#F3404E>");
						stringBuilder.Append(0);
						stringBuilder.Append("</color>");
					}
					else if (times > 0 && times < num)
					{
						stringBuilder.Append("<color=#FFFFFF>");
						stringBuilder.Append(num - times);
						stringBuilder.Append("</color>");
					}
					else
					{
						stringBuilder.Append(num);
					}
					stringBuilder.Append("/");
					stringBuilder.Append(num);
					__instance.timesExecuted.text = stringBuilder.ToString();
					if (times != 0 && doAnim && !__instance.animatedUse.gameObject.activeSelf)
					{
						__instance.animatedUse.gameObject.SetActive(true);
					}
				}
			}
			return false;
        }
    }




	//测试怪物死亡随机发牌给角色   如果在非角色回合死亡会发牌给任意一个角色(无论是否存活)

	[HarmonyPatch(typeof(CharacterItem), "KillCharacterCO")]
	public class CharacterItem_KillCharacterCO_Patch
	{
		[HarmonyPostfix]
		public static void CharacterItem_KillCharacterCO_Patch_Postfix(CharacterItem __instance)
		{
			Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 方法触发" );

			if (__instance != null)
            {
				Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 判断1触发");

				MatchManager matchManagerInstance = MatchManager.Instance;
				Traverse matchManagerInstanceTraverse = Traverse.Create(matchManagerInstance);
				Traverse CharacterItemTraverse = Traverse.Create(__instance);

				//Hero theHero = matchManagerInstanceTraverse.Method("theHero").GetValue<Hero>();
				int heroActive = matchManagerInstanceTraverse.Field("heroActive").GetValue<int>();
				Hero[] TeamHero = matchManagerInstanceTraverse.Field("TeamHero").GetValue<Hero[]>();
				List<string>[] HeroHand = matchManagerInstanceTraverse.Field("HeroHand").GetValue<List<string>[]>();

				//bool isDying = CharacterItemTraverse.Field("isDying").GetValue<bool>();
				bool isHero = CharacterItemTraverse.Field("isHero").GetValue<bool>();
				NPC _npc = CharacterItemTraverse.Field("_npc").GetValue<NPC>();

                System.Random randomCard = new System.Random();
				System.Random randomHero = new System.Random();
				int randomCardIndex;
				int randomHeroIndex;

				if (!isHero && !_npc.Alive && _npc != null)            //npc是否为null判断, 有些卡牌会先触发战斗结束再触发击杀事件, 但是角色已经destory了.
				{
					bool noHeroActive = true;
					int npcCardNum = _npc.Cards.Count;
					Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 判断2触发");
					for (int i = 0; i < 4; i++)
					{
						Hero hero = MatchManager.Instance.GetHero(i);
						Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 判断3触发");
						if (hero != null && heroActive != -1)
						{
							Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 判断4触发" + " heroActive: " + heroActive);
							if (hero.Alive && hero == TeamHero[heroActive])
                            {
								Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 英雄回合击杀触发");
								noHeroActive = false;
								matchManagerInstanceTraverse.Field("theHero").SetValue(hero);
								ATO_mod_pch.heroClass = hero.ClassName;
								//Debug.Log("hero ID: " + matchManagerInstanceTraverse.Field("theHero").GetValue<Hero>().Id);
								if (npcCardNum > 0)
								{
									ATO_mod_pch.isNpcCard = true;
									randomCardIndex = randomCard.Next(0, npcCardNum);
									for (int EnchCardIndex = 0; EnchCardIndex < npcCardNum; EnchCardIndex++)    //如果怪手中有附魔卡优先给玩家附魔卡   待添加: 如果多张附魔未进行判断, 暂时只添加最后一张.
                                    {
										if(Globals.Instance.GetCardData(_npc.Cards[EnchCardIndex], false).CardType == Enums.CardType.Enchantment || Globals.Instance.GetCardData(_npc.Cards[EnchCardIndex], false).ItemEnchantment != null)
                                        {
											randomCardIndex = EnchCardIndex;
											Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 怪物有附魔卡触发");
										}
                                    }
									//hero.Cards.Add(_npc.Cards[randomCardIndex]);    //可用, 但是无法保存添加的卡牌
									AtOManager.Instance.AddCardToHero(heroActive, _npc.Cards[randomCardIndex]);
									AtOManager.Instance.SideBarRefreshCards(heroActive);
									//AtOManager.Instance.SaveCraftedCard(heroActive, _npc.Cards[randomCardIndex]);    //可能会导致保存后回合结束 待测试  (已测试, 是这个方法导致的, 需要寻找更安全的存储方法)
									if(HeroHand[heroActive].Count <= 0)      //防止在抽牌前击杀导致进行后续抽弃牌操作时 castcard的movenext方法出现问题.
                                    {
										MatchManager.Instance.GenerateNewCard(1, MatchManager.Instance.CreateCardInDictionary(_npc.Cards[randomCardIndex], "", false), true, Enums.CardPlace.TopDeck, null, null, i, true, 0);
										Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 英雄回合抽牌前击杀触发. 击杀时英雄手牌为: " + HeroHand[heroActive].Count);
									}
                                    else 
									{
										MatchManager.Instance.GenerateNewCard(1, MatchManager.Instance.CreateCardInDictionary(_npc.Cards[randomCardIndex], "", false), true, Enums.CardPlace.Hand, null, null, i, true, 0);
									}	
									ATO_mod_pch.heroClass = string.Empty;
									ATO_mod_pch.isNpcCard = false;
									//MatchManager.Instance.ItemTraitActivated(true);
									//MatchManager.Instance.CreateLogCardModification(_npc.Cards[0], hero);
									Debug.Log("给英雄单位发牌: " + _npc.Cards[randomCardIndex] + ", 敌人卡牌数量: " + npcCardNum + "给与英雄怪物的第几张牌: " + randomCardIndex + " 当前行动的英雄: " + hero.Id);
								}
							}
						}
					}
					if (noHeroActive)
					{
						if (npcCardNum > 0)
						{
							Debug.Log("CharacterItem_KillCharacterCO_Patch_Postfix 非----英雄回合击杀触发");
							randomHeroIndex = randomHero.Next(0, TeamHero.Length);
							Hero hero = MatchManager.Instance.GetHero(randomHeroIndex);
							ATO_mod_pch.isNpcCard = true;
							randomCardIndex = randomCard.Next(0, npcCardNum);
							//hero.Cards.Add(_npc.Cards[randomCardIndex]);
							AtOManager.Instance.AddCardToHero(randomHeroIndex, _npc.Cards[randomCardIndex]);
							AtOManager.Instance.SideBarRefreshCards(randomHeroIndex);
							//AtOManager.Instance.SaveCraftedCard(randomHeroIndex, _npc.Cards[randomCardIndex]);    //可能会导致保存后回合结束 待测试   (已测试, 是这个方法导致的, 需要寻找更安全的存储方法)
							//AtOManager.Instance.SaveGameTurn();  //无法保存回合内添加的卡牌到牌库
							MatchManager.Instance.GenerateNewCard(1, MatchManager.Instance.CreateCardInDictionary(_npc.Cards[randomCardIndex], "", false), true, Enums.CardPlace.TopDeck, null, null, randomHeroIndex, true, 0);
							ATO_mod_pch.heroClass = string.Empty;
							ATO_mod_pch.isNpcCard = false;
							//MatchManager.Instance.ItemTraitActivated(true);
							//MatchManager.Instance.CreateLogCardModification(_npc.Cards[0], hero);
							Debug.Log("给英雄单位发牌: " + _npc.Cards[0] + ", 敌人卡牌数量: " + npcCardNum + " 获得卡牌的英雄: " + hero.Id);
						}
					}

				}
			}
		}

	}


}
