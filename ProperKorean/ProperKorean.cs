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
using UnityEngine.ResourceManagement.AsyncOperations;

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
        public const string PluginVersion = "1.2.0";

        private static AssetBundle properBundle = AssetBundle.LoadFromMemory(Properties.Resources.properkorean);
        private static TMP_FontAsset properFont;

        private void Awake()
        {
            Log.Init(Logger);

            properFont = properBundle.LoadAsset<TMP_FontAsset>("Assets/properkorean/NotoSansCJKsc-Regular SDF (Korean).asset");
            On.RoR2.UI.HGTextMeshProUGUI.OnCurrentLanguageChanged += HGTextMeshProUGUI_OnCurrentLanguageChanged;
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
            LegacyResourcesAPI.LoadAsync<TMP_FontAsset>("TmpFonts/Bombardier/tmpBombDropshadow").Completed += x =>
            {
                if (x.Status == AsyncOperationStatus.Succeeded)
                {
                    // Tilde is placed too high up
                    var tilde = x.Result.glyphTable.Find(x => x.index == 95);
                    tilde.metrics = tilde.metrics with { horizontalBearingY = 40 };

                    properFont.fallbackFontAssets.Add(x.Result);

                    if (TMP_Settings.fallbackFontAssets[0].name != "tmpBombDropshadow")
                        TMP_Settings.fallbackFontAssets.Insert(0, x.Result);

                    RoR2.UI.HGTextMeshProUGUI.defaultLanguageFont = properFont;

                    Log.LogInfo("ProperKorean :: Fixed font");
                }
            };
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
    }
}