namespace SaveShipItemsOnDeathMod.Models
{
    public class PenaltyApplyResult
    {
        public int TotalItemsCount { get; set; }
        public int TotalCostInitial { get; set; }
        public int TotalCostCurrent { get; set; }
        public bool IsError { get; set; }
    }
}