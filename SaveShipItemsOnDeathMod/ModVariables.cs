using UnityEngine;

namespace SaveShipItemsOnDeathMod
{
    internal class ModVariables
    {
        public static ModVariables Instance { get; private set; }
        
        public bool IsAllPlayersDeadOverride { get; set; }
        public GameObject ModNetworkManagerGameObject { get; set; }
        public bool ShouldShowSavedItemsNotification { get; set; }
        public string SavedItemsTitle { get; set; }
        public string SavedItemsMessage { get; set; }
        
        public static bool TrySetInstance(ModVariables instance)
        {
            if (Instance != null)
            {
                return false;
            }

            Instance = instance;
            return true;
        }
    }
}