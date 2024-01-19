using UnityEngine;

namespace SaveShipItemsOnDeathMod
{
    internal class ModVariables
    {
        public static ModVariables Instance { get; private set; }
        
        public bool IsAllPlayersDeadOverride { get; set; }
        public GameObject ModNetworkManagerGameObject { get; set; }
        
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