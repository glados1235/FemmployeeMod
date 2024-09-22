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


namespace FemmployeeMod
{
    public class FemmployeeConfigUI : MonoBehaviour
    {
        public GameObject localFemmployeeGo;
        public TMP_Dropdown[] regionDropdowns;
        public TMP_Dropdown multiplierDropdown;
        public FemmployeeUIMaterialSettings suitMaterialSettings;
        public FemmployeeUIMaterialSettings skinMaterialSettings;
        public Toggle multiplierToggle;
        public List<FemmployeeUIWorker>[] AllSliders = new List<FemmployeeUIWorker>[5];
        public bool isUIOpen;
        public FemmployeeSuitPreview femmployeeSuitPreview;
        public Slider previewSpinSlider;
        public GameObject menuRoot;
        public GameObject mainMenu;
        public GameObject settingsMenu;
        public int sliderMultiplier;
        public bool isMultiplierEnabled;
        public static FemmployeeConfigUI instance;

        public void Awake()
        {
            SetupKeybindCallbacks();
            for (int i = 0; i < AllSliders.Length; i++)
            {
                AllSliders[i] = new List<FemmployeeUIWorker>();
            }
            instance = this;
        }

        public void Start()
        {
            if (femmployeeSuitPreview == null)
            {
                GameObject femmployeeSuitPreviewObject = (GameObject)Instantiate(Assets.MainAssetBundle.LoadAsset("FemmployeeSuitPreviewPrefab.prefab"), new Vector3(400, -100, 400), Quaternion.identity);
                femmployeeSuitPreview = femmployeeSuitPreviewObject.GetComponent<FemmployeeSuitPreview>();
                femmployeeSuitPreview.previewSpinSlider = previewSpinSlider;
            }
            foreach (var regionDropdown in regionDropdowns)
            {
                PopulateSliders(regionDropdown.GetComponent<FemmployeeUIWorker>());
            }
            sliderMultiplier = 1;
        }

        public void Update()
        {
            if (isUIOpen)
            {
                foreach (var regionDropdown in regionDropdowns)
                {
                    if (regionDropdown.IsExpanded) { regionDropdown.GetComponent<FemmployeeUIWorker>().targetElement.SetActive(false); }
                    else { regionDropdown.GetComponent<FemmployeeUIWorker>().targetElement.SetActive(true); }
                }
            }
        }

