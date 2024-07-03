using GameNetcodeStuff;
using LethalLib.Modules;
using ModelReplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FemmployeeMod
{
    public class FemmployeeSuitPreview : MonoBehaviour
    {
        public Camera modelViewCamera;
        public GameObject previewModel;
        public FemmployeeSettings settings;
        public Slider previewSpinSlider;


        public void Update()
        {
            float spinValue = previewSpinSlider.value;
            Quaternion targetRotation = Quaternion.Euler(0f, spinValue, 0f);

            previewModel.transform.rotation = targetRotation;
        }

        public void LoadSaveData(Femmployee playerFemmployee)
        {
            settings.bodyRegionMeshRenderers[0].sharedMesh = playerFemmployee.settings.partsList[0][playerFemmployee.settings.networkedSettings.headSync].mesh;
            settings.bodyRegionMeshRenderers[0].materials = playerFemmployee.settings.partsList[0][playerFemmployee.settings.networkedSettings.headSync].materials;

            settings.bodyRegionMeshRenderers[1].sharedMesh = playerFemmployee.settings.partsList[1][playerFemmployee.settings.networkedSettings.chestSync].mesh;
            settings.bodyRegionMeshRenderers[1].materials = playerFemmployee.settings.partsList[1][playerFemmployee.settings.networkedSettings.chestSync].materials;

            settings.bodyRegionMeshRenderers[2].sharedMesh = playerFemmployee.settings.partsList[2][playerFemmployee.settings.networkedSettings.armsSync].mesh;
            settings.bodyRegionMeshRenderers[2].materials = playerFemmployee.settings.partsList[2][playerFemmployee.settings.networkedSettings.armsSync].materials;

            settings.bodyRegionMeshRenderers[3].sharedMesh = playerFemmployee.settings.partsList[3][playerFemmployee.settings.networkedSettings.waistSync].mesh;
            settings.bodyRegionMeshRenderers[3].materials = playerFemmployee.settings.partsList[3][playerFemmployee.settings.networkedSettings.waistSync].materials;

            settings.bodyRegionMeshRenderers[4].sharedMesh = playerFemmployee.settings.partsList[4][playerFemmployee.settings.networkedSettings.legSync].mesh;
            settings.bodyRegionMeshRenderers[4].materials = playerFemmployee.settings.partsList[4][playerFemmployee.settings.networkedSettings.legSync].materials;

            NetworkList<float>[] blendshapeValuesArrays = new NetworkList<float>[]
            {
                playerFemmployee.settings.networkedSettings.headBlendshapeValues,
                playerFemmployee.settings.networkedSettings.chestBlendshapeValues,
                playerFemmployee.settings.networkedSettings.armsBlendshapeValues,
                playerFemmployee.settings.networkedSettings.waistBlendshapeValues,
                playerFemmployee.settings.networkedSettings.legsBlendshapeValues
            };

            

            for (int i = 0; i < 5; i++)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = settings.bodyRegionMeshRenderers[i].GetComponent<SkinnedMeshRenderer>();
                
                if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    for (int shapeID = 0; shapeID < blendshapeValuesArrays[i].Count; shapeID++)
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(shapeID, blendshapeValuesArrays[i][shapeID]);
                    }
                } 
            }
        }

        public void SetPreviewRegion(int dropdownID, string selectionKeyName, Femmployee playerFemmployee)
        {
            playerFemmployee.settings.previewBodyParts[dropdownID] = settings.partsList[dropdownID][selectionKeyName];
            settings.bodyRegionMeshRenderers[dropdownID].sharedMesh = playerFemmployee.settings.previewBodyParts[dropdownID].mesh;

            settings.bodyRegionMeshRenderers[dropdownID].materials = playerFemmployee.settings.previewBodyParts[dropdownID].materials;

            if (Tools.CheckIsServer())
            {
                switch (dropdownID)
                {
                    case 0:
                        playerFemmployee.settings.networkedSettings.headSync = selectionKeyName;
                        break;
                    case 1:
                        playerFemmployee.settings.networkedSettings.chestSync = selectionKeyName;
                        break;
                    case 2:
                        playerFemmployee.settings.networkedSettings.armsSync = selectionKeyName;
                        break;
                    case 3:
                        playerFemmployee.settings.networkedSettings.waistSync = selectionKeyName;
                        break;
                    case 4:
                        playerFemmployee.settings.networkedSettings.legSync = selectionKeyName;
                        break;
                    default:
                        FemmployeeModBase.mls.LogWarning("Invalid dropdown ID");
                        return;
                }
            }
            else
            {
                playerFemmployee.settings.networkedSettings.SetNetworkVarServerRpc(dropdownID, selectionKeyName);
            }
        }

        public void SetBlendshape(int id, float value, string[] blendshapes, Femmployee playerFemmployee)
        {
            float[] blendshapeValues = new float[blendshapes.Length];
            for (int i = 0; i < blendshapes.Length; i++)
            {
                int shapeID = settings.bodyRegionMeshRenderers[id].sharedMesh.GetBlendShapeIndex(blendshapes[i]);
                settings.bodyRegionMeshRenderers[id].SetBlendShapeWeight(shapeID, value);
                blendshapeValues[i] = settings.bodyRegionMeshRenderers[id].GetBlendShapeWeight(shapeID);
            }

            // Update network variables with the new blendshape values if on the server or request server to update blendshape values if on the client
            if (Tools.CheckIsServer())
            {
                Action<NetworkList<float>> clearList = list =>
                {
                    while (list.Count > 0)
                    {
                        list.RemoveAt(0);
                    }
                };

                Action<NetworkList<float>, float[]> addValuesToList = (list, values) =>
                {
                    foreach (var value in values)
                    {
                        list.Add(value);
                    }
                };

                switch (id)
                {
                    case 0:
                        clearList(playerFemmployee.settings.networkedSettings.headBlendshapeValues);
                        addValuesToList(playerFemmployee.settings.networkedSettings.headBlendshapeValues, blendshapeValues);
                        break;
                    case 1:
                        clearList(playerFemmployee.settings.networkedSettings.chestBlendshapeValues);
                        addValuesToList(playerFemmployee.settings.networkedSettings.chestBlendshapeValues, blendshapeValues);
                        break;
                    case 2:
                        clearList(playerFemmployee.settings.networkedSettings.armsBlendshapeValues);
                        addValuesToList(playerFemmployee.settings.networkedSettings.armsBlendshapeValues, blendshapeValues);
                        break;
                    case 3:
                        clearList(playerFemmployee.settings.networkedSettings.waistBlendshapeValues);
                        addValuesToList(playerFemmployee.settings.networkedSettings.waistBlendshapeValues, blendshapeValues);
                        break;
                    case 4:
                        clearList(playerFemmployee.settings.networkedSettings.legsBlendshapeValues);
                        addValuesToList(playerFemmployee.settings.networkedSettings.legsBlendshapeValues, blendshapeValues);
                        break;
                    default:
                        FemmployeeModBase.mls.LogWarning("Invalid dropdown ID");
                        return;
                }
            }
            else { playerFemmployee.settings.networkedSettings.SetBlendshapeNetworkVarServerRpc(id, blendshapeValues); }
        }

        public void SetMaterialSettings(Color color, float metallicValue, float smoothnessValue, string materialName)
        {
            foreach(var SMR in settings.bodyRegionMeshRenderers)
            {
                foreach(var material in SMR.materials)
                {
                    if (material.name == materialName)
                    {
                        material.color = color;
                        material.SetFloat("_Metallic", metallicValue);
                        material.SetFloat("_Smoothness", smoothnessValue);
                    }
                }
            }
        }

    }
}
