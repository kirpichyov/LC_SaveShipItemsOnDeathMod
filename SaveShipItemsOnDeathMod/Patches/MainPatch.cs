using GameNetcodeStuff;
using HarmonyLib;

namespace SaveShipItemsOnDeathMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MainPatch
    {
        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.FillEndGameStats))]
        [HarmonyPostfix]
        public static void PostFillEndGameStatsHook(HUDManager __instance)
        {
            ModLogger.Instance.LogDebug("Disabling allPlayersDead overlay");
            __instance.statsUIElements.allPlayersDeadOverlay.enabled = false;
        }

        [HarmonyPatch(typeof(StartOfRound), "AllPlayersHaveRevivedClientRpc")]
        [HarmonyPostfix]
        public static void ShowSavedItemsNotificationOnPurpose()
        {
            ModLogger.Instance.LogDebug("StartOfRound.AllPlayersHaveRevivedClientRpc patch");
            ModLogger.Instance.LogDebug($"ShouldShowSavedItemsNotification? {ModVariables.Instance.ShouldShowSavedItemsNotification}");
            
            if (ModVariables.Instance.ShouldShowSavedItemsNotification)
            {
                HUDManager.Instance.DisplayTip(ModVariables.Instance.SavedItemsTitle, ModVariables.Instance.SavedItemsMessage);
                ModVariables.Instance.ShouldShowSavedItemsNotification = false;
                ModVariables.Instance.SavedItemsMessage = string.Empty;
                ModVariables.Instance.SavedItemsTitle = string.Empty;
            }
        }
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPrefix]
        public static void PreOnDespawnItemsHook()
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                return;
            }
            
            if (TimeOfDay.Instance.daysUntilDeadline == 0)
            {
                ModLogger.Instance.LogDebug("Ignore patch logic. Days until deadline = 0.");
                return;
            }
            
            if (StartOfRound.Instance.allPlayersDead)
            {
                StartOfRound.Instance.allPlayersDead = false;
                ModLogger.Instance.LogDebug($"Pre DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
                ModVariables.Instance.IsAllPlayersDeadOverride = true;
            }
        }
        
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPostfix]
        public static void PostOnDespawnItemsHook(StartOfRound __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame)
            {
                return;
            }
            
            if (ModVariables.Instance.IsAllPlayersDeadOverride)
            {
                var feePercent = SaveShipItemsOnDeathModNetworkManager.Instance.ScrapFee_Percent.Value;
                var penaltyResult = PenaltyApplier.Apply(feePercent);

                if (penaltyResult.IsError)
                {
                    ModLogger.Instance.LogError("Error returned in penalty result.");
                    return;
                }

                __instance.allPlayersDead = true;
                ModVariables.Instance.IsAllPlayersDeadOverride = false;
                ModLogger.Instance.LogDebug($"Post DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
                
                if (penaltyResult.TotalItemsCount == 0)
                {
                    return;
                }

                var title = "KIRPICHYOV IND. MESSAGE";
                var message = "Kirpichyov Ind. saved your items but have taken fees. " +
                              $"Scrap prices were cut by {feePercent}%. " +
                              $"Total was {penaltyResult.TotalCostInitial}, now {penaltyResult.TotalCostCurrent}";
                
                HUDManager.Instance.AddTextToChatOnServer($"[Notification] {message}");

                var updatedNetworkIdsString = string.Join(';', penaltyResult.UpdatedItemsNetworkIds);
                
                SaveShipItemsOnDeathModNetworkManager.Instance.ApplyItemsPenaltyClientRpc(
                    serverTotalCurrentCost: penaltyResult.TotalCostCurrent,
                    serverTotalItemsCount: penaltyResult.TotalItemsCount,
                    serverTotalInitialCost: penaltyResult.TotalCostInitial,
                    serverNetworkObjectIds: updatedNetworkIdsString);
                
                SaveShipItemsOnDeathModNetworkManager.Instance.ShowItemsSavedNotificationOnReviveClientRpc(title, message);
            }
        }
    }
}