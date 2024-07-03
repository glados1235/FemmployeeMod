using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;

namespace FemmployeeMod
{
    public static class Tools
    {
        public static void LogAll(object obj)
        {
            Type type = obj.GetType();
            string className = type.Name;
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                FemmployeeModBase.mls.LogWarning($"Class: {className}, Variable: {field.Name}, Value: {field.GetValue(obj)}");
            }

        }

        public static bool CheckIsServer()
        {
            if (NetworkManager.Singleton.IsServer) { return true; }
            return false;
        }

        public static Dictionary<string, float> RetriveSliderData(List<FemmployeeUIWorker> sliders)
        {
            Dictionary<string, float> keyValuePairs = new Dictionary<string, float>();
            foreach (var slider in sliders)
            {
                if (!keyValuePairs.ContainsKey(slider.blendshapes[0]))
                {
                    keyValuePairs.Add(slider.blendshapes[0], slider.shapeSlider.value);
                }
            }
            return keyValuePairs;
        }
    }



}


