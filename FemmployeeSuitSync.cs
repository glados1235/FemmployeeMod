using ModelReplacement;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FemmployeeMod 
{
    public class FemmployeeSuitSync : NetworkBehaviour
    {

        public static FemmployeeSuitSync instance;
        public static List<Femmployee> femmployees = new List<Femmployee>();


        public void Awake()
        {
            instance = this;
        }


        public void ApplySettings(Femmployee femmployee)
        {
            if (IsServer) { ApplySettingsClientRpc(femmployee.GetComponent<NetworkObject>()); }
            else { ApplySettingsServerRpc((int)femmployee.controller.actualClientId); }


        }

        [ServerRpc(RequireOwnership = false)]
        public void ApplySettingsServerRpc(int id) 
        {
            
            foreach (var f in femmployees)
            {
                if (id == (int)f.controller.actualClientId)
                {
                    ApplySettingsClientRpc(femmployees[id].GetComponent<NetworkObject>());

                }
            }
        }

        [ClientRpc]
        public void ApplySettingsClientRpc(NetworkObjectReference networkObject)
        {

            
            ((GameObject)networkObject).GetComponent<Femmployee>().ApplySwapRegions(((GameObject)networkObject).GetComponent<Femmployee>().localModdedUI.femmployeeSuitPreview.settings);
        }

    }
}
