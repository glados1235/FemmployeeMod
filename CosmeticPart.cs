using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FemmployeeMod
{


    public class CosmeticPart : MonoBehaviour
    {
        public enum Region
        {
            head,
            chest,
            arms,
            waist,
            legs
        }
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
        }
    }
}
