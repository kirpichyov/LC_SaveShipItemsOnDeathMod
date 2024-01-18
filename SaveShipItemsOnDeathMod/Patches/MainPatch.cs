using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;

namespace SaveShipItemsOnDeathMod.Patches
{
    // TODO: Review/clean-up logs
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MainPatch
    {
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.FillEndGameStats))]
        [HarmonyPostfix]
        public static void PostFillEndGameStatsHook(HUDManager __instance)
        {
            ModLogger.Instance.LogInfo("Disabling allPlayersDead overlay");
            __instance.statsUIElements.allPlayersDeadOverlay.enabled = false;
        }
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPrefix]
        public static void PreOnDespawnItemsHook()
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                // TODO: Remove log
                ModLogger.Instance.LogInfo("Skip PreOnDespawnItemsHook patch logic since self is not server");
                return;
            }
            
            if (TimeOfDay.Instance.daysUntilDeadline == 0)
            {
                ModLogger.Instance.LogInfo("Ignore patch logic. Days until deadline = 0.");
                return;
            }
            
            if (StartOfRound.Instance.allPlayersDead)
            {
                StartOfRound.Instance.allPlayersDead = false;
                ModLogger.Instance.LogInfo($"Pre DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
                ModVariables.IsAllPlayersDeadOverride = true;
            }
        }
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPostfix]
        public static void PostOnDespawnItemsHook(StartOfRound __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                // TODO: Remove log
                ModLogger.Instance.LogInfo("Skip PostOnDespawnItemsHook patch logic since self is not server");
                return;
            }
            
            if (ModVariables.IsAllPlayersDeadOverride)
            {
                var allItemsOnLevel = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
                if (allItemsOnLevel == null)
                {
                    ModLogger.Instance.LogError($"'{nameof(allItemsOnLevel)}' is null");
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
                    ModLogger.Instance.LogInfo("No items to apply price penalty to");
                    return;
                }

                ModLogger.Instance.LogInfo($"Found {itemsToApplyPenalty.Length} to apply price penalty to");

                var initialScrapValueTotal = 0;
                var newScrapValueTotal = 0;

                foreach (var item in itemsToApplyPenalty)
                {
                    var initialScrapValue = item.scrapValue;
                    initialScrapValueTotal += initialScrapValue;

                    if (item.scrapValue == 0)
                    {
                        ModLogger.Instance.LogInfo($"Initial scrap value for {item.name} is 0. Skipping it");
                        continue;
                    }
                    
                    item.SetScrapValue(initialScrapValue == 1 ? 1 : initialScrapValue / 2);
                    newScrapValueTotal += item.scrapValue;
                    ModLogger.Instance.LogInfo($"Scrap value was {initialScrapValue}, now {item.scrapValue} for '{item.name}'");
                }

                ModLogger.Instance.LogInfo("Value of scrap on the ship was cut in half due to crew death. " +
                                    $"Was {initialScrapValueTotal}, now {newScrapValueTotal}");

                __instance.allPlayersDead = true;
                ModVariables.IsAllPlayersDeadOverride = false;
                ModLogger.Instance.LogInfo($"Post DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");

                var message = "Kirpichyov Ind. saved your items but have taken fees. " +
                              "Scrap prices were cut in a half. " +
                              $"Total was {initialScrapValueTotal}, now {newScrapValueTotal}";
                
                HUDManager.Instance.AddTextToChatOnServer($"[Notification] {message}");
                
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