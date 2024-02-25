using Unity.Netcode;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSuitPreview : MonoBehaviour
    {
        public Camera modelViewCamera;
        public GameObject previewModel;
        public FemmployeeSettings settings;

        public bool isBeingEdited;


        public void SetPreviewRegion(int dropdownID, int selectionIndex)
        {
            if(dropdownID == 0)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.headRegionParts[selectionIndex];
            }
            if(dropdownID == 1)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.chestRegionParts[selectionIndex];
            }
            if (dropdownID == 2)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.armsRegionParts[selectionIndex];
            }
            if(dropdownID == 3)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.waistRegionParts[selectionIndex];
            }
            if(dropdownID == 4)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.legsRegionParts[selectionIndex];
            }
        }

    }
}
