using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FemmployeeMod
{
    public class FemmployeeUIWorker : MonoBehaviour
    {
        public FemmployeeConfigUI configUI;
        public int objectID;
        public GameObject targetElement;
        public bool shouldDisable;
        public GameObject[] disableList;
        public Slider shapeSlider;
        public float DefaultSliderMax = 100;
        public BlendshapeData[] blendshapes;
        public int mode;


        public void OnDestroy()
        {
            if(shapeSlider != null)
            {
                configUI.AllSliders[objectID].Remove(this);
            }
        }
        
        public void ButtonTrigger()
        {
            configUI.ButtonTask(this);
        }

        public void DropdownTrigger(int selectionIndex)
        {
            TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();
            configUI.DropdownSelection(this, dropdown.options[dropdown.value].text);
        }

        public void SliderValueChange(float value)
        {
            configUI.femmployeeSuitPreview.SetBlendshape(objectID, shapeSlider.value, blendshapes, configUI.localFemmployeeGo.GetComponent<Femmployee>());
        }

        public void ToggleInteract(bool toggle)
        {
            configUI.ToggleTask(this);
        }
    }
}
