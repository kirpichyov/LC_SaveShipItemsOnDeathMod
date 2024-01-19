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
        
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        public static void Test()
        {
            if (UnityInput.Current.GetKey(KeyCode.Home) && DateTime.UtcNow.Second % 2 == 0)
            {
                ModLogger.Instance.LogInfo("Debug update called");
                SaveShipItemsOnDeathModNetworkManager.Instance.ShowSaveItemsNotificationClientRpc("Test", "It's me - Mario!");
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
                var penaltyResult = PenaltyApplier.Apply();

                if (penaltyResult.IsError)
                {
                    ModLogger.Instance.LogError("Error returned in penalty result.");
                    return;
                }

                __instance.allPlayersDead = true;
                ModVariables.Instance.IsAllPlayersDeadOverride = false;
                ModLogger.Instance.LogInfo($"Post DespawnPropsAtEndOfRound, set allPlayersDead={StartOfRound.Instance.allPlayersDead}");
                
                if (penaltyResult.TotalItemsCount == 0)
                {
                    return;
                }

                var title = "KIRPICHYOV IND. MESSAGE";
                var message = "Kirpichyov Ind. saved your items but have taken fees. " +
                              "Scrap prices were cut in a half. " +
                              $"Total was {penaltyResult.TotalCostInitial}, now {penaltyResult.TotalCostCurrent}";
                
                HUDManager.Instance.AddTextToChatOnServer($"[Notification] {message}");
                
                SaveShipItemsOnDeathModNetworkManager.Instance.ApplyItemsPenaltyClientRpc(
                    serverTotalCurrentCost: penaltyResult.TotalCostCurrent,
                    serverTotalItemsCount: penaltyResult.TotalItemsCount,
                    serverTotalInitialCost: penaltyResult.TotalCostInitial);

                SaveShipItemsOnDeathModNetworkManager.Instance.ShowSaveItemsNotificationClientRpc(title, message);
            }
        }
    }
}