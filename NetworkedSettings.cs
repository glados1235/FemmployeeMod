using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
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
            NetworkLog.LogError($"{ _playerID}");


            NetworkLog.LogError($"{StartOfRound.Instance.allPlayerScripts[playerID]}");


            settings = StartOfRound.Instance.allPlayerScripts[playerID].GetComponent<Femmployee>().settings;


            settings.networkedSettings = this;


            playerID = _playerID;
        }
    }
}
