using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;

namespace SaveShipItemsOnDeathMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MainPatch
    {
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPrefix]
        public static void PreOnDespawnItemsHook()
        {
            if (ModBase.IsAppExiting)
            {
                ModBase.Log.LogInfo("Ignore patch logic. App exiting.");
                return;
            }

            if (TimeOfDay.Instance.daysUntilDeadline == 0)
            {
                ModBase.Log.LogInfo("Ignore patch logic. Days until deadline = 0.");
                return;
            }
            
            if (StartOfRound.Instance.allPlayersDead)
            {
                StartOfRound.Instance.allPlayersDead = false;
                ModBase.Log.LogInfo($"Pre DespawnPropsAtEndOfRound() called, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
                ModBase.IsAllPlayersDeadOverride = true;
            }
        }
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPostfix]
        public static void PostOnDespawnItemsHook(StartOfRound __instance)
        {
            if (ModBase.IsAppExiting)
            {
                ModBase.Log.LogInfo("Ignore patch logic. App exiting.");
                return;
            }
            
            if (ModBase.IsAllPlayersDeadOverride)
            {
                var allItemsOnLevel = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
                if (allItemsOnLevel == null)
                {
                    ModBase.Log.LogError($"'{nameof(allItemsOnLevel)}' is null");
                    return;
                }

                var itemsToApplyPenalty = allItemsOnLevel
                    .Where(item => item.isInShipRoom &&
                                   item.grabbable &&
                                   item.itemProperties.isScrap &&
                                   !item.deactivated &&
                                   !item.isHeld).ToArray();
                
                if (itemsToApplyPenalty.Length == 0)
                {
                    ModBase.Log.LogInfo("No items to apply price penalty to");
                    return;
                }

                ModBase.Log.LogInfo($"Found {itemsToApplyPenalty.Length} to apply price penalty to");

                var initialScrapValueTotal = 0;
                var newScrapValueTotal = 0;

                foreach (var item in itemsToApplyPenalty)
                {
                    var initialScrapValue = item.scrapValue;
                    initialScrapValueTotal += initialScrapValue;
                    item.SetScrapValue(initialScrapValue == 1 ? 1 : initialScrapValue / 2);
                    newScrapValueTotal += item.scrapValue;
                    ModBase.Log.LogInfo($"Scrap value was {initialScrapValue}, now {item.scrapValue} for '{item.name}'");
                }

                ModBase.Log.LogInfo("Value of scrap on the ship was cut in half due to crew death. " +
                                    $"Was {initialScrapValueTotal}, now {newScrapValueTotal}");

                __instance.allPlayersDead = true;
                ModBase.IsAllPlayersDeadOverride = false;
                ModBase.Log.LogInfo($"Post DespawnPropsAtEndOfRound() called, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");

                var message = "Kirpichyov Ind. saved your items but have taken fees. " +
                              "Scrap prices were cut in a half. " +
                              $"Total was {initialScrapValueTotal}, now {newScrapValueTotal}";
                
                HUDManager.Instance
                    .AddTextToChatOnServer($"[Notification] {message}");
                
                HUDManager.Instance.ReadDialogue(new[]
                {
                    new DialogueSegment()
                    {
                        bodyText = message,
                        speakerText = "KIRPICHYOV IND. MESSAGE",
                    }
                });
            }
        }
    }
}