        public void PopulateDropdowns()
        {
            foreach (TMP_Dropdown dropdown in regionDropdowns)
            {
                dropdown.ClearOptions();
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

        public void SetMultiplier(int value)
        {
            for (int i = 0; i < 5; i++)
            {
                foreach(var slider in AllSliders[i])
                {
                    slider.shapeSlider.maxValue = slider.DefaultSliderMax * value;
                    FemmployeeModBase.mls.LogInfo($"Assisgned the value {slider.shapeSlider.maxValue} to the slider {slider.blendshapes[0]}");
                }
            }
        }

        public void PopulateSliders(FemmployeeUIWorker sender)
        {
            foreach (Transform child in sender.targetElement.transform)
            {
                Destroy(child.gameObject);
            }

            AllSliders[sender.objectID].Clear();

            List<BlendshapeData> allShapes = new List<BlendshapeData>();

            GameObject sliderGO = (GameObject)Assets.MainAssetBundle.LoadAsset("BlendshapeSlider.prefab");

            GameObject blendshapeSlidersGO = sender.targetElement;

            string[] regionSuffix = { "Head", "Chest", "Arms", "Waist", "Leg" };

            foreach (var smr in femmployeeSuitPreview.settings.bodyRegionMeshRenderers)
            {
                int regionID = Array.IndexOf(femmployeeSuitPreview.settings.bodyRegionMeshRenderers, smr);

                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    string shapeName = smr.sharedMesh.GetBlendShapeName(i);
                    string[] parts = shapeName.Split('_');

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

            foreach (var t in allShapes)
            {
                FemmployeeModBase.mls.LogWarning($"{t.OriginalRegionID}  {t.ControllingRegionID}  {t.BlendshapeName}");
            }

            var shapeGroupsBySuffix = allShapes
                .GroupBy(shape => shape.BlendshapeName.Split('_')[1])
                .ToDictionary(g => int.Parse(g.Key), g => g.ToList());

            foreach (var group in shapeGroupsBySuffix)
            {

                
                GameObject slider = Instantiate(sliderGO, blendshapeSlidersGO.transform);

                FemmployeeUIWorker worker = slider.GetComponent<FemmployeeUIWorker>();

                worker.configUI = this;
                worker.blendshapes = group.Value.ToArray();
                worker.shapeSlider.onValueChanged.AddListener(delegate { worker.SliderValueChange(1f); });
                worker.objectID = sender.objectID;
                worker.shapeSlider.maxValue = worker.DefaultSliderMax * sliderMultiplier;

                int shapeIndex = femmployeeSuitPreview.settings.bodyRegionMeshRenderers[group.Value[0].OriginalRegionID].sharedMesh.GetBlendShapeIndex(group.Value[0].BlendshapeName);
                FemmployeeModBase.mls.LogMessage($"the shape Index of group {group.Key} is {shapeIndex} here more data kek {group.Value[0].BlendshapeName}");
                worker.shapeSlider.value = femmployeeSuitPreview.settings.bodyRegionMeshRenderers[worker.objectID].GetBlendShapeWeight(shapeIndex);

                string originalText = group.Value[0].BlendshapeName;
                string modifiedText = Regex.Replace(originalText, @"_.*$", "");

                worker.targetElement.GetComponent<TMP_Text>().text = modifiedText;

                slider.name = $"BlendshapeSlider_{group.Key}";
                AllSliders[sender.objectID].Add(worker);
            }
        }

        public void OnDestroy()
        {
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed -= FemmployeeUIToggle;
        }

        public void SetupKeybindCallbacks() 
        {
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed += FemmployeeUIToggle;
        }
 
        public void FemmployeeUIToggle(InputAction.CallbackContext UIOpenContext)
        {
            if (!UIOpenContext.performed) return;
            if (GameNetworkManager.Instance.localPlayerController.TryGetComponent<Femmployee>(out _))
            {
                isUIOpen = !isUIOpen;
                menuRoot.SetActive(isUIOpen);
                mainMenu.SetActive(true);
                settingsMenu.SetActive(false);

                Cursor.lockState = isUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = isUIOpen;
                GameNetworkManager.Instance.localPlayerController.disableLookInput = isUIOpen;
                GameNetworkManager.Instance.localPlayerController.inTerminalMenu = isUIOpen;
            }
        }

        public void ApplyChanges()
        {
            localFemmployeeGo.GetComponent<Femmployee>().settings.networkedSettings.ApplySettings();
            localFemmployeeGo.GetComponent<Femmployee>().settings.networkedSettings.SaveSuitData();
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

        public void ToggleMultiplier(bool value, FemmployeeUIWorker sender)
        {
            isMultiplierEnabled = value;
            sender.targetElement?.SetActive(value);
            if (!value)
            {
                SetMultiplier(1);
                sliderMultiplier = 1;
                multiplierDropdown.value = 0;
            }
        }

        public void ToggleTask(FemmployeeUIWorker sender)
        {
            if (sender.mode == 0) 
            {
                ToggleMultiplier(sender.GetComponent<Toggle>().isOn, sender);
            }

        }

        public void SetSliderMultiplier(int value)
        {
            sliderMultiplier = value + 1;
            SetMultiplier(sliderMultiplier);
        }

        public void DropdownSelection(FemmployeeUIWorker sender, string selectionKeyName) 
        {
            if(sender.mode == 0)
            {
                femmployeeSuitPreview.SetPreviewRegion(sender.objectID, selectionKeyName, localFemmployeeGo.GetComponent<Femmployee>());
                PopulateSliders(sender);
            }
            if(sender.mode == 1)
            {
                if (isMultiplierEnabled)
                {
                    SetSliderMultiplier(sender.GetComponent<TMP_Dropdown>().value);
                } 
            }

        }

        public void SendColorData()
        { 

            femmployeeSuitPreview.SetMaterialSettings(suitMaterialSettings.colorValue, suitMaterialSettings.metallicValue, suitMaterialSettings.smoothnessValue, skinMaterialSettings.colorValue, skinMaterialSettings.metallicValue, skinMaterialSettings.smoothnessValue, localFemmployeeGo.GetComponent<Femmployee>());
        }


    }
}
