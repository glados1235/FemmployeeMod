using FemmployeeMod;
using LethalLib;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static IngamePlayerSettings;


namespace ModelReplacement
{
    public class Femmployee : BodyReplacementBase 
    {
        public FemmployeeSettings settings;
        public SkinnedMeshRenderer meshRenderer;
        public FemmployeeConfigUI localModdedUI;
        protected override GameObject LoadAssetsAndReturnModel()
        { 
            string model_name = "Femmployee";
            return Assets.MainAssetBundle.LoadAsset<GameObject>(model_name);
        }

        protected override void Awake()
        {
            base.Awake();
            if (StartOfRound.Instance.IsServer)
            {
                GameObject settingsObject = Instantiate(FemmployeeModBase.settingsPrefab);
                settings = settingsObject.GetComponent<FemmployeeSettings>();
                settingsObject.GetComponent<NetworkObject>().Spawn();
            }
        }

        protected override void Start()
        {

            settings.controller = controller;

            if (settings.controller != GameNetworkManager.Instance.localPlayerController) { return; }

            settings.transform.SetParent(transform, false);

            settings.replacementModel = replacementModel;

            settings.suitName = suitName;

            meshRenderer = settings.replacementModel.GetComponentInChildren<SkinnedMeshRenderer>();
            
            settings.BodyMeshRenderer = meshRenderer;

            settings.cosmeticParts = replacementModel.GetComponentsInChildren<CosmeticPart>();

            localModdedUI = FindObjectOfType<FemmployeeConfigUI>();

            localModdedUI.localSettings = settings;

            FemmployeeModBase.mls.LogWarning($"The suit {settings.suitName} has been put on by player {settings.controller.actualClientId} and the suit model is {settings.replacementModel} and the localModdedUI is {localModdedUI}");



        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(settings.gameObject);

        }

    }
}