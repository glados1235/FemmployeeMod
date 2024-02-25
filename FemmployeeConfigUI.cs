using ModelReplacement;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using static IngamePlayerSettings;

namespace FemmployeeMod
{
    public class FemmployeeConfigUI : MonoBehaviour
    {
        public GameObject blendshapeListGo;
        public GameObject localFemmployeeGo;
        public TMP_Dropdown[] regionDropdowns;
        public bool isUIOpen;
        public FemmployeeSuitPreview femmployeeSuitPreview; 
        public GameObject menu;


        public void Awake()
        {
            SetupKeybindCallbacks();
        }

        public void Start()
        {
            if (femmployeeSuitPreview == null)
            {
                GameObject femmployeeSuitPreviewObject = (GameObject)Instantiate(Assets.MainAssetBundle.LoadAsset("Assets/ModdedUI/FemmployeeSuitPreviewPrefab/FemmployeeSuitPreviewPrefab.prefab"), new Vector3(400, -100, 400), Quaternion.identity);
                femmployeeSuitPreview = femmployeeSuitPreviewObject.GetComponent<FemmployeeSuitPreview>();
            }

        }

        public void PopulateDropdowns()
        {
            foreach(TMP_Dropdown dropdown in regionDropdowns)
            {
                dropdown.ClearOptions();
            }

            foreach (var mesh in femmployeeSuitPreview.settings.headRegionParts)
            {
                regionDropdowns[0].options.Add(new TMP_Dropdown.OptionData(mesh.name));
            }

            foreach (var mesh in femmployeeSuitPreview.settings.chestRegionParts)
            {
                regionDropdowns[1].options.Add(new TMP_Dropdown.OptionData(mesh.name));
            }

            foreach (var mesh in femmployeeSuitPreview.settings.armsRegionParts)
            {
                regionDropdowns[2].options.Add(new TMP_Dropdown.OptionData(mesh.name));
            }

            foreach (var mesh in femmployeeSuitPreview.settings.waistRegionParts)
            {
                regionDropdowns[3].options.Add(new TMP_Dropdown.OptionData(mesh.name));
            }

            foreach (var mesh in femmployeeSuitPreview.settings.legsRegionParts)
            {
                regionDropdowns[4].options.Add(new TMP_Dropdown.OptionData(mesh.name));
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
            femmployeeSuitPreview.isBeingEdited = isUIOpen;
        }

        public void ApplyChanges()
        {
            FemmployeeSuitSync.instance.ApplySettings(localFemmployeeGo.GetComponent<Femmployee>());
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

        public void DropdownSelection(FemmployeeUIWorker sender, int selectionIndex) 
        {
            femmployeeSuitPreview.SetPreviewRegion(sender.objectID, selectionIndex);
            FemmployeeModBase.mls.LogWarning($"{sender.objectID} {selectionIndex}");
        }

    }
}
