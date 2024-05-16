using ModelReplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
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

        public ulong playerID;


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
            StartOfRound.Instance.allPlayerScripts[playerID].GetComponent<Femmployee>().ApplySwapRegions();
        }

        [ClientRpc]
        public void SetNetworkedSettingsClientRpc(ulong _playerID)
        {
            StartCoroutine(WaitForFemmployeeComponent(_playerID));
        }

        private IEnumerator WaitForFemmployeeComponent(ulong _playerID)
        {
            yield return new WaitUntil(() => StartOfRound.Instance.allPlayerScripts[_playerID].GetComponent<Femmployee>() != null);
            AssignValuesToComponents(_playerID);
        }

        private void AssignValuesToComponents(ulong _playerID)
        {
            settings = StartOfRound.Instance.allPlayerScripts[_playerID].GetComponent<Femmployee>().settings;
            settings.networkedSettings = this;
            playerID = _playerID;
        }
    }
}
