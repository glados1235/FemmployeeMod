using ModelReplacement;
using Unity.Netcode;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSuitSync : NetworkBehaviour
    {

        public static FemmployeeSuitSync instance;
        public GameObject femmployeeGo;
        
        public void Awake()
        {
            instance = this;
        }

        public void ApplySettings(Femmployee femmployee)
        {
            femmployeeGo = femmployee.gameObject;
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
            femmployeeGo.GetComponent<Femmployee>().ApplySwapRegions();
            NetworkLog.LogWarning($"{femmployeeGo}");
        }

    }
}
