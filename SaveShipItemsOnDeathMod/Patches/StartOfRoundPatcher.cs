using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace SaveShipItemsOnDeathMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void SpawnNetManager(StartOfRound __instance)
        {
            if(__instance.IsHost)
            {
                var gameObject = Object.Instantiate(ModVariables.Instance.ModNetworkManagerGameObject);
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}