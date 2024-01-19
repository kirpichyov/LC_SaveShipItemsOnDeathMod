using System.Linq;
using SaveShipItemsOnDeathMod.Models;

namespace SaveShipItemsOnDeathMod
{
    public static class PenaltyApplier
    {
        public static PenaltyApplyResult Apply()
        {
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
                    
                item.SetScrapValue(initialScrapValue == 1 ? 1 : initialScrapValue / 2);
                newScrapValueTotal += item.scrapValue;
                ModLogger.Instance.LogInfo($"Scrap value was {initialScrapValue}, now {item.scrapValue} for '{item.name}'");
            }

            ModLogger.Instance.LogInfo("Value of scrap on the ship was cut in half due to crew death. " +
                                       $"Was {initialScrapValueTotal}, now {newScrapValueTotal}");
            
            return new PenaltyApplyResult()
            {
                TotalItemsCount = itemsToApplyPenalty.Length,
                TotalCostInitial = initialScrapValueTotal,
                TotalCostCurrent = newScrapValueTotal,
            };
        }
    }
}