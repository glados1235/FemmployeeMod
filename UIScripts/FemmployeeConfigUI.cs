using ModelReplacement;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using static IngamePlayerSettings;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.VFX;
using BepInEx.Configuration;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using static FemmployeeMod.FemmployeeSuitPreview;


namespace FemmployeeMod.UIScripts
{
    public class FemmployeeConfigUI : MonoBehaviour
    {
        // References to UI elements and settings
        public GameObject localFemmployeeGo;
        public TMP_Dropdown[] regionDropdowns;
        public TMP_Dropdown multiplierDropdown;
        public FemmployeeUIMaterialSettings suitMaterialSettings;
        public FemmployeeUIMaterialSettings skinMaterialSettings;
        public Toggle multiplierToggle;
        public List<BlendshapeSlider>[] AllSliders = new List<BlendshapeSlider>[5];
        public bool isUIOpen;
        public FemmployeeSuitPreview femmployeeSuitPreview;
        public Slider previewSpinSlider;
        public GameObject menuRoot;
        public GameObject mainMenu;
        public GameObject settingsMenu;
        public int sliderMultiplier;
        public bool isMultiplierEnabled;
        public static FemmployeeConfigUI instance;

        // Called when the script instance is being loaded
        public void Awake()
        {
            SetupKeybindCallbacks(); // Set up keybind callbacks
            for (int i = 0; i < AllSliders.Length; i++)
            {
                AllSliders[i] = new List<BlendshapeSlider>(); // Initialize AllSliders array
            }
            instance = this; // Set the static instance
        }

        // Called before the first frame updates
        public void Start()
        {
            if (femmployeeSuitPreview == null)
            {
                // Instantiate the suit preview object if it doesn't exist
                GameObject femmployeeSuitPreviewObject = (GameObject)Instantiate(Assets.MainAssetBundle.LoadAsset("FemmployeeSuitPreviewPrefab.prefab"), new Vector3(400, -100, 400), Quaternion.identity);
                femmployeeSuitPreview = femmployeeSuitPreviewObject.GetComponent<FemmployeeSuitPreview>();
                femmployeeSuitPreview.previewSpinSlider = previewSpinSlider; // Set the spin slider
            }

            sliderMultiplier = 1; // Initialize slider multiplier

            foreach (var regionDropdown in regionDropdowns)
            {
                // Populate sliders for each dropdown
                PopulateSliders(regionDropdown.GetComponent<RegionDropdown>());
            }
        }

        public void Update()
        {
            if (isUIOpen)
            {
                // Toggle visibility of target elements based on the state of region dropdowns
                foreach (var regionDropdown in regionDropdowns)
                {
                    if (regionDropdown.IsExpanded) { regionDropdown.GetComponent<RegionDropdown>().targetElement.SetActive(false); }
                    else { regionDropdown.GetComponent<RegionDropdown>().targetElement.SetActive(true); }
                }
            }
        }

        // Clear and repopulate the dropdown options
        public void PopulateDropdowns()
        {
            foreach (TMP_Dropdown dropdown in regionDropdowns)
            {
                dropdown.ClearOptions(); // Clear existing options
            }

            // Iterate over dropdowns
            for (int i = 0; i < regionDropdowns.Length; i++)
            {
                // Check if we have parts for this region
                if (i < femmployeeSuitPreview.settings.partsList.Count)
                {
                    // Iterate over parts for this region
                    foreach (var part in femmployeeSuitPreview.settings.partsList[i])
                    {
                        // Add each part to the dropdown options
                        regionDropdowns[i].options.Add(new TMP_Dropdown.OptionData(part.Key));
                    }
                }
                else
                {
                    // Log a warning if there are no parts for this region
                    FemmployeeModBase.mls.LogWarning($"No parts found for region {i}");
                }
            }
            
        }

        // Set the multiplier for all sliders
        public void SetMultiplier(int value)
        {
            for (int i = 0; i < 5; i++)
            {
                foreach (var slider in AllSliders[i])
                {
                    slider.shapeSlider.maxValue = slider.DefaultSliderMax * value; // Update max value of sliders
                }
            }
        }

        // Populate sliders for a specific region dropdown
        public void PopulateSliders(RegionDropdown sender)
        {
            // Clear existing sliders in the target element
            foreach (Transform child in sender.targetElement.transform)
            {
                Destroy(child.gameObject);
            }

            // Reset the list of sliders for the current region
            AllSliders[sender.objectID].Clear();

            // Create a list to store all blendshape data
            List<BlendshapeData> allShapes = new List<BlendshapeData>();

            // Load the prefab for the blendshape slider
            GameObject sliderPrefabGO = (GameObject)Assets.MainAssetBundle.LoadAsset("BlendshapeSlider.prefab");

            // Get the parent object where sliders will be instantiated
            GameObject blendshapeSlidersGO = sender.targetElement;

            // Define region suffixes for easier identification
            string[] regionSuffix = { "Head", "Chest", "Arms", "Waist", "Leg" };

            // Iterate through all mesh renderers in the femmployee suit preview
            foreach (var smr in femmployeeSuitPreview.settings.bodyRegionMeshRenderers)
            {
                int regionID = Array.IndexOf(femmployeeSuitPreview.settings.bodyRegionMeshRenderers, smr);

                // Check each blendshape in the mesh renderer
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    string shapeName = smr.sharedMesh.GetBlendShapeName(i);
                    string[] parts = shapeName.Split('_');

                    // Filter blendshapes based on the correct region and format
                    if (parts.Length > 2 && int.TryParse(parts[1], out int suffix) && parts[2] == regionSuffix[sender.objectID])
                    {
                        allShapes.Add(new BlendshapeData
                        {
                            OriginalRegionID = regionID,
                            ControllingRegionID = sender.objectID,
                            BlendshapeName = shapeName
                        });
                    }
                }
            }

