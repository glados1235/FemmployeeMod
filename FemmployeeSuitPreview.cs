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

        public void PreviewLoadSaveData(Femmployee playerFemmployee)
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

            NetworkList<BlendshapeValuePair>[] blendshapeValuesArrays = new NetworkList<BlendshapeValuePair>[]
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
                        skinnedMeshRenderer.SetBlendShapeWeight(i, blendshapeValuesArrays[i][shapeID].ShapeValue);
                    }
                }
            }

            foreach (var SMR in settings.bodyRegionMeshRenderers)
            {
                foreach (var material in SMR.materials)
                {
                    if (material.name == "Suit (Instance)")
                    {
                        material.color = playerFemmployee.settings.networkedSettings.suitMaterialValues.Value.colorValue;
                        material.SetFloat("_Metallic", playerFemmployee.settings.networkedSettings.suitMaterialValues.Value.metallicValue);
                        material.SetFloat("_Smoothness", playerFemmployee.settings.networkedSettings.suitMaterialValues.Value.smoothnessValue);
                    }
                }

                foreach (var material in SMR.materials)
                {
                    if (material.name == "Skin (Instance)")
                    {
                        material.color = playerFemmployee.settings.networkedSettings.skinMaterialValues.Value.colorValue;
                        material.SetFloat("_Metallic", playerFemmployee.settings.networkedSettings.skinMaterialValues.Value.metallicValue);
                        material.SetFloat("_Smoothness", playerFemmployee.settings.networkedSettings.skinMaterialValues.Value.smoothnessValue);
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

            SetMaterialSettings(playerFemmployee.settings.networkedSettings.suitMaterialValues.Value.colorValue, playerFemmployee.settings.networkedSettings.suitMaterialValues.Value.metallicValue, playerFemmployee.settings.networkedSettings.suitMaterialValues.Value.smoothnessValue,
                playerFemmployee.settings.networkedSettings.skinMaterialValues.Value.colorValue, playerFemmployee.settings.networkedSettings.skinMaterialValues.Value.metallicValue, playerFemmployee.settings.networkedSettings.skinMaterialValues.Value.smoothnessValue, playerFemmployee);
        }

        public void SetPreviewBlendshape(int id, float value, BlendshapeData[] blendshapes, Femmployee playerFemmployee)
        {

            for (int i = 0; i < blendshapes.Length; i++)
            {
                BlendshapeData blendshapeData = blendshapes[i];
                int originalRegionID = blendshapeData.OriginalRegionID;

                // Get the shape ID based on the blendshape name within the correct original region
                int shapeId = settings.bodyRegionMeshRenderers[originalRegionID].sharedMesh.GetBlendShapeIndex(blendshapeData.BlendshapeName);

                if (shapeId != -1)
                {
                    // Set the blendshape weight for the region
                    settings.bodyRegionMeshRenderers[originalRegionID].SetBlendShapeWeight(shapeId, value);
                    playerFemmployee.settings.networkedSettings.SetBlendshapeValue(value, blendshapes);
                }
            }
        }


        public void SetMaterialSettings(Color suitColorValue, float suitMetallicValue, float suitSmoothnessValue, Color skinColorValue, float skinMetallicValue, float skinSmoothnessValue, Femmployee playerFemmployee)
        {

            foreach (var SMR in settings.bodyRegionMeshRenderers)
            {
                foreach (var material in SMR.materials)
                {
                    if (material.name == "Suit (Instance)")
                    {
                        material.color = suitColorValue;
                        material.SetFloat("_Metallic", suitMetallicValue);
                        material.SetFloat("_Smoothness", suitSmoothnessValue);
                    }

                    if (material.name == "Skin (Instance)")
                    {
                        material.color = skinColorValue;
                        material.SetFloat("_Metallic", skinMetallicValue);
                        material.SetFloat("_Smoothness", skinSmoothnessValue);
                    }
                }
            }

            if (Tools.CheckIsServer())
            {
                playerFemmployee.settings.networkedSettings.SetMaterialData(suitColorValue, suitMetallicValue, suitSmoothnessValue, skinColorValue, skinMetallicValue, skinSmoothnessValue);
            }
            else
            {
                playerFemmployee.settings.networkedSettings.SetMaterialDataServerRpc(suitColorValue, suitMetallicValue, suitSmoothnessValue, skinColorValue, skinMetallicValue, skinSmoothnessValue);
            }
        }

    }

    public struct BlendshapeData
    {
        public int OriginalRegionID; // The region where the blendshape is located
        public int ControllingRegionID; // The region that controls the blendshape (based on the slider)
        public string BlendshapeName; // The full name of the blendshape
    }

}
