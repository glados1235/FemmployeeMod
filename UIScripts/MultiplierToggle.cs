using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FemmployeeMod.UIScripts
{
    public class MultiplierToggle : MonoBehaviour
    {
        public FemmployeeConfigUI configUI;
        public GameObject targetElement;


        public void ToggleInteract(bool value)
        {
            targetElement?.SetActive(value);
            configUI.ToggleMultiplier(value);
        }
    }
}