            // Group blendshapes by their numeric suffix
            var shapeGroupsBySuffix = allShapes
               .GroupBy(shape => shape.BlendshapeName.Split('_')[1])
               .ToDictionary(g => int.Parse(g.Key), g => g.ToList());

            // Create sliders for each group of blendshapes
            foreach (var group in shapeGroupsBySuffix)
            {
                // Instantiate a new slider prefab
                GameObject sliderGO = Instantiate(sliderPrefabGO, blendshapeSlidersGO.transform);

                // Get the BlendshapeSlider component from the instantiated object
                BlendshapeSlider slider = sliderGO.GetComponent<BlendshapeSlider>();

                // Set up the slider properties
                slider.configUI = this;
                slider.blendshapes = group.Value.ToArray();
                slider.objectID = sender.objectID;
                slider.shapeSlider.maxValue = slider.DefaultSliderMax * sliderMultiplier;

                // Find the index of the first blendshape in the group
                int shapeIndex = femmployeeSuitPreview.settings.bodyRegionMeshRenderers[group.Value[0].OriginalRegionID].sharedMesh.GetBlendShapeIndex(group.Value[0].BlendshapeName);

                // Set the initial slider value based on the current blendshape weight
                slider.shapeSlider.value = femmployeeSuitPreview.settings.bodyRegionMeshRenderers[slider.objectID].GetBlendShapeWeight(shapeIndex);

                // Extract and set the display text for the slider
                string originalText = group.Value[0].BlendshapeName;
                string modifiedText = Regex.Replace(originalText, @"_.*$", "");
                slider.targetElement.GetComponent<TMP_Text>().text = modifiedText;

                // Name the slider object
                slider.name = $"BlendshapeSlider_{group.Key}";

                // Add the slider to the list of sliders for the current region
                AllSliders[sender.objectID].Add(slider);
            }
        }

        // Called when the script is being destroyed
        public void OnDestroy()
        {
            // Unbind the UI toggle key action
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed -= FemmployeeUIToggle;
        }

        // Set up keybind callbacks
        public void SetupKeybindCallbacks()
        {
            // Bind the UI toggle key action
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed += FemmployeeUIToggle;
        }

        // Toggle the state of the UI
        public void FemmployeeUIToggle(InputAction.CallbackContext UIOpenContext)
        {
            if (!UIOpenContext.performed) return; // Return if the UI toggle action wasn't performed
            if (GameNetworkManager.Instance.localPlayerController.TryGetComponent<Femmployee>(out _))
            {
                isUIOpen = !isUIOpen; // Toggle the UI state
                menuRoot.SetActive(isUIOpen);
                mainMenu.SetActive(isUIOpen);
                settingsMenu.SetActive(false);

                Cursor.lockState = isUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = isUIOpen;
                GameNetworkManager.Instance.localPlayerController.disableLookInput = isUIOpen;
                GameNetworkManager.Instance.localPlayerController.inTerminalMenu = isUIOpen;
            }
        }

        // Apply the changes to the Femmployee game object
        public void ApplyChanges()
        {
            localFemmployeeGo.GetComponent<Femmployee>().settings.networkedSettings.ApplySettings();
            localFemmployeeGo.GetComponent<Femmployee>().settings.networkedSettings.SaveSuitData();
        }

        // Toggle the state of the multiplier
        public void ToggleMultiplier(bool value)
        {
            isMultiplierEnabled = value;
            if (!value)
            {
                SetMultiplier(1); // Reset the multiplier to 1
                sliderMultiplier = 1;
                multiplierDropdown.value = 0;
            }
        }

        // Set the slider multiplier value
        public void SetSliderMultiplier(int value)
        {
            sliderMultiplier = value + 1;
            SetMultiplier(sliderMultiplier);
        }

        // Handle region dropdown selection
        public void RegionDropdownSelection(RegionDropdown sender, string selectionKeyName)
        {
            int objectID = sender.objectID;
            femmployeeSuitPreview.SetPreviewRegion(objectID, selectionKeyName, localFemmployeeGo.GetComponent<Femmployee>());
            PopulateSliders(sender); // Repopulate sliders for the selected region
        }

        // Handle multiplier dropdown selection
        public void MultiplierDropdownSelection(int value)
        {
            if (isMultiplierEnabled)
            {
                SetSliderMultiplier(value);
            }
        }

        // Send color data to the suit preview
        public void SendColorData()
        {
            femmployeeSuitPreview.SetMaterialSettings(suitMaterialSettings.colorValue, suitMaterialSettings.metallicValue, suitMaterialSettings.smoothnessValue, skinMaterialSettings.colorValue, skinMaterialSettings.metallicValue, skinMaterialSettings.smoothnessValue, localFemmployeeGo.GetComponent<Femmployee>());
        }
    }
}
