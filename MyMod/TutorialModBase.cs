using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace MyMod
{
    
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class TutorialModBase : BaseUnityPlugin
    {
        private const string ModGuid = "Kirpichyov.MyMod";
        private const string ModName = "Kirpichyov's MyMod";
        private const string ModVersion = "1.0.0.0";

        private readonly Harmony _harmony = new Harmony(ModGuid);
        internal static ManualLogSource Log;
    
        public static TutorialModBase Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
        
            Log.LogInfo("MyMod loaded.");
        
            new Harmony(ModGuid).PatchAll(Assembly.GetExecutingAssembly());
            _harmony.PatchAll(typeof(TutorialModBase));
            _harmony.PatchAll(typeof(PlayerControllerB));
        }
    }
}