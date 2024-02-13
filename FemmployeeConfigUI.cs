using ModelReplacement;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace FemmployeeMod
{
    public class FemmployeeConfigUI : MonoBehaviour
    {
        public FemmployeeSettings localSettings;
        public Slider breastSlider;
        public Slider bulgeSlider;
        public FemmployeeSuitPreview femmployeeSuitPreview; 
        public GameObject menu;


        public void Awake()
        {
            SetupKeybindCallbacks();
        }


        public void SetupKeybindCallbacks()
        {
            FemmployeeModBase.InputActionsInstance.FemmployeeUIToggle.performed += FemmployeeUIToggle;
        }

        public void FemmployeeUIToggle(InputAction.CallbackContext explodeConext)
        {
            if (!explodeConext.performed) return;
            localSettings.isBeingEdited = !localSettings.isBeingEdited;
            menu.SetActive(localSettings.isBeingEdited);
            Cursor.lockState = localSettings.isBeingEdited ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = localSettings.isBeingEdited;
            localSettings.controller.disableLookInput = localSettings.isBeingEdited;

        }

        public void ApplyChanges()
        {
            if (localSettings == null) return;
            FemmployeeModBase.mls.LogWarning("ApplyChanges called on the UI script!");
            localSettings.ApplySettings();
        }

        public void RotateModelLeftButton()
        {
            if (femmployeeSuitPreview != null)
            {
                femmployeeSuitPreview.RotatePreviewModel(1);
            }
        }
        public void RotateModelRightButton()
        {
            if (femmployeeSuitPreview != null)
            {
                femmployeeSuitPreview.RotatePreviewModel(-1);
            }
        }

        public void update()
        {
            if (localSettings.isBeingEdited)
            {
                localSettings.breastSize = breastSlider.value;
                localSettings.bulgeSize = bulgeSlider.value;
            }
        }
    }
}
