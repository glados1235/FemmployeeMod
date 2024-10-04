using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FemmployeeMod.UIScripts
{
    public class MenuButton : MonoBehaviour
    {
        public int objectID;
        public GameObject targetElement;
        public bool shouldDisable;
        public GameObject[] disableList;

        public void ButtonTrigger()
        {
            targetElement.SetActive(!targetElement.activeSelf);
            if (shouldDisable)
            {
                foreach (var s in disableList)
                {
                    s.SetActive(false);
                }
            }
        }
    }
}
