using GameNetcodeStuff;
using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static IngamePlayerSettings;

namespace FemmployeeMod
{
    public class FemmployeeSettings : NetworkBehaviour
    {
        public PlayerControllerB controller;

        public GameObject replacementModel;

        public SkinnedMeshRenderer BodyMeshRenderer;

        public CosmeticPart[] cosmeticParts;

        public string suitName { get; set; } = "";

        public float bulgeSize;

        public float breastSize;



        public void ApplySettings()
        {
            if (IsServer) { ApplySettingsClientRpc(); }
            else { ApplySettingsServerRpc(); }
        }


        [ServerRpc(RequireOwnership = false)]
        public void ApplySettingsServerRpc()
        {
            ApplySettingsClientRpc();
        }

        [ClientRpc]
        public void ApplySettingsClientRpc()
        {
            BodyMeshRenderer.SetBlendShapeWeight(0, breastSize);
            BodyMeshRenderer.SetBlendShapeWeight(1, bulgeSize);
            Tools.LogAll(this);
        }
    }
}
