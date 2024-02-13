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

        protected override void Start()
        {

            if (StartOfRound.Instance.IsServer)
            {
                var settingsObject = Instantiate(FemmployeeModBase.settingsPrefab);
                settingsObject.GetComponent<NetworkObject>().Spawn();
                settingsObject.GetComponent<FemmployeeSettings>().localSuit.Value = this;
            }

            settings = FindObjectsOfType<FemmployeeSettings>().First(setting => setting.localSuit.Value == this);
            settings.transform.SetParent(transform, false);

            settings = replacementModel.GetComponent<FemmployeeSettings>();
            settings.controller = controller;
            if (settings.controller != GameNetworkManager.Instance.localPlayerController) { return; }
            settings.replacementModel = replacementModel;
            settings.suitName = suitName;
            meshRenderer = settings.replacementModel.GetComponentInChildren<SkinnedMeshRenderer>();
            localModdedUI = FindObjectOfType<FemmployeeConfigUI>();
            localModdedUI.localSettings = settings;
            localModdedUI.localSettings.localSuit.Value = this;
            


            FemmployeeModBase.mls.LogError($"The suit {settings.suitName} has been put on by player {settings.controller.actualClientId} and the suit model is {settings.replacementModel} and the localModdedUI is {localModdedUI}");
        }


    }
}