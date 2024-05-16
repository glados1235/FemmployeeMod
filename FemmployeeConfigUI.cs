using ModelReplacement;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using static IngamePlayerSettings;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace FemmployeeMod
{
    public class FemmployeeConfigUI : MonoBehaviour
    {
        public GameObject localFemmployeeGo;
        public TMP_Dropdown[] regionDropdowns;
        public bool isUIOpen;
        public FemmployeeSuitPreview femmployeeSuitPreview; 
        public GameObject menu;

        public static FemmployeeConfigUI instance;


        public void Awake()
        {
            SetupKeybindCallbacks();
            instance = this;
        }

        public void Start()
        {
            if (femmployeeSuitPreview == null)
            {
                GameObject femmployeeSuitPreviewObject = (GameObject)Instantiate(Assets.MainAssetBundle.LoadAsset("FemmployeeSuitPreviewPrefab.prefab"), new Vector3(400, -100, 400), Quaternion.identity);
                femmployeeSuitPreview = femmployeeSuitPreviewObject.GetComponent<FemmployeeSuitPreview>();
                
            }

        }

        public void PopulateDropdowns()
        {
            foreach (TMP_Dropdown dropdown in regionDropdowns)
            {
                dropdown.ClearOptions();
                FemmployeeModBase.mls.LogWarning($"Dropdown options cleared for dropdown {dropdown.name}");
            }

            // Iterate over dropdowns
            for (int i = 0; i < regionDropdowns.Length; i++)
            {
                // Check if we have parts for this region
                if (i < femmployeeSuitPreview.settings.partsList.Count)
                {
                    FemmployeeModBase.mls.LogWarning($"Parts found for region {i}");

                    // Iterate over parts for this region
                    foreach (var part in femmployeeSuitPreview.settings.partsList[i])
                    {
                        // Add each part to the dropdown options
                        regionDropdowns[i].options.Add(new TMP_Dropdown.OptionData(part.Key));
                        FemmployeeModBase.mls.LogWarning($"Added part {part.Key} to dropdown {regionDropdowns[i].name}");
                    }
                }
                else
                {
                    // Log a warning if there are no parts for this region
                    FemmployeeModBase.mls.LogWarning($"No parts found for region {i}");
                }
            }
        }


        public void PopulateSliders(FemmployeeUIWorker sender)
        {
            foreach (Transform child in sender.targetElement.transform)
            {
                Destroy(child.gameObject);
            }
            string[] shapes = new string[femmployeeSuitPreview.settings.bodyRegionMeshRenderers[sender.objectID].sharedMesh.blendShapeCount];

            GameObject sliderGO = (GameObject)Assets.MainAssetBundle.LoadAsset("BlendshapeSlider.prefab");

            GameObject blendshapeSlidersGO = sender.targetElement;


            for (int i = 0; i < femmployeeSuitPreview.settings.bodyRegionMeshRenderers[sender.objectID].sharedMesh.blendShapeCount; i++)
            {
                shapes[i] = femmployeeSuitPreview.settings.bodyRegionMeshRenderers[sender.objectID].sharedMesh.GetBlendShapeName(i);
            }

            Dictionary<int, List<string>> shapeGroupsBySuffix = new Dictionary<int, List<string>>();

            foreach (string shapeName in shapes)
            {
                string[] parts = shapeName.Split('_');

                if (parts.Length > 1 && int.TryParse(parts[1], out int suffix))
                {

                    if (!shapeGroupsBySuffix.ContainsKey(suffix))
                    {

                        shapeGroupsBySuffix.Add(suffix, new List<string>());
                    }

                    shapeGroupsBySuffix[suffix].Add(shapeName);
                }
            }

            // Iterate through the shapeGroupsBySuffix dictionary
            foreach (var group in shapeGroupsBySuffix)
            {
                FemmployeeModBase.mls.LogWarning($"Numeric Suffix: {group.Key}, Blend Shapes: {string.Join(", ", group.Value)}");

                GameObject slider = Instantiate(sliderGO, blendshapeSlidersGO.transform);

                FemmployeeUIWorker worker = slider.GetComponent<FemmployeeUIWorker>();

                worker.configUI = this;
                worker.blendshapes = group.Value.ToArray();
                worker.shapeSlider.onValueChanged.AddListener(delegate { worker.SliderValueChange(1f); });
                worker.objectID = sender.objectID;
                worker.shapeSlider.value = femmployeeSuitPreview.settings.bodyRegionMeshRenderers[worker.objectID].GetBlendShapeWeight(group.Key);
                worker.targetElement.GetComponent<TMP_Text>().text = $"{group.Value[0]}";
                slider.name = $"BlendshapeSlider_{group.Key}";

            }
        }



        public void SetupKeybindCallbacks() 
        {
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed += FemmployeeUIToggle;
        }
 
        public void FemmployeeUIToggle(InputAction.CallbackContext UIOpenContext)
        {
            if (!UIOpenContext.performed) return;
            isUIOpen = !isUIOpen;
            menu.SetActive(isUIOpen);
            Cursor.lockState = isUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isUIOpen;
            GameNetworkManager.Instance.localPlayerController.disableLookInput = isUIOpen;
        }

        public void ApplyChanges()
        {
            localFemmployeeGo.GetComponent<Femmployee>().settings.networkedSettings.ApplySettings();
        }

        public void ButtonTask(FemmployeeUIWorker sender)
        {
            sender.targetElement.SetActive(!sender.targetElement.activeSelf);
            if (sender.shouldDisable)
            {
                foreach(var s in sender.disableList)
                {
                    s.SetActive(false);
                }
            }
        }

        public void DropdownSelection(FemmployeeUIWorker sender, string selectionKeyName) 
        {
            femmployeeSuitPreview.SetPreviewRegion(sender.objectID, selectionKeyName, localFemmployeeGo.GetComponent<Femmployee>());
            PopulateSliders(sender);
        }



    }
}
