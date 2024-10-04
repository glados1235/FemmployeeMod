using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;
using System;
using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;
using FemmployeeMod;
using System.IO;

namespace ModelReplacement 
{
    [BepInPlugin("com.TiltedHat.FemmployeeMod", "Femmployee Mod", "1.0.2")]
    [BepInDependency("meow.ModelReplacementAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("x753.More_Suits", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class FemmployeeModBase : BaseUnityPlugin
    {
        public static FemmployeeModBase instance;
        internal static InputClass InputActionsInstance = new InputClass();
        public static ManualLogSource mls;
        public static ConfigFile config;
        public static GameObject networkedSettingsGo;
        public static string saveFilePath = Path.Combine(Paths.BepInExRootPath, "plugins", "TiltedTomb-The_Femmployee_Mod", "FemmployeeSaveData.json");

        public static ConfigEntry<bool> useSaveFileFormatting;



        private static void InitConfig()
        {
            useSaveFileFormatting = config.Bind<bool>("Femmployee Mod Settings", "Use Save File formatting", false, "Enable for save file debugging. makes the mod generate the JSON with Formatting.Indented true.");
        }
        private void Awake()
        {

            config = base.Config;
            InitConfig();

            if (instance == null)
            {
                instance = this;
            }

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

            Assets.PopulateAssets();
            ModelReplacementAPI.RegisterSuitModelReplacement("Femmployee", typeof(Femmployee));
            Harmony harmony = new Harmony("com.TiltedHat.FemmployeeMod");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {"com.TiltedHat.FemmployeeMod"} is loaded!");
            mls = Logger;

            networkedSettingsGo = (GameObject)Assets.MainAssetBundle.LoadAsset("NetworkedSettings.prefab");
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(networkedSettingsGo);
        }

    }

    public static class Assets
    {
        // Replace mbundle with the Asset Bundle Name from your unity project 
        public static string mainAssetBundleName = "femmployeemodbundle";
        public static AssetBundle MainAssetBundle = null;
        private static string GetAssemblyName() => Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_");
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
            GameObject prefab = bundle.LoadAsset<GameObject>("ModdedUI.prefab");
            GameObject.Instantiate(prefab);
        }
    }


    public class InputClass : LcInputActions
    {
        [InputAction("<Keyboard>/backslash", Name = "Femmployee UI")]
        public InputAction FemmployeeUIToggle { get; set; }
    }

    public class NetworkMaterialProperties : INetworkSerializable
    {
        public Color colorValue;
        public float metallicValue;
        public float smoothnessValue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref colorValue);
            serializer.SerializeValue(ref metallicValue);
            serializer.SerializeValue(ref smoothnessValue);
        }
    }

}