using HarmonyLib;
using Unity.Netcode;

namespace SaveShipItemsOnDeathMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void AddToPrefabs(ref GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(ModVariables.Instance.ModNetworkManagerGameObject);
        }
    }
}