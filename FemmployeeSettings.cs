using GameNetcodeStuff;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSettings : MonoBehaviour
    {
        public PlayerControllerB controller;

        public GameObject replacementModel;
         
        public SkinnedMeshRenderer[] bodyRegionMeshRenderers;
        public Mesh[] previewBodyMeshes;

        public FemmployeePart[] headRegionParts;
        public FemmployeePart[] chestRegionParts;
        public FemmployeePart[] armsRegionParts;
        public FemmployeePart[] waistRegionParts;
        public FemmployeePart[] legsRegionParts;

        public string suitName { get; set; } = "";

    }

}
