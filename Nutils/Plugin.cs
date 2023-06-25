using BepInEx;
using Menu;
using Nutils.hook;
using System;
using System.Collections.Generic;
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
    [BepInPlugin("nutils", "Nutils", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        InitializationScreen.InitializationStep currentStep = InitializationScreen.InitializationStep.WRAP_UP;
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        }

        private bool isLoaded = false;

        public static void Log(string m)
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
