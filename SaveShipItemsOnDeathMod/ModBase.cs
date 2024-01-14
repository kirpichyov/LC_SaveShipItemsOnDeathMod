using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SaveShipItemsOnDeathMod
{
    
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class ModBase : BaseUnityPlugin
    {
        private const string ModGuid = "Kirpichyov.SaveShipItemsOnDeath";
        private const string ModName = "Kirpichyov's SaveShipItemsOnDeath";
        private const string ModVersion = "1.0.0.3";

        private readonly Harmony _harmony = new Harmony(ModGuid);
        internal static ManualLogSource Log;

        internal static bool IsAppExiting = false;
        internal static bool IsAllPlayersDeadOverride = false;
    
        public static ModBase Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
        
            Log.LogInfo($"{ModName} loaded.");
        
            new Harmony(ModGuid).PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnApplicationQuit()
        {
            IsAppExiting = true;
        }
    }
}