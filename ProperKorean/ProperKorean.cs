using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public const string PluginVersion = "1.3.1";

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
            var font = LegacyResourcesAPI.LoadAsync<TMP_FontAsset>("TmpFonts/Bombardier/tmpBombDropshadow").WaitForCompletion();

            // Tilde is placed too high up
            var tilde = font.glyphTable.Find(x => x.index == 95);
            tilde.metrics = tilde.metrics with { horizontalBearingY = 40 };

            properFont.fallbackFontAssets.Add(font);

            if (TMP_Settings.fallbackFontAssets[0].name != "tmpBombDropshadow")
                TMP_Settings.fallbackFontAssets.Insert(0, font);

            HGTextMeshProUGUI.defaultLanguageFont = properFont;

            Log.LogInfo("ProperKorean :: Fixed font");
        }

        private static void FixChat()
        {
            var chatInRun = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ChatBox, In Run.prefab").WaitForCompletion();
            foreach (var lang in chatInRun.GetComponent<ChatBox>().languageAffectedbyLineCounts)
                if (lang.languageAffected == "ko")
                    lang.languageAffected = "ko_NULL";

            var chat = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/ChatBox.prefab").WaitForCompletion();
            foreach (var lang in chat.GetComponent<ChatBox>().languageAffectedbyLineCounts)
                if (lang.languageAffected == "ko")
                    lang.languageAffected = "ko_NULL";

            Log.LogInfo("ProperKorean :: Fixed chatbox");
        }

        private static void HGTextMeshProUGUI_OnCurrentLanguageChanged(On.RoR2.UI.HGTextMeshProUGUI.orig_OnCurrentLanguageChanged orig)
        {
            orig();

            if (Language.currentLanguageName == "ko")
            {
                FixFont();
                FixLocalization();
                FixChat();
            }
        }
    }
}