using ModelReplacement;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using static IngamePlayerSettings;

namespace FemmployeeMod
{
    public class FemmployeeConfigUI : MonoBehaviour
    {
        public FemmployeeSettings localSettings;
        public GameObject blendshapeListGo;
        public GameObject[] regionDropdowns;
        public Slider breastSlider;
        public Slider bulgeSlider;
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

        public void SetupKeybindCallbacks()
        {
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed += FemmployeeUIToggle;
        }

        public void FemmployeeUIToggle(InputAction.CallbackContext explodeConext)
        {
            if (!explodeConext.performed) return;
            isUIOpen = !isUIOpen;
            menu.SetActive(isUIOpen);
            Cursor.lockState = isUIOpen ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = isUIOpen;
            localSettings.controller.disableLookInput = isUIOpen;
            femmployeeSuitPreview.isBeingEdited = isUIOpen;
        }

        public void ApplyChanges()
        {
            if (localSettings == null) return;
            localSettings.ApplySettings();
            Tools.LogAll(this);
            FemmployeeModBase.mls.LogWarning(breastSlider.value);
            FemmployeeModBase.mls.LogWarning(bulgeSlider.value);
        }

        public void RotateModelLeftButton()
        {
            if (femmployeeSuitPreview != null)
            {
                femmployeeSuitPreview.RotatePreviewModel(100);
            }
        }
        public void RotateModelRightButton()
        {
            if (femmployeeSuitPreview != null)
            {
                femmployeeSuitPreview.RotatePreviewModel(-100);
            }
        }

        public void update()
        {
            if (isUIOpen)
            {
                localSettings.breastSize = breastSlider.value;
                localSettings.bulgeSize = bulgeSlider.value;
                femmployeeSuitPreview.breastSize = breastSlider.value;
                femmployeeSuitPreview.bulgeSize = bulgeSlider.value;
            }
        }

        public void ButtonTask(ButtonWorker sender)
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

        public void UpdateBlendshapeSliderList()
        {

        }

    }
}
