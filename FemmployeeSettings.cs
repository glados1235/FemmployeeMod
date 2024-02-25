using GameNetcodeStuff;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSettings : MonoBehaviour
    {
        public PlayerControllerB controller;

        public GameObject replacementModel;
         
        public SkinnedMeshRenderer[] bodyRegionMeshRenderers;

        public Mesh[] headRegionParts;
        public Mesh[] chestRegionParts;
        public Mesh[] armsRegionParts;
        public Mesh[] waistRegionParts;
        public Mesh[] legsRegionParts;

        public string suitName { get; set; } = "";

    }

}
