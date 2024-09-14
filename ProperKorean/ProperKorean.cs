using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using R2API.Utils;
using RoR2;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.AddressableAssets;

namespace ProperKorean
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ProperKorean : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "DVRP";
        public const string PluginName = "ProperKorean";
        public const string PluginVersion = "1.2.3";

        private static AssetBundle properBundle = AssetBundle.LoadFromMemory(Properties.Resources.properkorean);
        private static TMP_FontAsset properFont;

        private void Awake()
        {
            Log.Init(Logger);

            properFont = properBundle.LoadAsset<TMP_FontAsset>("Assets/properkorean/NotoSansCJKsc-Regular SDF (Korean).asset");
            On.RoR2.UI.HGTextMeshProUGUI.OnCurrentLanguageChanged += HGTextMeshProUGUI_OnCurrentLanguageChanged;

            ImproveSeekerTooltips();
        }

        private static void FixLocalization()
        {
            var properTemp = JObject.Parse(System.Text.Encoding.UTF8.GetString(Properties.Resources.output_korean));
            var proper = JsonConvert.DeserializeObject<Dictionary<string, string>>(properTemp["strings"].ToString());

            foreach (var token in proper)
            {
                if (Language.currentLanguage.GetLocalizedStringByToken(token.Key) != token.Value)
                    Language.currentLanguage.SetStringByToken(token.Key, token.Value);
            }

            Log.LogInfo("ProperKorean :: Fixed localization");
        }

        private static void FixFont()
        {
            var font = LegacyResourcesAPI.LoadAsync<TMP_FontAsset>("TmpFonts/Bombardier/tmpBombDropshadow").WaitForCompletion();

            // Tilde is placed too high up
            var tilde = font.glyphTable.Find(x => x.index == 95);
            tilde.metrics = tilde.metrics with { horizontalBearingY = 40 };

            properFont.fallbackFontAssets.Add(font);

            if (TMP_Settings.fallbackFontAssets[0].name != "tmpBombDropshadow")
                TMP_Settings.fallbackFontAssets.Insert(0, font);

            RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont = properFont;

            Log.LogInfo("ProperKorean :: Fixed font");
        }

        private static void HGTextMeshProUGUI_OnCurrentLanguageChanged(On.RoR2.UI.HGTextMeshProUGUI.orig_OnCurrentLanguageChanged orig)
        {
            orig();

            if (Language.currentLanguageName == "ko")
            {
                FixFont();
                FixLocalization();
            }
        }

        private static void ImproveSeekerTooltips()
        {
            var seeker = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SeekerBody.prefab").WaitForCompletion();
            foreach (var skill in seeker.GetComponents<GenericSkill>())
            {
                switch (skill.skillName)
                {
                    case "SpiritPunch":
                        skill.skillFamily.variants[0].skillDef.keywordTokens = new string[] { "SEEKER_PRIMARY_UPGRADE_TOOLTIP" };
                        break;

                    case "UnseenHand":
                        skill.skillFamily.variants[0].skillDef.keywordTokens = new string[] { "SEEKER_SECONDARY_UPGRADE_TOOLTIP" };
                        skill.skillFamily.variants[1].skillDef.keywordTokens = new string[] { "SEEKER_SECONDARY_ALT1_UPGRADE_TOOLTIP" };
                        break;

                    case "Sojourn":
                        skill.skillFamily.variants[0].skillDef.keywordTokens = new string[] { "SEEKER_UTILITY_UPGRADE_TOOLTIP" };
                        break;

                    case "MeditateUI":
                        skill.skillFamily.variants[0].skillDef.keywordTokens[0] = "SEEKER_SPECIAL_UPGRADE_TOOLTIP";
                        break;
                }
            }
        }
    }
}