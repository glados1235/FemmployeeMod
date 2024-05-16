using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FemmployeePart", menuName = "FemmployeeMod/FemmployeePart", order = 1)]
public class FemmployeePart : ScriptableObject
{
    public Mesh mesh;
    public Material[] materials;
}


[CreateAssetMenu(fileName = "FemmployeePartsInitializationList", menuName = "FemmployeeMod/FemmployeePartsInitializationList", order = 1)]
public class FemmployeePartsInitializationList : ScriptableObject
{
    public List<FemmployeePart[]> fullPartsList = new List<FemmployeePart[]>();

    public FemmployeePart[] headParts;
    public FemmployeePart[] chestParts;
    public FemmployeePart[] armsParts;
    public FemmployeePart[] waistParts;
    public FemmployeePart[] legsParts;

    public void Awake()
    {
        fullPartsList.Add(headParts);
        fullPartsList.Add(chestParts);
        fullPartsList.Add(armsParts);
        fullPartsList.Add(waistParts);
        fullPartsList.Add(legsParts);
    }
}
