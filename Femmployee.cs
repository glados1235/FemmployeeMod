using FemmployeeMod;
using System.Runtime.Serialization;
using UnityEngine;

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
            FemmployeeSuitSync.femmployees.Add(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            FemmployeeSuitSync.femmployees.Remove(this);
        }

        protected override void Start()
        {
            settings = replacementModel.GetComponent<FemmployeeSettings>();

            localModdedUI = FindObjectOfType<FemmployeeConfigUI>();

            if (controller != GameNetworkManager.Instance.localPlayerController) { return; }

            settings.controller = controller;

            settings.replacementModel = replacementModel;

            settings.suitName = suitName;

            localModdedUI.PopulateDropdowns();

            localModdedUI.localFemmployeeGo = gameObject;

            FemmployeeModBase.mls.LogWarning($"The suit {settings.suitName} has been put on by player {settings.controller.actualClientId}");
        }

        public void ApplySwapRegions(FemmployeeSettings settings)
        {
            for (int smr = 0; smr < 5; smr++) 
            {
                settings.bodyRegionMeshRenderers[smr].sharedMesh = settings.bodyRegionMeshRenderers[smr].sharedMesh;
            }
        }

    }
}