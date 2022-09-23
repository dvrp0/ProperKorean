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
        public const string PluginVersion = "1.1.0";

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

            Log.LogInfo("ProperKorean :: Proper JSON Loaded");

            foreach (var token in proper)
            {
                if (Language.currentLanguage.GetLocalizedStringByToken(token.Key) != token.Value)
                    Language.currentLanguage.SetStringByToken(token.Key, token.Value);
            }

            Log.LogInfo("ProperKorean :: Proper JSON Applied");
        }

        static void HGTextMeshProUGUI_OnCurrentLanguageChanged(On.RoR2.UI.HGTextMeshProUGUI.orig_OnCurrentLanguageChanged orig)
        {
            if (Language.currentLanguageName == "ko")
            {
                Language.currentLanguage.SetStringByToken("DEFAULT_FONT", "TmpFonts/Bombardier/tmpBombDropShadow");
                TMP_Settings.fallbackFontAssets.Add(properFont);
                FixLocalization();

                Log.LogInfo("ProperKorean :: FallbackFont Added");
            }
            else if (TMP_Settings.fallbackFontAssets.Count == 2)
            {
                TMP_Settings.fallbackFontAssets.RemoveAt(1);

                Log.LogInfo("ProperKorean :: FallbackFont Removed");
            }

            orig();
        }
    }
}