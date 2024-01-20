using System;
using System.Linq;
using SaveShipItemsOnDeathMod.Models;
using UnityEngine;

namespace SaveShipItemsOnDeathMod
{
    public static class PenaltyApplier
    {
        public static PenaltyApplyResult Apply(int feePercent)
        {
            ModLogger.Instance.LogInfo("Fee percent to apply should be " + feePercent);

            var allItemsOnLevel = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            if (allItemsOnLevel == null)
            {
                ModLogger.Instance.LogError($"'{nameof(allItemsOnLevel)}' is null");

                return new PenaltyApplyResult()
                {
                    TotalItemsCount = 0,
                    TotalCostInitial = 0,
                    TotalCostCurrent = 0,
                    IsError = true,
                };
            }

            var itemsToApplyPenalty = allItemsOnLevel
                .Where(item => item.isInShipRoom &&
                               item.grabbable &&
                               item.itemProperties.isScrap &&
                               !item.deactivated)
                .ToArray();
                
            if (itemsToApplyPenalty.Length == 0)
            {
                ModLogger.Instance.LogInfo("No items to apply price penalty to");
                
                return new PenaltyApplyResult()
                {
                    TotalItemsCount = 0,
                    TotalCostInitial = 0,
                    TotalCostCurrent = 0,
                };
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
                
                var newScrapValue = CalcNewScarpValue(item.scrapValue, feePercent);
                
                item.SetScrapValue(newScrapValue);
                newScrapValueTotal += item.scrapValue;
                
                ModLogger.Instance.LogInfo($"Scrap value was {initialScrapValue}, now {item.scrapValue} for '{item.name}' | networkId: {item.NetworkObjectId}");
            }

            ModLogger.Instance.LogInfo($"Value of scrap on the ship was cut by {feePercent}% due to crew death. " +
                                       $"Was {initialScrapValueTotal}, now {newScrapValueTotal}");
            
            return new PenaltyApplyResult()
            {
                TotalItemsCount = itemsToApplyPenalty.Length,
                TotalCostInitial = initialScrapValueTotal,
                TotalCostCurrent = newScrapValueTotal,
            };
        }

        private static int CalcNewScarpValue(int originalValue, int percentToApply)
        {
            if (percentToApply == 0)
            {
                return originalValue;
            }
            
            if (originalValue == 1 || percentToApply == 100)
            {
                return 1;
            }

            var percentFloat = percentToApply / 100f;
            var feeAmount = Mathf.Clamp(originalValue * percentFloat, 1, originalValue);
            var feeAmountRounded = Mathf.RoundToInt(feeAmount);
            var newAmount = Math.Clamp(originalValue - feeAmountRounded, 1, originalValue);

            return newAmount;
        }
    }
}