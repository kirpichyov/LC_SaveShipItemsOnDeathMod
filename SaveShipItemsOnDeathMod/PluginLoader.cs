using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace SaveShipItemsOnDeathMod
{
    
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class PluginLoader : BaseUnityPlugin
    {
        private const string ModGuid = "Kirpichyov.SaveShipItemsOnDeath";
        private const string ModName = "Kirpichyov's SaveShipItemsOnDeath";
        private const string ModVersion = "1.0.1";

        private readonly Harmony _harmony = new Harmony(ModGuid);
    
        public static PluginLoader Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            ModLogger.TrySetInstance(Logger);
        
            ModLogger.Instance.LogInfo($"{ModName} loaded.");
        
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}