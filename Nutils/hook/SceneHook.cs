using BepInEx.Logging;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Nutils.hook
{

    static public class SceneHook
    {
        /// <summary>
        /// 添加入场动画（即黄白猫新开游戏的cg过场）
        /// </summary>
        /// <param name="name">猫猫名字</param>
        /// <param name="music">过场时的背景音乐</param>
        /// <param name="ID">新的过场ID</param>
        /// <param name="buildSlideAction">创建新SlideShow的函数</param>
        static public void AddIntroSlideShow(string name, string music, SlideShow.SlideShowID ID, Action<SlideShow> buildSlideAction)
        {
            OnModsInit();
            SlideShowArg arg;
            arg.name = name;
            arg.music = music;
            arg.id = ID;
            arg.buildSlideAction = buildSlideAction;
            slideIntroArgs.Add(arg);
        }

        /// <summary>
        /// 添加飞升动画
        /// </summary>
        /// <param name="name">猫猫名字</param>
        /// <param name="music">过场时的背景音乐</param>
        /// <param name="ID">新的过场ID</param>
        /// <param name="buildSlideAction">创建新SlideShow的函数</param>
        static public void AddOutroSlideShow(string name, string music, SlideShow.SlideShowID ID, Action<SlideShow> buildSlideAction)
        {
            OnModsInit();
            SlideShowArg arg;
            arg.name = name;
            arg.music = music;
            arg.id = ID;
            arg.buildSlideAction = buildSlideAction;
            slideOutroArgs.Add(arg);
        }

        /// <summary>
        /// 添加单个场景（slide Show是由多个场景依次播放的）
        /// </summary>
        /// <param name="id">过场ID</param>
        /// <param name="action">创建单个scene的函数</param>
        static public void AddScene(MenuScene.SceneID id, Action<MenuScene> action)
        {
            OnModsInit();
            sceneArgs.Add(id, action);
        }

        #region Hook
        static bool loaded = false;
        static SceneHook()
        {
            slideIntroArgs = new List<SlideShowArg>();
            slideOutroArgs = new List<SlideShowArg>();
            sceneArgs = new Dictionary<MenuScene.SceneID, Action<MenuScene>>();
        
        }
        

        static void OnModsInit()
        {
            if (!loaded)
            {
               
                IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGameIL;
                IL.RainWorldGame.ExitToVoidSeaSlideShow += RainWorldGame_ExitToVoidSeaSlideShowIL;
                On.Menu.SlideShow.ctor += SlideShow_ctor;
                On.Menu.SlideShow.NextScene += SlideShow_NextScene;
                On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
                loaded = true;
            }
            
        }


        private static void RainWorldGame_ExitToVoidSeaSlideShowIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After, i => i.MatchLdfld<MainLoopProcess>("manager"),
                                             i => i.MatchLdsfld<SlideShow.SlideShowID>("WhiteOutro")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<SlideShow.SlideShowID, RainWorldGame, SlideShow.SlideShowID>>((id, game) =>
                 {
                     foreach (var arg in slideOutroArgs)
                     {
                         if (arg.name == game.session.characterStats.name.value)
                         {
                             return arg.id;
                         }
                     }
                     return id;
                 });
            }
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig(self);
            if (self.sceneID != null && sceneArgs.ContainsKey(self.sceneID))
                sceneArgs[self.sceneID](self);
        }


        private static void SlideShow_NextScene(On.Menu.SlideShow.orig_NextScene orig, SlideShow self)
        {
            if (self.preloadedScenes.Length == 0)
                return;
            orig(self);
        }


        private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, SlideShow self, ProcessManager manager, SlideShow.SlideShowID slideShowID)
        {
            //处理音乐部分
            try
            {
                foreach (var arg in slideIntroArgs)
                {
                    if (arg.id == slideShowID)
                    {
                        self.waitForMusic = arg.music;
                        self.stall = true;
                        manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                        break;
                    }
                }
                foreach (var arg in slideOutroArgs)
                {
                    if (arg.id == slideShowID)
                    {
                        self.waitForMusic = arg.music;
                        self.stall = true;
                        manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                        break;
                    }
                }
            }
            catch
            {
                Debug.LogError("[Nutils] Yeah slide show has some bugs, but i don't want to fix");
            }

            orig(self, manager, slideShowID);
            try
            {
                self.processAfterSlideShow = ProcessManager.ProcessID.Game;
                foreach (var arg in slideIntroArgs)
                {
                    if (arg.id == slideShowID)
                    {
                        if (arg.buildSlideAction == null)
                            return;
                        arg.buildSlideAction(self);
                        self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
                        for (int num10 = 0; num10 < self.preloadedScenes.Length; num10++)
                        {
                            self.preloadedScenes[num10] = new SlideShowMenuScene(self, self.pages[0], self.playList[num10].sceneID);
                            self.preloadedScenes[num10].Hide();
                        }
                        break;
                    }
                }
                foreach (var arg in slideOutroArgs)
                {
                    if (arg.id == slideShowID)
                    {
                        if (arg.buildSlideAction == null)
                            return;
                        arg.buildSlideAction(self);
                        self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
                        for (int num10 = 0; num10 < self.preloadedScenes.Length; num10++)
                        {
                            self.preloadedScenes[num10] = new SlideShowMenuScene(self, self.pages[0], self.playList[num10].sceneID);
                            self.preloadedScenes[num10].Hide();
                        }
                        break;
                    }
                }
            }
            catch
            {
                Debug.LogError("[Nutils] Yeah slide show has some bugs, but i don't want to fix");
            }
            self.NextScene();
        }

        private static void SlugcatSelectMenu_StartGameIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<MainLoopProcess>("manager"),
                i => i.MatchLdsfld<ProcessManager.ProcessID>("Game")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<ProcessManager.ProcessID, SlugcatSelectMenu, SlugcatStats.Name, ProcessManager.ProcessID>>((id, self, name) =>
                {
                    foreach (var arg in slideIntroArgs)
                    {
                        if (arg.name == name.value)
                        {
                            self.manager.nextSlideshow = arg.id;
                            return ProcessManager.ProcessID.SlideShow;
                        }
                    }
                    return id;
                });

            }

        }

        static List<SlideShowArg> slideIntroArgs;
        static List<SlideShowArg> slideOutroArgs;
        static Dictionary<MenuScene.SceneID, Action<MenuScene>> sceneArgs;

        #endregion
    }
    public struct SlideShowArg
    {
        public string music;
        public string name;
        public SlideShow.SlideShowID id;
        public Action<SlideShow> buildSlideAction;
    }


}
