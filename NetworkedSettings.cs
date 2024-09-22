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

        public NetworkList<BlendshapeValuePair> headBlendshapeValues = new NetworkList<BlendshapeValuePair>();
        public NetworkList<BlendshapeValuePair> chestBlendshapeValues = new NetworkList<BlendshapeValuePair>();
        public NetworkList<BlendshapeValuePair> armsBlendshapeValues = new NetworkList<BlendshapeValuePair>();
        public NetworkList<BlendshapeValuePair> waistBlendshapeValues = new NetworkList<BlendshapeValuePair>();
        public NetworkList<BlendshapeValuePair> legsBlendshapeValues = new NetworkList<BlendshapeValuePair>();

        public NetworkVariable<NetworkMaterialProperties> suitMaterialValues = new NetworkVariable<NetworkMaterialProperties>();
        public NetworkVariable<NetworkMaterialProperties> skinMaterialValues = new NetworkVariable<NetworkMaterialProperties>();

        public NetworkVariable<int> playerID;
        public NetworkVariable<bool> hasInitialized;

        public FemmployeeConfigUI localUI;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (Tools.CheckIsServer()) 
            { 
                playerID.Value = -1;
                suitMaterialValues.Value = new NetworkMaterialProperties();
                skinMaterialValues.Value = new NetworkMaterialProperties();
            }
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

            if (settings.controller == GameNetworkManager.Instance.localPlayerController && File.Exists(FemmployeeModBase.saveFilePath))
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
                ApplySettingsClientRpc();
            }
            else 
            {
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
        public void SetBlendshapeNetworkVarServerRpc(int id, float[] shapeValues, int[] shapeIDs)
        {
            FemmployeeModBase.mls.LogWarning($"debugging ShapeValues for regionID {id}");
            foreach (var value in shapeValues)
            {
                FemmployeeModBase.mls.LogWarning($"{value}");
            }
            FemmployeeModBase.mls.LogWarning($" ");
            FemmployeeModBase.mls.LogWarning($"debugging shapeIDs for regionID {id}");
            foreach (var shapeID in shapeIDs)
            {
                FemmployeeModBase.mls.LogWarning($"{shapeID}");
            }
            FemmployeeModBase.mls.LogWarning($" ");
            FemmployeeModBase.mls.LogWarning($" ");
            FemmployeeModBase.mls.LogWarning($"|||||| NEW LINE |||||||");
            FemmployeeModBase.mls.LogWarning($" ");
            FemmployeeModBase.mls.LogWarning($" ");

            void ClearList(NetworkList<BlendshapeValuePair> list)
            {
                while (list.Count > 0)
                {
                    list.RemoveAt(0);
                }
            }

            void AddValuesToList(NetworkList<BlendshapeValuePair> list, float[] values, int[] ids)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    list.Add(new BlendshapeValuePair(values[i], ids[i]));
                }
            }

            // Clear and add values for the specific region's networked list
            switch (id)
            {
                case 0:
                    ClearList(headBlendshapeValues);
                    AddValuesToList(headBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 1:
                    ClearList(chestBlendshapeValues);
                    AddValuesToList(chestBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 2:
                    ClearList(armsBlendshapeValues);
                    AddValuesToList(armsBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 3:
                    ClearList(waistBlendshapeValues);
                    AddValuesToList(waistBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 4:
                    ClearList(legsBlendshapeValues);
                    AddValuesToList(legsBlendshapeValues, shapeValues, shapeIDs);
                    break;
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void SetMaterialDataServerRpc(Color suitColorValue, float suitMetallicValue, float suitSmoothnessValue, Color skinColorValue, float skinMetallicValue, float skinSmoothnessValue)
        {
            SetMaterialData(suitColorValue, skinMetallicValue, suitSmoothnessValue, skinColorValue, skinMetallicValue, skinSmoothnessValue);
        }

        public void SetMaterialData(Color suitColorValue, float suitMetallicValue, float suitSmoothnessValue, Color skinColorValue, float skinMetallicValue, float skinSmoothnessValue)
        {
            NetworkMaterialProperties skinProperties = new NetworkMaterialProperties
            {
                colorValue = skinColorValue,
                metallicValue = skinMetallicValue,
                smoothnessValue = skinSmoothnessValue
            };

            NetworkMaterialProperties suitProperties = new NetworkMaterialProperties
            {
                colorValue = suitColorValue,
                metallicValue = suitMetallicValue,
                smoothnessValue = suitSmoothnessValue
            };

            suitMaterialValues.Value = suitProperties;

            skinMaterialValues.Value = skinProperties;
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
                Multiplier = localUI.isMultiplierEnabled,
                suitMaterialData = new MaterialData
                {
                    colorValueR = suitMaterialValues.Value.colorValue.r,
                    colorValueG = suitMaterialValues.Value.colorValue.g,
                    colorValueB = suitMaterialValues.Value.colorValue.b,
                    metallicValue = suitMaterialValues.Value.metallicValue,
                    smoothnessValue = suitMaterialValues.Value.smoothnessValue
                },
                skinMaterialData = new MaterialData
                {
                    colorValueR = skinMaterialValues.Value.colorValue.r,
                    colorValueG = skinMaterialValues.Value.colorValue.g,
                    colorValueB = skinMaterialValues.Value.colorValue.b,
                    metallicValue = skinMaterialValues.Value.metallicValue,
                    smoothnessValue = skinMaterialValues.Value.smoothnessValue
                }
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

                localUI.suitMaterialSettings.RGBSliders[0].value = suitData.suitMaterialData.colorValueR;
                localUI.suitMaterialSettings.RGBSliders[1].value = suitData.suitMaterialData.colorValueG;
                localUI.suitMaterialSettings.RGBSliders[2].value = suitData.suitMaterialData.colorValueB;
                localUI.suitMaterialSettings.metallicSlider.value = suitData.suitMaterialData.metallicValue;
                localUI.suitMaterialSettings.smoothnessSlider.value = suitData.suitMaterialData.smoothnessValue;

                localUI.skinMaterialSettings.RGBSliders[0].value = suitData.skinMaterialData.colorValueR;
                localUI.skinMaterialSettings.RGBSliders[1].value = suitData.skinMaterialData.colorValueG;
                localUI.skinMaterialSettings.RGBSliders[2].value = suitData.skinMaterialData.colorValueB;
                localUI.skinMaterialSettings.metallicSlider.value = suitData.skinMaterialData.metallicValue;
                localUI.skinMaterialSettings.smoothnessSlider.value = suitData.skinMaterialData.smoothnessValue;

                for (int i = 0; i < 5; i++)
                {
                    string sliderKey = $"{regions[i].Replace("Sync", "")} Sliders";
                    foreach (var slider in localUI.AllSliders[i])
                    {
                        if (slider.objectID == i)
                        {
                            slider.shapeSlider.maxValue = slider.DefaultSliderMax * suitData.MultiplierValue;
                            slider.shapeSlider.value = suitData.SliderValues[sliderKey][slider.blendshapes[0].BlendshapeName];
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

                Color suitColor = new Color
                {
                    r = suitData.suitMaterialData.colorValueR,
                    g = suitData.suitMaterialData.colorValueG,
                    b = suitData.suitMaterialData.colorValueB
                };
                Color skinColor = new Color
                {
                    r = suitData.skinMaterialData.colorValueR,
                    g = suitData.skinMaterialData.colorValueG,
                    b = suitData.skinMaterialData.colorValueB
                };

                SetMaterialData(suitColor, suitData.suitMaterialData.metallicValue, suitData.suitMaterialData.smoothnessValue, skinColor, suitData.skinMaterialData.metallicValue, suitData.skinMaterialData.smoothnessValue);

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

[System.Serializable]
public struct BlendshapeValuePair : INetworkSerializable, IEquatable<BlendshapeValuePair>
{
    public float ShapeValue;
    public int ShapeID;

    // Parameterized constructor for ease of use
    public BlendshapeValuePair(float floatValue, int shapeID)
    {
        ShapeValue = floatValue;
        ShapeID = shapeID;
    }

    // Serialization method
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ShapeValue);
        serializer.SerializeValue(ref ShapeID);
    }

    // Implement IEquatable
    public bool Equals(BlendshapeValuePair other)
    {
        return ShapeValue.Equals(other.ShapeValue) && ShapeID.Equals(other.ShapeID);
    }

    public override bool Equals(object obj)
    {
        if (obj is BlendshapeValuePair)
        {
            return Equals((BlendshapeValuePair)obj);
        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + ShapeValue.GetHashCode();
            hash = hash * 23 + ShapeID.GetHashCode();
            return hash;
        }
    }

    // Override the equality operators
    public static bool operator ==(BlendshapeValuePair left, BlendshapeValuePair right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BlendshapeValuePair left, BlendshapeValuePair right)
    {
        return !(left == right);
    }
}


