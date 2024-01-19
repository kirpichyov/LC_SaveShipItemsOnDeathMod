using Unity.Netcode;

namespace SaveShipItemsOnDeathMod
{
    internal class SaveShipItemsOnDeathModNetworkManager : NetworkBehaviour
    {
        public static SaveShipItemsOnDeathModNetworkManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            ModLogger.Instance.LogInfo("ModNetworkManager Awake");
        }
        
        [ClientRpc]
        public void ApplyItemsPenaltyClientRpc(
            int serverTotalItemsCount,
            int serverTotalInitialCost,
            int serverTotalCurrentCost)
        {
            // TODO: Clean-up logs
            ModLogger.Instance.LogInfo("Received rpc for ApplyItemsPenaltyClientRpc");
            ModLogger.Instance.LogInfo("[Control data from server]");
            ModLogger.Instance.LogInfo($"IsServer={IsServer};IsHost={IsHost}");
            ModLogger.Instance.LogInfo($"{nameof(serverTotalItemsCount)}={serverTotalItemsCount}");
            ModLogger.Instance.LogInfo($"{nameof(serverTotalInitialCost)}={serverTotalInitialCost}");
            ModLogger.Instance.LogInfo($"{nameof(serverTotalCurrentCost)}={serverTotalCurrentCost}");

            // alternative: if (GameNetworkManager.Instance.isHostingGame)
            if (IsServer || IsHost)
            {
                return;
            }
            
            var penaltyResult = PenaltyApplier.Apply();
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