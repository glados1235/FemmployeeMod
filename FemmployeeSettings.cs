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
        public PlayerControllerB controller = null;

        public GameObject replacementModel = null;

        public Femmployee localSuit;

        public string suitName { get; set; } = "";

        public bool isBeingEdited;

        public float bulgeSize;

        public float breastSize;



        public void ApplySettings()
        {
            if (isBeingEdited)
            {
                if (IsServer) { ApplySettingsClientRpc(); FemmployeeModBase.mls.LogWarning("ApplySettings called on the server!"); }
                else { ApplySettingsServerRpc(); FemmployeeModBase.mls.LogWarning("ApplySettings called on the Client!"); }
            }
            
        }


        [ServerRpc(RequireOwnership = false)]
        public void ApplySettingsServerRpc()
        {
            FemmployeeModBase.mls.LogWarning("we made it! ClientRPC!");
            ApplySettingsClientRpc();
        }

        [ClientRpc]
        public void ApplySettingsClientRpc()
        {
            FemmployeeModBase.mls.LogWarning("we made it! ServerRPC!");
            localSuit.meshRenderer.SetBlendShapeWeight(0, breastSize);
            localSuit.meshRenderer.SetBlendShapeWeight(1, bulgeSize);
        }
    }
}
