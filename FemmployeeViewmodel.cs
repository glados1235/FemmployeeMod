using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FemmployeeMod
{
    public class FemmployeeViewmodel : MonoBehaviour
    {
        public FemmployeeSettings settings;

        public void Start()
        {
            settings = gameObject.GetComponent<FemmployeeSettings>();
        }

    }
}
