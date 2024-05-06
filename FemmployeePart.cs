using UnityEngine;

[CreateAssetMenu(fileName = "FemmployeePart", menuName = "Custom/FemmployeePart", order = 1)]
public class FemmployeePart : ScriptableObject
{
    public Mesh mesh;
    public Material[] materials;
}
