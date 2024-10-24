using ModelReplacement;
using System.Runtime.Serialization;
using UnityEngine;
using FemmployeeMod;
using System.Collections.Generic;
using Unity.Netcode;
using static IngamePlayerSettings;
using System.Collections;
using BepInEx;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using static UnityEngine.ParticleSystem.PlaybackState;
using FemmployeeMod.UIScripts;

namespace ModelReplacement
{
    public class Femmployee : BodyReplacementBase
    {
        public FemmployeeSettings settings;
        public FemmployeeConfigUI localModdedUI;
        public FemmployeeViewmodel localViewModel;
        
        public override GameObject LoadAssetsAndReturnModel()
        {
            string model_name = "Femmployee";
            return Assets.MainAssetBundle.LoadAsset<GameObject>(model_name);
        }

        public override void Awake()
        {
            base.Awake();

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Tools.CheckIsServer())
            {
                settings.networkedSettings.SelfDestruct();
            }

        }

        public override void Start()
        {
            settings = replacementModel.GetComponent<FemmployeeSettings>();
            settings.controller = controller;
            settings.replacementModel = replacementModel;
            settings.suitName = suitName;

            if (Tools.CheckIsServer())
            {
                GameObject networkedSettingsGo = Instantiate(FemmployeeModBase.networkedSettingsGo);
                var networkObject = networkedSettingsGo.GetComponent<NetworkObject>();
                var networkedSettings = networkedSettingsGo.GetComponent<NetworkedSettings>();
                networkObject.Spawn();
                networkObject.TrySetParent(replacementModel.gameObject, false);
                networkedSettings.playerID.Value = (int)controller.playerClientId;
            }
            localModdedUI = FindObjectOfType<FemmployeeConfigUI>();
            InitializeParts();
            if (controller != GameNetworkManager.Instance.localPlayerController) { return; }
            localModdedUI.femmployeeSuitPreview.settings.partsList = settings.partsList;
            localModdedUI.localFemmployeeGo = gameObject;
            localModdedUI.PopulateDropdowns();
            StartCoroutine(WaitForNetworkSettingsInitialization());
        }

        private IEnumerator WaitForNetworkSettingsInitialization()
        {
            yield return new WaitUntil(() => settings.networkedSettings != null);
            yield return new WaitUntil(() => settings.networkedSettings.hasInitialized.Value == true); 
            yield return new WaitForSeconds(0.2f);
            localModdedUI.femmployeeSuitPreview.LoadSaveData(this);
        }

        private void InitializeParts()
        {
            ScriptableObject[] allScriptables = Assets.MainAssetBundle.LoadAllAssets<ScriptableObject>();

            foreach (ScriptableObject scriptableObject in allScriptables)
            {
                if (scriptableObject is FemmployeePartsInitializationList femmployeePartsInitializationList)
                {
                    for (int i = 0; i < femmployeePartsInitializationList.fullPartsList.Count; i++)
                    {
                        Dictionary<string, FemmployeePart> partsDictionary = new Dictionary<string, FemmployeePart>();

                        foreach (FemmployeePart part in femmployeePartsInitializationList.fullPartsList[i])
                        {
                            partsDictionary.Add(part.name, part);
                        }

                        settings.partsList.Add(partsDictionary);
                    }
                }
            }

        }

        public void ApplySwapRegions()
        {
            int maxValue = localModdedUI.isMultiplierEnabled ? localModdedUI.sliderMultiplier * 100 : 100;

            // Apply meshes and materials.
            ApplyMeshesAndMaterials();

            // Initialize the list of blendshape arrays
            List<NetworkList<BlendshapeValuePair>> blendshapeLists = new List<NetworkList<BlendshapeValuePair>>
            {
                settings.networkedSettings.headBlendshapeValues,
                settings.networkedSettings.chestBlendshapeValues,
                settings.networkedSettings.armsBlendshapeValues,
                settings.networkedSettings.waistBlendshapeValues,
                settings.networkedSettings.legsBlendshapeValues
            };

            // Apply clamped blendshape values
            for (int i = 0; i < blendshapeLists.Count; i++)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = settings.bodyRegionMeshRenderers[i];
                // Iterate over blendshapes, clamp values and apply them
                foreach (var blendshapeValuePair in blendshapeLists[i])
                {
                    float clampedValue = Mathf.Clamp(blendshapeValuePair.ShapeValue, 0, maxValue);
                    skinnedMeshRenderer.SetBlendShapeWeight(blendshapeValuePair.ShapeID, clampedValue);
                }
            }

