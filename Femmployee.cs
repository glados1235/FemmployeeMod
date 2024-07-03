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

namespace ModelReplacement
{
    public class Femmployee : BodyReplacementBase
    {
        public FemmployeeSettings settings;
        public FemmployeeConfigUI localModdedUI;

        protected override GameObject LoadAssetsAndReturnModel()
        {
            string model_name = "Femmployee";
            return Assets.MainAssetBundle.LoadAsset<GameObject>(model_name);
        }

        protected override void Awake()
        {
            base.Awake();

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Tools.CheckIsServer())
            {
                settings.networkedSettings.SelfDestruct();
            }

        }

        protected override void Start()
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

            //head Apply
            settings.bodyRegionMeshRenderers[0].sharedMesh = settings.partsList[0][settings.networkedSettings.headSync].mesh;
            settings.bodyRegionMeshRenderers[0].materials = settings.partsList[0][settings.networkedSettings.headSync].materials;

            //Chest Apply
            settings.bodyRegionMeshRenderers[1].sharedMesh = settings.partsList[1][settings.networkedSettings.chestSync].mesh;
            settings.bodyRegionMeshRenderers[1].materials = settings.partsList[1][settings.networkedSettings.chestSync].materials;

            //Arms Apply
            settings.bodyRegionMeshRenderers[2].sharedMesh = settings.partsList[2][settings.networkedSettings.armsSync].mesh;
            settings.bodyRegionMeshRenderers[2].materials = settings.partsList[2][settings.networkedSettings.armsSync].materials;

            //Waist Apply
            settings.bodyRegionMeshRenderers[3].sharedMesh = settings.partsList[3][settings.networkedSettings.waistSync].mesh;
            settings.bodyRegionMeshRenderers[3].materials = settings.partsList[3][settings.networkedSettings.waistSync].materials;

            //Legs Apply
            settings.bodyRegionMeshRenderers[4].sharedMesh = settings.partsList[4][settings.networkedSettings.legSync].mesh;
            settings.bodyRegionMeshRenderers[4].materials = settings.partsList[4][settings.networkedSettings.legSync].materials;

            // Initialize arrays with sizes matching the NetworkLists
            float[] headBlendshapes = new float[settings.networkedSettings.headBlendshapeValues.Count];
            float[] chestBlendshapes = new float[settings.networkedSettings.chestBlendshapeValues.Count];
            float[] armsBlendshapes = new float[settings.networkedSettings.armsBlendshapeValues.Count];
            float[] waistBlendshapes = new float[settings.networkedSettings.waistBlendshapeValues.Count];
            float[] legsBlendshapes = new float[settings.networkedSettings.legsBlendshapeValues.Count];

            // Use the arrays in place of the original NetworkLists
            float[][] blendshapeArrays =
            [
                headBlendshapes,
                chestBlendshapes,
                armsBlendshapes,
                waistBlendshapes,
                legsBlendshapes
            ];

            for (int i = 0; i < 5; i++)
            {
                for (int shapeValues = 0; shapeValues < blendshapeArrays[i].Length; shapeValues++)
                {
                    float clampedValue = 0;
                    if (i == 0) clampedValue = Mathf.Clamp(settings.networkedSettings.headBlendshapeValues[shapeValues], 0, maxValue);
                    if (i == 1) clampedValue = Mathf.Clamp(settings.networkedSettings.chestBlendshapeValues[shapeValues], 0, maxValue);
                    if (i == 2) clampedValue = Mathf.Clamp(settings.networkedSettings.armsBlendshapeValues[shapeValues], 0, maxValue);
                    if (i == 3) clampedValue = Mathf.Clamp(settings.networkedSettings.waistBlendshapeValues[shapeValues], 0, maxValue);
                    if (i == 4) clampedValue = Mathf.Clamp(settings.networkedSettings.legsBlendshapeValues[shapeValues], 0, maxValue);
                    FemmployeeModBase.mls.LogWarning(clampedValue);
                    blendshapeArrays[i][shapeValues] = clampedValue;
                }
            }

            // Apply the clamped blendshape values
            for (int i = 0; i < 5; i++)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = settings.bodyRegionMeshRenderers[i];

                for (int shapeID = 0; shapeID < skinnedMeshRenderer.sharedMesh.blendShapeCount; shapeID++)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(shapeID, blendshapeArrays[i][shapeID]);
                }
            }

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
                }
            }

            foreach (var SMR in settings.bodyRegionMeshRenderers)
            {
                foreach (var material in SMR.materials)
                {
                    if (material.name == "Tubing (Instance)")
                    {
                        material.color = settings.networkedSettings.skinMaterialValues.Value.colorValue;
                        material.SetFloat("_Metallic", settings.networkedSettings.skinMaterialValues.Value.metallicValue);
                        material.SetFloat("_Smoothness", settings.networkedSettings.skinMaterialValues.Value.smoothnessValue);
                    }
                }
            }
        }

        public class FemmployeeSaveData
        {
            public PartsList PartsList { get; set; }
            public Dictionary<string, Dictionary<string, float>> SliderValues { get; set; } = new Dictionary<string, Dictionary<string, float>>();
            public int MultiplierValue { get; set; }
            public bool Multiplier { get; set; }
        }
         
        public class PartsList
        {
            public string HeadSync { get; set; }
            public string ChestSync { get; set; }
            public string ArmsSync { get; set; }
            public string WaistSync { get; set; }
            public string LegSync { get; set; }
        }
    }
}