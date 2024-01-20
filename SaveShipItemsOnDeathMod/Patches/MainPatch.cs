using System;
using System.Linq;
using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

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

        [HarmonyPatch(typeof(StartOfRound), "AllPlayersHaveRevivedClientRpc")]
        [HarmonyPostfix]
        public static void ShowSavedItemsNotificationOnPurpose()
        {
            ModLogger.Instance.LogInfo("StartOfRound.AllPlayersHaveRevivedClientRpc patch");
            ModLogger.Instance.LogInfo($"ShouldShowSavedItemsNotification? {ModVariables.Instance.ShouldShowSavedItemsNotification}");
            
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
                ModLogger.Instance.LogInfo("Ignore patch logic. Days until deadline = 0.");
                return;
            }
            
            if (StartOfRound.Instance.allPlayersDead)
            {
                StartOfRound.Instance.allPlayersDead = false;
                ModLogger.Instance.LogInfo($"Pre DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
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
                ModLogger.Instance.LogInfo($"Post DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
                
                // TODO: Implement here check, if server count not 0 but client,
                // then take [network object ids|alt: names and coordinates] of objects from server (should be also sent via RPC)
                // and try to map it
                // issue is that mod allows to connect after lobby closed, so player connected has items as InShip=false
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