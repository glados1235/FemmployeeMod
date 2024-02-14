using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FemmployeeMod
{
    public enum Region
    {
        head,
        chest,
        arms,
        waist,
        legs
    }

    public class CosmeticPart : MonoBehaviour
    {
        public string CosmeticName { get; set; } = "";
        public int cosmeticID;
        public Region region;
        public bool isActive;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public List<BlendshapeTarget> BlendshapeTargets;

        public void Start()
        {
            name = gameObject.name;
            skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
            foreach (var b in BlendshapeTargets) { b.region = region; }
        }
    }
}
