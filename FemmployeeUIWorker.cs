using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeUIWorker : MonoBehaviour
    {
        public FemmployeeConfigUI configUI;
        public int objectID;
        public GameObject targetElement;
        public bool shouldDisable;
        public GameObject[] disableList;

        
        public void ButtonTrigger()
        {
            configUI.ButtonTask(this);
        }

        public void DropdownTrigger(int selectionIndex)
        {
            configUI.DropdownSelection(this, gameObject.GetComponent<TMP_Dropdown>().value);
        }
    }
}
