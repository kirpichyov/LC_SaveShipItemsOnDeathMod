using GameNetcodeStuff;
using HarmonyLib;

namespace MyMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        //[HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        [HarmonyPatch("LateUpdate")]
        [HarmonyPostfix]
        public static void InfiniteSprintPatchLate(ref float ___sprintMeter)
        {
            TutorialModBase.Log.LogInfo($"[Pre-Late] Current sprintMeter={___sprintMeter}");
            ___sprintMeter = 1f;
            TutorialModBase.Log.LogInfo($"[Post-Late] Current sprintMeter={___sprintMeter}");
        }
    
        [HarmonyPatch("Update")]
        //[HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void InfiniteSprintPatch(ref float ___sprintMeter)
        {
            TutorialModBase.Log.LogInfo($"[Pre] Current sprintMeter={___sprintMeter}");
            ___sprintMeter = 1f;
            TutorialModBase.Log.LogInfo($"[Post] Current sprintMeter={___sprintMeter}");
        }
    }
}