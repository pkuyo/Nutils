using BepInEx;
using Menu;
using Nutils.hook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
namespace Nutils
{
    [BepInPlugin("nutils", "Nutils", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        }

        private bool isLoaded = false;

        public static void Log(object m)
        {
            Debug.Log("[Nutils] " + m);
        }
        public static void LogError(object m)
        {
            Debug.Log("[Nutils] " + m);
        }
        public static void Log(string f, params object[] args)
        {
            Debug.Log("[Nutils] " + string.Format(f, args));
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            try
            {
                orig(self);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                Debug.LogException(e);
            }

            try
            {
                if (!isLoaded)
                {
                    PlayerBaseHook.OnModsInit();
                    DeathSaveDataHook.OnModsInit();
                    MiscSaveDataHook.OnModsInit();

                    AssetBundle bundle =
                        AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/nutilsasset"));

                    foreach (var shader in bundle.LoadAllAssets<Shader>())
                    {
                        var name = Path.GetFileName(shader.name);
                        self.Shaders.Add($"Nutils.{name}",
                            FShader.CreateShader($"Nutils.{name}", shader));
                        Plugin.Log($"Load shader: {name}");
                    }

                    Logger.LogInfo("Nutils Inited");
                    isLoaded = true;
                }

            }
            catch(Exception e)
            {
                Logger.LogError(e);
                Debug.LogException(e);
            }
        }
    }
}
