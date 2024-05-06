using Unity.Netcode;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSuitPreview : MonoBehaviour
    {
        public Camera modelViewCamera;
        public GameObject previewModel;
        public FemmployeeSettings settings;

        public void SetPreviewRegion(int dropdownID, int selectionIndex)
        {
            if(dropdownID == 0)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.headRegionParts[selectionIndex].mesh;
            }
            if(dropdownID == 1)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.chestRegionParts[selectionIndex].mesh;
            }
            if (dropdownID == 2)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.armsRegionParts[selectionIndex].mesh;
            }
            if(dropdownID == 3)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.waistRegionParts[selectionIndex].mesh;
            }
            if(dropdownID == 4)
            {
                settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = settings.legsRegionParts[selectionIndex].mesh;
            }
            FemmployeeSuitSync.instance.SyncPreviewBodyMeshes(dropdownID, selectionIndex, NetworkManager.Singleton.LocalClientId);
        }

    }
}