            // Apply material properties
            ApplyMaterialProperties();
        }

        private void ApplyMeshesAndMaterials()
        {
            // Apply meshes and materials for each body region
            for (int i = 0; i < settings.bodyRegionMeshRenderers.Length; i++)
            {
                settings.bodyRegionMeshRenderers[i].sharedMesh = settings.partsList[i][GetSyncValue(i)].mesh;

                settings.bodyRegionMeshRenderers[i].materials = settings.partsList[i][GetSyncValue(i)].materials;
            }
            if (!ModelReplacementAPI.EnforceViewModelGeneration.Value) { FemmployeeModBase.mls.LogWarning("'Generate Viewmodels by default' is disabled! Skipping viewmodel generation"); return; }
            if (controller != GameNetworkManager.Instance.localPlayerController) { return; }
            ApplyLocalViewmodel();
        }

        private void ApplyLocalViewmodel()
        {
            FemmployeeSettings viewmodelSettings = replacementViewModel.GetComponent<FemmployeeSettings>();
            viewmodelSettings.bodyRegionMeshRenderers[2].sharedMesh = settings.partsList[2][GetSyncValue(2)].mesh;
            viewmodelSettings.bodyRegionMeshRenderers[2].materials = settings.partsList[2][GetSyncValue(2)].materials;

            Destroy(replacementViewModel);
            replacementViewModel = LoadViewModelreplacement();
            viewModelAvatar.AssignViewModelReplacement(controller.gameObject, replacementViewModel);
            SetAvatarRenderers(true);
            viewState.ReportBodyReplacementAddition(this);
            cosmeticManager.ReportBodyReplacementAddition(this);
        }

        private string GetSyncValue(int index)
        {
            switch (index)
            {
                case 0: return settings.networkedSettings.headSync;
                case 1: return settings.networkedSettings.chestSync;
                case 2: return settings.networkedSettings.armsSync;
                case 3: return settings.networkedSettings.waistSync;
                case 4: return settings.networkedSettings.legSync;
                default: return string.Empty;
            }
        }

        private void ApplyMaterialProperties()
        {
            foreach (var SMR in settings.bodyRegionMeshRenderers)
            {
                foreach (var material in SMR.materials)
                {
                    if (material.name == "Suit (Instance)")
                    {
                        material.color = settings.networkedSettings.suitMaterialValues.Value.colorValue;
                        material.SetFloat("_Metallic", settings.networkedSettings.suitMaterialValues.Value.metallicValue);
                        material.SetFloat("_Smoothness", settings.networkedSettings.suitMaterialValues.Value.smoothnessValue);
                    }

                    if (material.name == "Skin (Instance)")
                    {
                        material.color = settings.networkedSettings.skinMaterialValues.Value.colorValue;
                        material.SetFloat("_Metallic", settings.networkedSettings.skinMaterialValues.Value.metallicValue);
                        material.SetFloat("_Smoothness", settings.networkedSettings.skinMaterialValues.Value.smoothnessValue);
                    }
                }
            }
            if (!ModelReplacementAPI.EnforceViewModelGeneration.Value) { return; }
            if (controller != GameNetworkManager.Instance.localPlayerController) { return; }
            ApplyViewmodelMaterialProperties();
        }

        private void ApplyViewmodelMaterialProperties()
        {
            FemmployeeSettings viewmodelSettings = replacementViewModel.GetComponent<FemmployeeSettings>();
            foreach (var material in viewmodelSettings.bodyRegionMeshRenderers[2].materials)
            {
                if (material.name == "Suit (Instance)")
                {
                    material.color = settings.networkedSettings.suitMaterialValues.Value.colorValue;
                    material.SetFloat("_Metallic", settings.networkedSettings.suitMaterialValues.Value.metallicValue);
                    material.SetFloat("_Smoothness", settings.networkedSettings.suitMaterialValues.Value.smoothnessValue);
                }

                if (material.name == "Skin (Instance)")
                {
                    material.color = settings.networkedSettings.skinMaterialValues.Value.colorValue;
                    material.SetFloat("_Metallic", settings.networkedSettings.skinMaterialValues.Value.metallicValue);
                    material.SetFloat("_Smoothness", settings.networkedSettings.skinMaterialValues.Value.smoothnessValue);
                }
            }
        }

        public class FemmployeeSaveData
        {
            public PartsList PartsList { get; set; }
            public Dictionary<string, Dictionary<string, float>> SliderValues { get; set; } = new Dictionary<string, Dictionary<string, float>>();
            public int MultiplierValue { get; set; }
            public bool Multiplier { get; set; }
            public MaterialData suitMaterialData { get; set; }
            public MaterialData skinMaterialData { get; set; }
        }
         
        public class PartsList
        {
            public string HeadSync { get; set; }
            public string ChestSync { get; set; }
            public string ArmsSync { get; set; }
            public string WaistSync { get; set; }
            public string LegSync { get; set; }
        }

        public class MaterialData
        {
            public float colorValueR { get; set; }
            public float colorValueG { get; set; }
            public float colorValueB { get; set; }
            public float metallicValue { get; set; }
            public float smoothnessValue { get; set; }
        }    
    }
}