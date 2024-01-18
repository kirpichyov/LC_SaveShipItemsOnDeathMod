using BepInEx.Logging;

namespace SaveShipItemsOnDeathMod
{
    internal static class ModLogger
    {
        public static ManualLogSource Instance { get; private set; }

        public static bool TrySetInstance(ManualLogSource instance)
        {
            if (Instance != null)
            {
                return false;
            }

            Instance = instance;
            return true;
        }
    }
}