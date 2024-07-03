using BepInEx;
using ModelReplacement;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static IngamePlayerSettings;
using static ModelReplacement.Femmployee;

namespace FemmployeeMod
{
    public class NetworkedSettings : NetworkBehaviour
    {
        public FemmployeeSettings settings;

        public string headSync
        {
            get => _headSync.Value.Value;
            internal set
            {
                _headSync.Value = new FixedString64Bytes(value);
            }
        }
        public string chestSync
        {
            get => _chestSync.Value.Value;
            internal set
            {
                _chestSync.Value = new FixedString64Bytes(value);
            }
        }
        public string armsSync
        {
            get => _armsSync.Value.Value;
            internal set
            {
                _armsSync.Value = new FixedString64Bytes(value);
            }
        }
        public string waistSync
        {
            get => _waistSync.Value.Value;
            internal set
            {
                _waistSync.Value = new FixedString64Bytes(value);
            }
        }
        public string legSync
        {
            get => _legSync.Value.Value;
            internal set
            {
                _legSync.Value = new FixedString64Bytes(value);
            }
        }

        private NetworkVariable<FixedString64Bytes> _headSync = new() { Value = "" };
        private NetworkVariable<FixedString64Bytes> _chestSync = new() { Value = "" };
        private NetworkVariable<FixedString64Bytes> _armsSync = new() { Value = "" };
        private NetworkVariable<FixedString64Bytes> _waistSync = new() { Value = "" };
        private NetworkVariable<FixedString64Bytes> _legSync = new() { Value = "" };

        public NetworkList<float> headBlendshapeValues = new NetworkList<float>();
        public NetworkList<float> chestBlendshapeValues = new NetworkList<float>();
        public NetworkList<float> armsBlendshapeValues = new NetworkList<float>();
        public NetworkList<float> waistBlendshapeValues = new NetworkList<float>();
        public NetworkList<float> legsBlendshapeValues = new NetworkList<float>();

        public NetworkVariable<NetworkMaterialProperties> suitMaterialValues = new NetworkVariable<NetworkMaterialProperties>() { Value = new NetworkMaterialProperties() };
        public NetworkVariable<NetworkMaterialProperties> skinMaterialValues = new NetworkVariable<NetworkMaterialProperties>() { Value = new NetworkMaterialProperties() };

        public NetworkVariable<int> playerID;
        public NetworkVariable<bool> hasInitialized;

        public FemmployeeConfigUI localUI;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (Tools.CheckIsServer()) { playerID.Value = -1; }
            StartCoroutine(waitForIDSync());
        }

        public IEnumerator waitForIDSync()
        {
            yield return new WaitUntil(() => playerID.Value != -1);
            StartCoroutine(WaitForFemmployeeComponent(playerID.Value));
        }

        [ClientRpc]
        public void AssignValuesClientRpc(int _playerID)
        {
            StartCoroutine(WaitForFemmployeeComponent(_playerID));
        }

        private IEnumerator WaitForFemmployeeComponent(int _playerID)
        {
            yield return new WaitUntil(() => StartOfRound.Instance.allPlayerScripts[_playerID].GetComponent<Femmployee>() != null);
            AssignValuesToComponents(_playerID);
        }

