using FemmployeeMod;
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

        protected override void Start()
        {
            if (controller != GameNetworkManager.Instance.localPlayerController) { return; }

            settings = replacementModel.GetComponent<FemmployeeSettings>();

            settings.controller = controller;

            settings.replacementModel = replacementModel;

            settings.suitName = suitName;

            localModdedUI = FindObjectOfType<FemmployeeConfigUI>();

            localModdedUI.PopulateDropdowns();

            localModdedUI.localFemmployeeGo = gameObject;

            FemmployeeModBase.mls.LogWarning($"The suit {settings.suitName} has been put on by player {settings.controller.actualClientId}");
        }

        public void ApplySwapRegions()
        {
            for (int smr = 0; smr < 5; smr++) 
            {
                settings.bodyRegionMeshRenderers[smr].sharedMesh = localModdedUI.femmployeeSuitPreview.settings.bodyRegionMeshRenderers[smr].sharedMesh;
            }
        }

    }
}