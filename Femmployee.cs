using ModelReplacement;
using System.Runtime.Serialization;
using UnityEngine;
using FemmployeeMod;
using System.Collections.Generic;
using Unity.Netcode;
using static IngamePlayerSettings;
using System.Collections;

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
                settings.networkedSettings.SelfDestructServerRpc();
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


                networkedSettings.SetNetworkedSettingsClientRpc(controller.actualClientId);
                networkedSettings.SetObjectNameClientRpc(controller.actualClientId);
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
        }

    }
}