using BepInEx.Configuration;

namespace SaveShipItemsOnDeathMod
{
    public static class ModConfig
    {
        private const string ScrapFee_Percent_Description =
            "Percent to take from items cost as penalty. " +
            "Min value is 0, means no fee. Max value is 100, means cost will reduce to 1." +
            "Since the game uses an integer for scrap value the final amount will be rounded.";
        
        public static void Init()
        {
            PluginLoader.Instance.BindConfig(ref ScrapFee_Percent, "Penalty", "Scrap Fee", 50, ScrapFee_Percent_Description);
            ModLogger.Instance.LogInfo($"Value loaded from config Penalty_ScrapFee={ScrapFee_Percent.Value}");
        }
        
        public static ConfigEntry<int> ScrapFee_Percent;
    }
}