using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace MyMod
{
    
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class ModBase : BaseUnityPlugin
    {
        private const string ModGuid = "Kirpichyov.SaveShipItemsOnDeath";
        private const string ModName = "Kirpichyov's SaveShipItemsOnDeath";
        private const string ModVersion = "1.0.0.1";

        private readonly Harmony _harmony = new Harmony(ModGuid);
        internal static ManualLogSource Log;
    
        public static ModBase Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
        
            Log.LogInfo("MyMod loaded.");
        
            new Harmony(ModGuid).PatchAll(Assembly.GetExecutingAssembly());
            _harmony.PatchAll(typeof(ModBase));
            _harmony.PatchAll(typeof(PlayerControllerB));
        }
    }
}