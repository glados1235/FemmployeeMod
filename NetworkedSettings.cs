using BepInEx;
using FemmployeeMod.UIScripts;
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

                NetworkMaterialProperties initalSuitMaterialValues = new NetworkMaterialProperties();
                initalSuitMaterialValues.colorValue = FemmployeeConfigUI.instance.suitMaterialSettings.defaultColorValue;
                initalSuitMaterialValues.metallicValue = FemmployeeConfigUI.instance.suitMaterialSettings.defaultMetallicValue;
                initalSuitMaterialValues.smoothnessValue = FemmployeeConfigUI.instance.suitMaterialSettings.defaultSmoothnessValue;
                suitMaterialValues.Value = initalSuitMaterialValues;

                NetworkMaterialProperties initalSkinMaterialValues = new NetworkMaterialProperties();
                initalSkinMaterialValues.colorValue = FemmployeeConfigUI.instance.skinMaterialSettings.defaultColorValue;
                initalSkinMaterialValues.metallicValue = FemmployeeConfigUI.instance.skinMaterialSettings.defaultMetallicValue;
                initalSkinMaterialValues.smoothnessValue = FemmployeeConfigUI.instance.skinMaterialSettings.defaultSmoothnessValue;
                skinMaterialValues.Value = initalSkinMaterialValues;
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
            else { FemmployeeModBase.mls.LogWarning("Suit data file not found! A new file will be generated next time you apply suit settings."); }

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

        public void SetBlendshapeValue(float value, BlendshapeData[] blendshapes)
        {
            // Separate lists, initialized with sizes matching the blendshapes for each specific region
            List<BlendshapeValuePair> headBlendshapeValuePairs = new List<BlendshapeValuePair>();
            List<BlendshapeValuePair> chestBlendshapeValuePairs = new List<BlendshapeValuePair>();
            List<BlendshapeValuePair> armsBlendshapeValuePairs = new List<BlendshapeValuePair>();
            List<BlendshapeValuePair> waistBlendshapeValuePairs = new List<BlendshapeValuePair>();
            List<BlendshapeValuePair> legsBlendshapeValuePairs = new List<BlendshapeValuePair>();

            for (int i = 0; i < blendshapes.Length; i++)
            {
                BlendshapeData blendshapeData = blendshapes[i];
                int originalRegionID = blendshapeData.OriginalRegionID;

                // Get the shape ID based on the blendshape name within the correct original region
                int shapeId = settings.bodyRegionMeshRenderers[originalRegionID].sharedMesh.GetBlendShapeIndex(blendshapeData.BlendshapeName);

                if (shapeId != -1)
                {
                    // Create a new BlendshapeValuePair with the shape ID and value
                    BlendshapeValuePair valuePair = new BlendshapeValuePair(value, shapeId);

                    // Add the value to the correct region's blendshape list
                    switch (originalRegionID)
                    {
                        case 0:
                            headBlendshapeValuePairs.Add(valuePair);
                            break;
                        case 1:
                            chestBlendshapeValuePairs.Add(valuePair);
                            break;
                        case 2:
                            armsBlendshapeValuePairs.Add(valuePair);
                            break;
                        case 3:
                            waistBlendshapeValuePairs.Add(valuePair);
                            break;
                        case 4:
                            legsBlendshapeValuePairs.Add(valuePair);
                            break;
                        default:
                            FemmployeeModBase.mls.LogWarning("Invalid region ID");
                            break;
                    }

                    if (Tools.CheckIsServer())
                    {
                        void AddValuesToList(NetworkList<BlendshapeValuePair> settingsList, List<BlendshapeValuePair> blendshapeValues)
                        {
                            // Iterate through new blendshapes values to merge or append
                            foreach (var valuePair in blendshapeValues)
                            {
                                bool shapeExists = false;

                                for (int j = 0; j < settingsList.Count; j++)
                                {
                                    if (settingsList[j].ShapeID == valuePair.ShapeID)
                                    {
                                        // If shape exists, overwrite its value
                                        settingsList[j] = valuePair;
                                        shapeExists = true;
                                        break;
                                    }
                                }

                                // If shape does not exist, add the new value pair
                                if (!shapeExists)
                                {
                                    settingsList.Add(valuePair);
                                }
                            }
                        }

                        // Update values for the specific region's networked list
                        switch (originalRegionID)
                        {
                            case 0:
                                AddValuesToList(headBlendshapeValues, headBlendshapeValuePairs);
                                break;

                            case 1:
                                AddValuesToList(chestBlendshapeValues, chestBlendshapeValuePairs);
                                break;

                            case 2:
                                AddValuesToList(armsBlendshapeValues, armsBlendshapeValuePairs);
                                break;

                            case 3:
                                AddValuesToList(waistBlendshapeValues, waistBlendshapeValuePairs);
                                break;

                            case 4:
                                AddValuesToList(legsBlendshapeValues, legsBlendshapeValuePairs);
                                break;
                        }
                    }
                    else
                    {
                        // Send the blendshape data to the server, using only the relevant region's values
                        (float[] shapeValues, int[] shapeIDs) = ConvertBlendshapeListToArrays(originalRegionID switch
                        {
                            0 => headBlendshapeValuePairs,
                            1 => chestBlendshapeValuePairs,
                            2 => armsBlendshapeValuePairs,
                            3 => waistBlendshapeValuePairs,
                            4 => legsBlendshapeValuePairs,
                            _ => new List<BlendshapeValuePair>()
                        });

                        SetBlendshapeNetworkVarServerRpc(originalRegionID, shapeValues, shapeIDs);
                    }
                }
            }
        }

        private (float[] shapeValues, int[] shapeIDs) ConvertBlendshapeListToArrays(List<BlendshapeValuePair> blendshapeList)
        {
            int count = blendshapeList.Count;
            float[] shapeValues = new float[count];
            int[] shapeIDs = new int[count];

            for (int i = 0; i < count; i++)
            {
                shapeValues[i] = blendshapeList[i].ShapeValue;
                shapeIDs[i] = blendshapeList[i].ShapeID;
            }

            return (shapeValues, shapeIDs);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetBlendshapeNetworkVarServerRpc(int id, float[] shapeValues, int[] shapeIDs)
        {
            void AddValuesToList(NetworkList<BlendshapeValuePair> list, float[] values, int[] ids)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    bool shapeExists = false;

                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].ShapeID == ids[i])
                        {
                            // If shape exists, overwrite its value
                            list[j] = new BlendshapeValuePair(values[i], ids[i]);
                            shapeExists = true;
                            break;
                        } 
                    }

                    // If shape does not exist, add the new value pair
                    if (!shapeExists)
                    {
                        list.Add(new BlendshapeValuePair(values[i], ids[i]));
                    }
                }
            }

            // Update values for the specific region's networked list
            switch (id)
            {
                case 0:
                    AddValuesToList(headBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 1:
                    AddValuesToList(chestBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 2:
                    AddValuesToList(armsBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 3:
                    AddValuesToList(waistBlendshapeValues, shapeValues, shapeIDs);
                    break;

                case 4:
                    AddValuesToList(legsBlendshapeValues, shapeValues, shapeIDs);
                    break;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetBlendshapeNetworkVarServerRPC(int TargetList)
        {
            ResetBlendshapeNetworkVar(TargetList);
        }

        public void ResetBlendshapeNetworkVar(int targetList)
        {
            // Ensure settings and networkedSettings are not null
            if (settings?.networkedSettings == null)
            {
                Debug.LogError("Settings or networkedSettings are null");
                return;
            }

            // Create a list of NetworkList<BlendshapeValuePair> from various blendshape values in networkedSettings
            List<NetworkList<BlendshapeValuePair>> blendshapeLists = new List<NetworkList<BlendshapeValuePair>>
            {
                settings.networkedSettings.headBlendshapeValues,
                settings.networkedSettings.chestBlendshapeValues,
                settings.networkedSettings.armsBlendshapeValues,
                settings.networkedSettings.waistBlendshapeValues,
                settings.networkedSettings.legsBlendshapeValues
            };

            // Ensure the targetList index is within bounds
            if (targetList < 0 || targetList >= blendshapeLists.Count)
            {
                Debug.LogError($"Invalid targetList index: {targetList}");
                return;
            }

            // Clear the list located at the index of targetList
            blendshapeLists[targetList].Clear();
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
            List<BlendshapeSlider>[] regionSliderLists = new List<BlendshapeSlider>[5];

            for (int i = 0; i < 5; i++)
            {
                regionSliderLists[i] = new List<BlendshapeSlider>();

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


