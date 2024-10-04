using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FemmployeeMod.UIScripts
{
    public class BlendshapeSlider : MonoBehaviour
    {
        public FemmployeeConfigUI configUI;
        public int objectID;
        public GameObject targetElement;
        public Slider shapeSlider;
        public float DefaultSliderMax = 100;
        public BlendshapeData[] blendshapes;

        public void OnDestroy()
        {
            if (shapeSlider != null)
            {
                configUI.AllSliders[objectID].Remove(this);
            }
        }

        public void SliderValueChange(float value)
        {
            configUI.femmployeeSuitPreview.SetPreviewBlendshape(objectID, value, blendshapes, configUI.localFemmployeeGo.GetComponent<Femmployee>());
        }
    }
}
