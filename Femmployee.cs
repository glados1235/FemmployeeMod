using ModelReplacement;
using System.Runtime.Serialization;
using UnityEngine;
using FemmployeeMod;
using System.Collections.Generic;
using Unity.Netcode;
using static IngamePlayerSettings;

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
                settings.networkedSettings = networkedSettings;
                networkedSettings.SetNetworkedSettingsClientRpc(controller.actualClientId);
            }



            localModdedUI = FindObjectOfType<FemmployeeConfigUI>();

            InitializeParts();

            if (controller != GameNetworkManager.Instance.localPlayerController) { return; }
            localModdedUI.femmployeeSuitPreview.settings.partsList = settings.partsList;
            localModdedUI.localFemmployeeGo = gameObject;
            localModdedUI.PopulateDropdowns();

            FemmployeeModBase.mls.LogWarning($"The suit {settings.suitName} has been put on by player {settings.controller.actualClientId}");
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
            //head Apply
            settings.bodyRegionMeshRenderers[0].sharedMesh = settings.partsList[0][settings.networkedSettings.headSync].mesh;

            //Chest Apply
            settings.bodyRegionMeshRenderers[1].sharedMesh = settings.partsList[1][settings.networkedSettings.chestSync].mesh;

            //Arms Apply
            settings.bodyRegionMeshRenderers[2].sharedMesh = settings.partsList[2][settings.networkedSettings.armsSync].mesh;

            //Waist Apply
            settings.bodyRegionMeshRenderers[3].sharedMesh = settings.partsList[3][settings.networkedSettings.waistSync].mesh;

            //Legs Apply
            settings.bodyRegionMeshRenderers[4].sharedMesh = settings.partsList[4][settings.networkedSettings.legSync].mesh;


            for (int smr = 0; smr < 5; smr++) 
            {
                settings.bodyRegionMeshRenderers[smr].materials = settings.previewBodyParts[smr].materials;
            }
        }

    }
}