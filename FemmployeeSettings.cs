using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeSettings : MonoBehaviour
    {
        public PlayerControllerB controller;
        public GameObject replacementModel;
        public FemmployeePart[] previewBodyParts;
        public SkinnedMeshRenderer[] bodyRegionMeshRenderers;
        public NetworkedSettings networkedSettings;


        public List<Dictionary<string, FemmployeePart>> partsList = new List<Dictionary<string, FemmployeePart>>();

        public string suitName { get; set; } = "";

        public Dictionary<string, FemmployeePart> headPartsCollection = new Dictionary<string, FemmployeePart>();
        public Dictionary<string, FemmployeePart> chestPartsCollection = new Dictionary<string, FemmployeePart>();
        public Dictionary<string, FemmployeePart> armsPartsCollection = new Dictionary<string, FemmployeePart>();
        public Dictionary<string, FemmployeePart> waistPartsCollection = new Dictionary<string, FemmployeePart>();
        public Dictionary<string, FemmployeePart> legsPartsCollection = new Dictionary<string, FemmployeePart>();

    }

}
