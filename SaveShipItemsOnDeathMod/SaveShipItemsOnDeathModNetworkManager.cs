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
        public void SendSaveItemsNotificationClientRpc(string title, string message)
        {
            ModLogger.Instance.LogInfo("Received rpc for SendSaveItemsNotificationRpc");
            ModLogger.Instance.LogInfo("RPC is hosting " + GameNetworkManager.Instance.isHostingGame);
            ModLogger.Instance.LogInfo("RPC is server " + IsServer);
            ModLogger.Instance.LogInfo("RPC is client " + IsClient);
            
            // if (GameNetworkManager.Instance.isHostingGame)
            // {
            //     return;
            // }
            
            HUDManager.Instance.ReadDialogue(new[]
            {
                new DialogueSegment()
                {
                    bodyText = message,
                    speakerText = title,
                }
            });
        }
    }
}