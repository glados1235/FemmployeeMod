using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FemmployeeMod
{
    public class ButtonWorker : MonoBehaviour
    {
        public FemmployeeConfigUI configUI;
        public GameObject targetElement;
        public bool shouldDisable;
        public GameObject[] disableList;

        
        public void OnClick()
        {
            configUI.ButtonTask(this);
        }
    }
}