        public void AssignValuesToComponents(int _playerID)
        {
            settings = StartOfRound.Instance.allPlayerScripts[_playerID].GetComponent<Femmployee>().settings;
            settings.networkedSettings = this;
            name = name + " || Player: " + _playerID;
            localUI = StartOfRound.Instance.allPlayerScripts[playerID.Value].GetComponent<Femmployee>().localModdedUI;
            if (Tools.CheckIsServer())
            {
                headSync = settings.bodyRegionMeshRenderers[0].sharedMesh.name;
                chestSync = settings.bodyRegionMeshRenderers[1].sharedMesh.name;
                armsSync = settings.bodyRegionMeshRenderers[2].sharedMesh.name;
                waistSync = settings.bodyRegionMeshRenderers[3].sharedMesh.name;
                legSync = settings.bodyRegionMeshRenderers[4].sharedMesh.name;
                hasInitialized.Value = true;
            }

            if (File.Exists(FemmployeeModBase.saveFilePath))
            {
                LoadSuitData(SuitDataParser(File.ReadAllText(FemmployeeModBase.saveFilePath)));
            }
            else { FemmployeeModBase.mls.LogWarning("Suit data file not found."); }

            StartCoroutine(waitToAssign());
        }
        public IEnumerator waitToAssign()
        {
            yield return new WaitForSeconds(0.5f);
            ApplySettings();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetNetworkVarServerRpc(int id, string value)
        {

            switch (id)
            {
                case 0:
                    headSync = value;
                    break;
                case 1:
                    chestSync = value;
                    break;
                case 2:
                    armsSync = value;
                    break;
                case 3:
                    waistSync = value;
                    break;
                case 4:
                    legSync = value;
                    break;
                default:
                    FemmployeeModBase.mls.LogWarning("Invalid dropdown ID");
                    return;
            }
        }

        public void ApplySettings()
        {
            if (IsServer) 
            {
                SetMaterialData(localUI.suitMaterialSettings.colorValue, localUI.suitMaterialSettings.metallicValue, localUI.suitMaterialSettings.smoothnessValue, localUI.skinMaterialSettings.colorValue, localUI.skinMaterialSettings.metallicValue, localUI.skinMaterialSettings.smoothnessValue);
                ApplySettingsClientRpc();
            }
            else 
            {
                SetMaterialDataServerRpc(localUI.suitMaterialSettings.colorValue, localUI.suitMaterialSettings.metallicValue, localUI.suitMaterialSettings.smoothnessValue, localUI.skinMaterialSettings.colorValue, localUI.skinMaterialSettings.metallicValue, localUI.skinMaterialSettings.smoothnessValue);
                ApplySettingsServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ApplySettingsServerRpc()
        {
            ApplySettingsClientRpc();
        }

        [ClientRpc]
        public void ApplySettingsClientRpc()
        {
            StartOfRound.Instance.allPlayerScripts[playerID.Value].GetComponent<Femmployee>().ApplySwapRegions();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetBlendshapeNetworkVarServerRpc(int id, float[] blendshapeValues)
        {
            Action<NetworkList<float>> clearList = list =>
            {
                while (list.Count > 0)
                {
                    list.RemoveAt(0);
                }
            };

            Action<NetworkList<float>, float[]> addValuesToList = (list, values) =>
            {
                foreach (var value in values)
                {
                    list.Add(value);
                }
            };

            switch (id)
            {
                case 0:
                    clearList(headBlendshapeValues);
                    addValuesToList(headBlendshapeValues, blendshapeValues);
                    break;
                case 1:
                    clearList(chestBlendshapeValues);
                    addValuesToList(chestBlendshapeValues, blendshapeValues);
                    break;
                case 2:
                    clearList(armsBlendshapeValues);
                    addValuesToList(armsBlendshapeValues, blendshapeValues);
                    break;
                case 3:
                    clearList(waistBlendshapeValues);
                    addValuesToList(waistBlendshapeValues, blendshapeValues);
                    break;
                case 4:
                    clearList(legsBlendshapeValues);
                    addValuesToList(legsBlendshapeValues, blendshapeValues);
                    break;
                default:
                    FemmployeeModBase.mls.LogWarning("Invalid dropdown ID");
                    return;
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void SetMaterialDataServerRpc(Color suitColorValue, float suitMetallicValue, float suitSmoothnessValue, Color skinColorValue, float skinMetallicValue, float skinSmoothnessValue)
        {
            SetMaterialData(suitColorValue, skinMetallicValue, suitSmoothnessValue, skinColorValue, skinMetallicValue, skinSmoothnessValue);
        }

        public void SetMaterialData(Color suitColorValue, float suitMetallicValue, float suitSmoothnessValue, Color skinColorValue, float skinMetallicValue, float skinSmoothnessValue)
        {
            suitMaterialValues.Value.colorValue = suitColorValue;
            suitMaterialValues.Value.metallicValue = suitMetallicValue;
            suitMaterialValues.Value.smoothnessValue = suitSmoothnessValue;

            skinMaterialValues.Value.colorValue = skinColorValue;
            skinMaterialValues.Value.metallicValue = skinMetallicValue;
            skinMaterialValues.Value.smoothnessValue = skinSmoothnessValue;
        }

        public void SaveSuitData()
        {
            List<FemmployeeUIWorker>[] regionSliderLists = new List<FemmployeeUIWorker>[5];

            for (int i = 0; i < 5; i++)
            {
                regionSliderLists[i] = new List<FemmployeeUIWorker>();

                foreach (var sliderGroup in localUI.AllSliders[i])
                {
                    if (sliderGroup.objectID == i) { regionSliderLists[i].Add(sliderGroup); }
                }
            }

            FemmployeeSaveData suitData = new FemmployeeSaveData
            {
                PartsList = new PartsList
                {
                    HeadSync = headSync,
                    ChestSync = chestSync,
                    ArmsSync = armsSync,
                    WaistSync = waistSync,
                    LegSync = legSync
                },
                SliderValues =
                {
                    { "Head Sliders", Tools.RetriveSliderData(regionSliderLists[0]) },
                    { "Chest Sliders", Tools.RetriveSliderData(regionSliderLists[1]) },
                    { "Arms Sliders", Tools.RetriveSliderData(regionSliderLists[2]) },
                    { "Waist Sliders", Tools.RetriveSliderData(regionSliderLists[3]) },
                    { "Legs Sliders", Tools.RetriveSliderData(regionSliderLists[4]) }
                },
                MultiplierValue = localUI.sliderMultiplier,
                Multiplier = localUI.isMultiplierEnabled
            };

            string jsonData = JsonConvert.SerializeObject(suitData, FemmployeeModBase.useSaveFileFormatting.Value ? Formatting.Indented : Formatting.None);

            File.WriteAllText(FemmployeeModBase.saveFilePath, jsonData);
        }

        public FemmployeeSaveData SuitDataParser(string _suitData)
        {
            var suitData = JsonConvert.DeserializeObject<FemmployeeSaveData>(_suitData);
            int maxValue = localUI.isMultiplierEnabled ? localUI.sliderMultiplier * 100 : 100;
            FemmployeeSaveData parsedSaveData = suitData;

            foreach ( var region in parsedSaveData.SliderValues)
            {
                foreach(var sliderValue in region.Value)
                {
                    Mathf.Clamp(sliderValue.Value, 0, maxValue);  
                }
            }
            return parsedSaveData;
        }

        public void LoadSuitData(FemmployeeSaveData suitData)
        {

            if (!File.Exists(FemmployeeModBase.saveFilePath))
            {
                FemmployeeModBase.mls.LogWarning("Suit data file not found.");
                return;
            }

            // Mapping of region dropdowns to their corresponding suit data parts
            string[] regions = { "HeadSync", "ChestSync", "ArmsSync", "WaistSync", "LegSync" };


            if (settings.controller == GameNetworkManager.Instance.localPlayerController)
            {
                for (int i = 0; i < regions.Length; i++)
                {
                    string partSync = (string)suitData.PartsList.GetType().GetProperty(regions[i]).GetValue(suitData.PartsList);
                    localUI.regionDropdowns[i].value = localUI.regionDropdowns[i].options.FindIndex(option => option.text == partSync);
                }

                localUI.isMultiplierEnabled = suitData.Multiplier;
                localUI.sliderMultiplier = suitData.MultiplierValue;
                localUI.multiplierDropdown.value = suitData.MultiplierValue - 1;
                localUI.multiplierToggle.isOn = suitData.Multiplier;

                for (int i = 0; i < 5; i++)
                {
                    string sliderKey = $"{regions[i].Replace("Sync", "")} Sliders";
                    foreach (var slider in localUI.AllSliders[i])
                    {
                        if (slider.objectID == i)
                        {
                            slider.shapeSlider.maxValue = slider.DefaultSliderMax * suitData.MultiplierValue;
                            slider.shapeSlider.value = suitData.SliderValues[sliderKey][slider.blendshapes[0]];
                        }
                    }
                }
            }
            
            if (Tools.CheckIsServer())
            {
                headSync = suitData.PartsList.HeadSync;
                chestSync = suitData.PartsList.ChestSync;
                armsSync = suitData.PartsList.ArmsSync;
                waistSync = suitData.PartsList.WaistSync;
                legSync = suitData.PartsList.LegSync;

            }
            else
            {
                string stringSaveData = JsonConvert.SerializeObject(suitData, FemmployeeModBase.useSaveFileFormatting.Value ? Formatting.Indented : Formatting.None);
                LoadSuitSaveDataServerRpc(stringSaveData);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void LoadSuitSaveDataServerRpc(string suitDataString)
        {
            var suitData = JsonConvert.DeserializeObject<FemmployeeSaveData>(suitDataString);
            LoadSuitData(suitData);
        }

        public void SelfDestruct()
        {
            this.GetComponent<NetworkObject>().Despawn(true);
        }

    }
}
