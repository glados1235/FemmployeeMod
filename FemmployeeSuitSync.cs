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


        public void SyncPreviewBodyMeshes(int dropdownID, int selectionIndex, int id)
        {
            if (IsServer) { SyncSelectionClientRpc(dropdownID, selectionIndex, id); }
            else { SyncSelectionsServerRpc(dropdownID, selectionIndex, id); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncSelectionsServerRpc(int dropdownID, int selectionIndex, int id)
        {
            SyncSelectionClientRpc(dropdownID, selectionIndex, id);
        }

        [ClientRpc]
        public void SyncSelectionClientRpc(int dropdownID, int selectionIndex, int id)
        {
            foreach (var f in femmployees)
            {
                if ((int)f.controller.actualClientId == id)
                {
                    if (dropdownID == 0)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.headRegionParts[selectionIndex];
                    }
                    if (dropdownID == 1)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.chestRegionParts[selectionIndex];
                    }
                    if (dropdownID == 2)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.armsRegionParts[selectionIndex];
                    }
                    if (dropdownID == 3)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.waistRegionParts[selectionIndex];
                    }
                    if (dropdownID == 4)
                    {
                        f.settings.previewBodyMeshes[dropdownID] = f.settings.legsRegionParts[selectionIndex];
                    }
                }
            }
        }


        public void ApplySettings(int id)
        {
            if (IsServer) { ApplySettingsClientRpc(id); }
            else { ApplySettingsServerRpc(id); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ApplySettingsServerRpc(int id) 
        {
            ApplySettingsClientRpc(id);
        }

        [ClientRpc]
        public void ApplySettingsClientRpc(int id)
        {
            foreach(var f in femmployees)
            {
                if((int)f.controller.actualClientId == id)
                {
                    f.ApplySwapRegions();
                }
            }
        }

    }
}
