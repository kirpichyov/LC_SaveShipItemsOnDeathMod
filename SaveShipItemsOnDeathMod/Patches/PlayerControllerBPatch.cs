using GameNetcodeStuff;
using HarmonyLib;
using MyMod;

namespace SaveShipItemsOnDeathMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "LateUpdate")]
        [HarmonyPostfix]
        public static void FrozeAllPlayersDead(StartOfRound __instance)
        {
            // TODO: Try to make loot in ship cost as half instead of penalty
            if (__instance.allPlayersDead)
            {
                __instance.allPlayersDead = false;
                ModBase.Log.LogInfo($"Was True, set to {__instance.allPlayersDead}");
                
                var terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                var initialCredits = terminal.groupCredits;
                terminal.groupCredits = 0;
                ModBase.Log.LogInfo($"Credits reset. Was {initialCredits}");
                HUDManager.Instance.DisplayTip("WARNING", "Credits were withdrawn due to crew death");
            }
        }
    }
}