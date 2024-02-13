using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;
using System;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;
using BepInEx.Logging;
using Unity.Netcode;
using FemmployeeMod;

namespace ModelReplacement
{
    [BepInPlugin("com.TiltedHat.FemmployeeMod", "Femmployee Mod", "0.1.0")]
    [BepInDependency("meow.ModelReplacementAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("x753.More_Suits", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class FemmployeeModBase : BaseUnityPlugin
    {
        internal static InputClass InputActionsInstance = new InputClass();

        public static ManualLogSource mls;

        public static ConfigFile config;

        public static GameObject settingsPrefab;

        // Example Config for single model mod
        public static ConfigEntry<bool> enableModelForAllSuits { get; private set; }
        public static ConfigEntry<bool> enableModelAsDefault { get; private set; }
        public static ConfigEntry<string> suitNamesToEnableModel { get; private set; }

        private static void InitConfig()
        {
            enableModelForAllSuits = config.Bind<bool>("Suits to Replace Settings", "Enable Model for all Suits", false, "Enable to model replace every suit. Set to false to specify suits");
            enableModelAsDefault = config.Bind<bool>("Suits to Replace Settings", "Enable Model as default", false, "Enable to model replace every suit that hasn't been otherwise registered.");
            suitNamesToEnableModel = config.Bind<string>("Suits to Replace Settings", "Suits to enable Model for", "Default,Orange suit", "Enter a comma separated list of suit names.(Additionally, [Green suit,Pajama suit,Hazard suit])");

        }
        private void Awake()
        {
            config = base.Config;
            InitConfig();

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            settingsPrefab = LethalLib.Modules.NetworkPrefabs.CreateNetworkPrefab("FemmployeeSettings");
            settingsPrefab.AddComponent<FemmployeeSettings>();

            Assets.PopulateAssets();
            ModelReplacementAPI.RegisterSuitModelReplacement("Femmployee", typeof(Femmployee));
            Harmony harmony = new Harmony("com.TiltedHat.FemmployeeMod");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {"com.TiltedHat.FemmployeeMod"} is loaded!");
            mls = Logger;
        }
    }
    public static class Assets
    {
        // Replace mbundle with the Asset Bundle Name from your unity project 
        public static string mainAssetBundleName = "femmployeemodbundle";
        public static AssetBundle MainAssetBundle = null;
        private static string GetAssemblyName() => Assembly.GetExecutingAssembly().GetName().Name.Replace(" ","_");
        public static void PopulateAssets()
        {
            if (MainAssetBundle == null)
            {
                Console.WriteLine(GetAssemblyName() + "." + mainAssetBundleName);
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetAssemblyName() + "." + mainAssetBundleName))
                {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }

            }
        }
    }

    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void UICreationPatch()
        {
            var bundle = Assets.MainAssetBundle;
            string prefabName = "Assets/ModdedUI/ModdedUI.prefab";
            if (bundle != null && bundle.Contains(prefabName))
            {
                GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
                if (prefab != null)
                {
                    GameObject.Instantiate(prefab); 
                }
                else
                {
                    Debug.LogError($"Could not find prefab '{prefabName}' in the AssetBundle.");
                }
            }
            else
            {
                Debug.LogError("AssetBundle is null or does not contain the specified prefab.");
            }
        }
    }


    public class InputClass : LcInputActions
    {
        [InputAction("<Keyboard>/g", Name = "Femmployee UI")]
        public InputAction FemmployeeUIToggle { get; set; }
    }
 
}