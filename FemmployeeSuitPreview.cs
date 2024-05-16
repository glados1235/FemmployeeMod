using GameNetcodeStuff;
using ModelReplacement;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSuitPreview : MonoBehaviour
    {
        public Camera modelViewCamera;
        public GameObject previewModel;
        public FemmployeeSettings settings;
        public void SetPreviewRegion(int dropdownID, string selectionKeyName, Femmployee playerFemmployee)
        {
            playerFemmployee.settings.previewBodyParts[dropdownID] = settings.partsList[dropdownID][selectionKeyName];
            settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = playerFemmployee.settings.previewBodyParts[dropdownID].mesh;

            settings.bodyRegionMeshRenderers[dropdownID].materials = playerFemmployee.settings.previewBodyParts[dropdownID].materials;

            switch (dropdownID)
            {
                case 0:
                    playerFemmployee.settings.networkedSettings.headSync = selectionKeyName; 
                    break;
                case 1:
                    playerFemmployee.settings.networkedSettings.chestSync = selectionKeyName; 
                    break;
                case 2:
                    playerFemmployee.settings.networkedSettings.armsSync = selectionKeyName; 
                    break;
                case 3:
                    playerFemmployee.settings.networkedSettings.waistSync = selectionKeyName; 
                    break;
                case 4: 
                    playerFemmployee.settings.networkedSettings.legSync = selectionKeyName; 
                    break;
                default:
                    FemmployeeModBase.mls.LogWarning("Invalid dropdown ID");
                    return;
            }
        }

        public void SetBlendshape(int id, float value, string[] blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                int shapeID = settings.bodyRegionMeshRenderers[id].sharedMesh.GetBlendShapeIndex(blendshape);
                settings.bodyRegionMeshRenderers[id].SetBlendShapeWeight(shapeID, value);
            }
        }

    }
}
