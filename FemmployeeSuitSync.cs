using ModelReplacement;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static IngamePlayerSettings;

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


        public void SyncPreviewBodyMeshes(int dropdownID, int selectionIndex, ulong id)
        {
            if (IsServer) { SyncSelectionClientRpc(dropdownID, selectionIndex, id); }
            else { SyncSelectionsServerRpc(dropdownID, selectionIndex, id); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncSelectionsServerRpc(int dropdownID, int selectionIndex, ulong id)
        {
            SyncSelectionClientRpc(dropdownID, selectionIndex, id);
        }

        [ClientRpc]
        public void SyncSelectionClientRpc(int dropdownID, int selectionIndex, ulong id)
        {
            foreach (var f in femmployees)
            {
                if (f.controller.actualClientId == id)
                {
                    if (dropdownID == 0)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.headRegionParts[selectionIndex].mesh;
                    }
                    if (dropdownID == 1)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.chestRegionParts[selectionIndex].mesh;
                    }
                    if (dropdownID == 2)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.armsRegionParts[selectionIndex].mesh;
                    }
                    if (dropdownID == 3)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.waistRegionParts[selectionIndex].mesh;
                    }
                    if (dropdownID == 4)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.legsRegionParts[selectionIndex].mesh;
                    }
                }
            }
        }


        public void ApplySettings(ulong id)
        {
            if (IsServer) { ApplySettingsClientRpc(id); }
            else { ApplySettingsServerRpc(id); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ApplySettingsServerRpc(ulong id) 
        {
            ApplySettingsClientRpc(id);
        }

        [ClientRpc]
        public void ApplySettingsClientRpc(ulong id)
        {
            NetworkLog.LogError(id + " :is the ID of the player pre loop");
            foreach(var f in femmployees)
            {
                if(f.controller.actualClientId == id)
                {
                    NetworkLog.LogError((int)f.controller.actualClientId + " :is the ID of the list player it found");
                    f.ApplySwapRegions();
                }
            }
        }

    }
}
