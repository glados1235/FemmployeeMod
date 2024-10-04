using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FemmployeeMod.UIScripts
{
    public class RegionDropdown : MonoBehaviour
    {
        public FemmployeeConfigUI configUI;
        public int objectID;
        public GameObject targetElement;
        public bool shouldDisable;
        public GameObject[] disableList;

        public void DropdownTrigger(int selectionIndex)
        {
            TMP_Dropdown dropdown = GetComponent<TMP_Dropdown>();
            configUI.RegionDropdownSelection(this, dropdown.options[dropdown.value].text);
        }
    }
}
