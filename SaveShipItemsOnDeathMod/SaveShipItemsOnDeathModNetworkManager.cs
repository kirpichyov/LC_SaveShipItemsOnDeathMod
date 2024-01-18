using Unity.Netcode;
using UnityEngine;

namespace SaveShipItemsOnDeathMod
{
    internal class SaveShipItemsOnDeathModNetworkManager : NetworkBehaviour
    {
        public static SaveShipItemsOnDeathModNetworkManager Instance { get; private set; }

        private void Awake()
        {
            SaveShipItemsOnDeathModNetworkManager.Instance = this;

            // if (GameNetworkManager.Instance.isHostingGame)
            // {
            //     // DO HOST SENDING DATA (e.x. CONFIG) TO CLIENTS HERE
            // }
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