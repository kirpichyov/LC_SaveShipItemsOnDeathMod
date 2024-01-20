using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

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
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            
            Instance = this;
            ModLogger.TrySetInstance(Logger);
            ModVariables.TrySetInstance(new ModVariables());
        
            var assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod");
            var bundle = AssetBundle.LoadFromFile(assetDir);

            var netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/NetcodeModding/SaveShipItemsOnDeathNetworkManager.prefab");
            netManagerPrefab.AddComponent<SaveShipItemsOnDeathModNetworkManager>();
            
            ModVariables.Instance.ModNetworkManagerGameObject = netManagerPrefab;
            
            ModConfig.Init();
            
            _harmony.PatchAll();
            ModLogger.Instance.LogInfo($"{ModName} loaded.");
        }
        
        public void BindConfig<T>(ref ConfigEntry<T> config, string section, string key, T defaultValue, string description = "")
        {
            config = Config.Bind(section, key, defaultValue, description);
        }
    }
}