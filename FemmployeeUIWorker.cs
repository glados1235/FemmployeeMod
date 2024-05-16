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
        public string[] blendshapes;


        
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
            configUI.femmployeeSuitPreview.SetBlendshape(objectID, shapeSlider.value, blendshapes);
        }
    }
}
