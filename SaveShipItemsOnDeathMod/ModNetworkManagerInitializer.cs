using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveShipItemsOnDeathMod
{
    internal static class ModNetworkManagerInitializer
    {
        private static readonly string[] ExcludedScenes =
        {
            "MainMenu",
            "InitScene",
            "InitSceneLaunchOptions"
        };
        
        public static void ClientConnectInitializer(Scene scene, LoadSceneMode sceneEnum)
        {
            ModLogger.Instance.LogInfo("sn " + scene.name);
            ModLogger.Instance.LogInfo("sne " + sceneEnum);
            
            if (scene == null || ExcludedScenes.Contains(scene.name, StringComparer.InvariantCultureIgnoreCase))
            {
                ModLogger.Instance.LogInfo("Scene is null or exclude scene.");
                return;
            }
            
            ModLogger.Instance.LogInfo($"LOL H={NetworkManager.Singleton.IsHost}");
            ModLogger.Instance.LogInfo($"LOL S={NetworkManager.Singleton.IsServer}");
            ModLogger.Instance.LogInfo($"LOL L={NetworkManager.Singleton.IsListening}");

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
            {
                ModLogger.Instance.LogInfo("Network manager self is null or not host.");
                return;
            }
            
            var gameObject = new GameObject("ModNetworkManager");
            gameObject.AddComponent<NetworkObject>().Spawn();
            gameObject.AddComponent<SaveShipItemsOnDeathModNetworkManager>();
            
            ModLogger.Instance.LogInfo("Initialized SaveShipItemsOnDeathModNetworkManager");
        }
    }
}