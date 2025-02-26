﻿using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace FemmployeeMod.UIScripts
{
    public class FemmployeeUIMaterialSettings : MonoBehaviour
    {
        public FemmployeeConfigUI localFemmployeeConfigUI;

        public Slider[] RGBSliders;
        public Slider metallicSlider;
        public Slider smoothnessSlider;

        public Color colorValue;
        public float metallicValue;
        public float smoothnessValue;

        public Color defaultColorValue;
        public float defaultMetallicValue = 0;
        public float defaultSmoothnessValue = 0;

        public string materialName;

        public void SetColorValue(float value)
        {
            colorValue.r = RGBSliders[0].value;
            colorValue.g = RGBSliders[1].value;
            colorValue.b = RGBSliders[2].value;
            localFemmployeeConfigUI.SendColorData();
        }

        public void SetMetallicValue(float value)
        {
            metallicValue = metallicSlider.value;
            localFemmployeeConfigUI.SendColorData();
        }

        public void SetSmoothnessSlider(float value)
        {
            smoothnessValue = smoothnessSlider.value;
            localFemmployeeConfigUI.SendColorData();
        }


        public void ResetColorValue(bool value)
        {
            colorValue = defaultColorValue;
            RGBSliders[0].value = defaultColorValue.r;
            RGBSliders[1].value = defaultColorValue.g;
            RGBSliders[2].value = defaultColorValue.b;
        }

        public void ResetMetallicsSlider(bool value)
        {
            metallicValue = defaultMetallicValue;
            metallicSlider.value = defaultMetallicValue;
        }

        public void ResetSmoothnessSlider(bool value)
        {
            smoothnessValue = defaultSmoothnessValue;
            smoothnessSlider.value = defaultSmoothnessValue;
        }

    }
}
