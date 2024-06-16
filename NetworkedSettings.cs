using ModelReplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using static IngamePlayerSettings;

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

        public NetworkVariable<ulong> playerID;
        public NetworkVariable<bool> hasInitialized;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            StartCoroutine(waitForIDSync());
        }

        public IEnumerator waitForIDSync()
        {
            yield return new WaitForSeconds(2); //hekken change this asap
            FemmployeeModBase.mls.LogWarning($"the id is {playerID.Value}");
            StartCoroutine(WaitForFemmployeeComponent(playerID.Value));
        }

        [ClientRpc]
        public void AssignValuesClientRpc(ulong _playerID)
        {
            StartCoroutine(WaitForFemmployeeComponent(_playerID));
        }

        private IEnumerator WaitForFemmployeeComponent(ulong _playerID)
        {
            yield return new WaitUntil(() => StartOfRound.Instance.allPlayerScripts[_playerID].GetComponent<Femmployee>() != null);
            AssignValuesToComponents(_playerID);
        }

        public void AssignValuesToComponents(ulong _playerID)
        {
            settings = StartOfRound.Instance.allPlayerScripts[_playerID].GetComponent<Femmployee>().settings;
            settings.networkedSettings = this;
            name = name + " || Player: " + _playerID;
            FemmployeeModBase.mls.LogWarning($"ran AssignValuesToComponents with the id {_playerID} and changed the name to {name}");
            if (Tools.CheckIsServer())
            {
                headSync = settings.bodyRegionMeshRenderers[0].sharedMesh.name;
                chestSync = settings.bodyRegionMeshRenderers[1].sharedMesh.name;
                armsSync = settings.bodyRegionMeshRenderers[2].sharedMesh.name;
                waistSync = settings.bodyRegionMeshRenderers[3].sharedMesh.name;
                legSync = settings.bodyRegionMeshRenderers[4].sharedMesh.name;
            }

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
            if (IsServer) { ApplySettingsClientRpc(); }
            else { ApplySettingsServerRpc(); }
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

        public void SelfDestruct()
        {
            this.GetComponent<NetworkObject>().Despawn(true);
        }

    }
}
