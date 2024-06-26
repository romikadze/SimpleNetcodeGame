using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Source.Scripts.RelayNetwork
{
    public class RelayConnection : MonoBehaviour
    {
        public Action OnStartHost;
        public Action OnStartClient;

        private void Start()
        {
            Initialize();
        }

        private async void Initialize()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task<string> StartHostWithRelay(int maxConnections = 1)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(new RelayServerData(allocation, "dtls"));
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            if (NetworkManager.Singleton.StartHost())
            {
                OnStartHost?.Invoke();
                return joinCode;
            }

            return null;
        }

        public async Task<bool> StartClientWithRelay(string joinCode)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            if (!string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient())
            {
                OnStartClient?.Invoke();
                return true;
            }

            return false;
        }
    }
}