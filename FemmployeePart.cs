using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FemmployeePart", menuName = "FemmployeeMod/FemmployeePart", order = 1)]
public class FemmployeePart : ScriptableObject
{
    public Mesh mesh;
    public Material[] materials;
}
