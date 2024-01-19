namespace SaveShipItemsOnDeathMod
{
    public static class DebugUtils
    {
        public static void LogNullOrNot(string varName, object value)
        {
            ModLogger.Instance.LogInfo($"Variable {varName} null = {value is null}");
        }
    }
}