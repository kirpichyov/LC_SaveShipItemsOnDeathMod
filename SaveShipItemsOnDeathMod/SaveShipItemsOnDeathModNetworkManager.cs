using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SaveShipItemsOnDeathMod
{
    internal class SaveShipItemsOnDeathModNetworkManager : NetworkBehaviour
    {
        public static SaveShipItemsOnDeathModNetworkManager Instance { get; private set; }

        public NetworkVariable<int> ScrapFee_Percent = new NetworkVariable<int>(50);
        
        private void Awake()
        {
            Instance = this;

            if (GameNetworkManager.Instance.isHostingGame)
            {
                ScrapFee_Percent.Value = Mathf.Clamp(ModConfig.ScrapFee_Percent.Value, 0, 100);
                ModLogger.Instance.LogInfo("HOST SENDING CONFIG TO CLIENTS");
            }
            
            ModLogger.Instance.LogInfo("ModNetworkManager Awake");
        }
        
        [ClientRpc]
        public void ApplyItemsPenaltyClientRpc(
            int serverTotalItemsCount,
            int serverTotalInitialCost,
            int serverTotalCurrentCost,
            string serverNetworkObjectIds)
        {
            // TODO: Clean-up logs
            ModLogger.Instance.LogInfo("Received rpc for ApplyItemsPenaltyClientRpc");
            ModLogger.Instance.LogInfo("[Control data from server]");
            ModLogger.Instance.LogInfo($"IsServer={IsServer};IsHost={IsHost}");
            ModLogger.Instance.LogInfo($"{nameof(serverTotalItemsCount)}={serverTotalItemsCount}");
            ModLogger.Instance.LogInfo($"{nameof(serverTotalInitialCost)}={serverTotalInitialCost}");
            ModLogger.Instance.LogInfo($"{nameof(serverTotalCurrentCost)}={serverTotalCurrentCost}");
            ModLogger.Instance.LogInfo($"{nameof(serverNetworkObjectIds)}={serverNetworkObjectIds}");

            // alternative: if (GameNetworkManager.Instance.isHostingGame)
            if (IsServer || IsHost)
            {
                return;
            }
            
            var networkObjectIds = serverNetworkObjectIds
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(ulong.Parse)
                .ToArray();

            var feePercent = Instance.ScrapFee_Percent.Value;
            var penaltyResult = PenaltyApplier.Apply(feePercent, networkObjectIds);
            ModLogger.Instance.LogInfo($"Client finished apply penalty. IsError={penaltyResult.IsError}");
            ModLogger.Instance.LogInfo($"[Client data]");
            ModLogger.Instance.LogInfo($"{nameof(penaltyResult.TotalItemsCount)}={penaltyResult.TotalItemsCount}");
            ModLogger.Instance.LogInfo($"{nameof(penaltyResult.TotalCostInitial)}={penaltyResult.TotalCostInitial}");
            ModLogger.Instance.LogInfo($"{nameof(penaltyResult.TotalCostCurrent)}={penaltyResult.TotalCostCurrent}");
        }
        
        [ClientRpc]
        public void ShowItemsSavedNotificationOnReviveClientRpc(string title, string message)
        {
            ModLogger.Instance.LogInfo("Received rpc for ShowItemsSavedNotificationOnReviveClientRpc");
            
            ModVariables.Instance.ShouldShowSavedItemsNotification = true;
            ModVariables.Instance.SavedItemsTitle = title;
            ModVariables.Instance.SavedItemsMessage = message;
        }
    }
}