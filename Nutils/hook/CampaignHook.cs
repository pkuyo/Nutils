using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nutils.hook
{
    static public class CampaignHook
    {
        /// <summary>
        /// 为剧情战役设置固定出生点
        /// </summary>
        /// <param name="name">剧情猫名称(slugcatstat.name.value)</param>
        /// <param name="x">出生点Tile的x坐标</param>
        /// <param name="y">出生点Tile的y坐标</param>
        /// <param name="abstractNode">如果想要设置出生点到特殊位置（比如出口）需要设置对应node值</param>
        /// <param name="roomName">初始房间名称 需和Slugbase里的一致</param>
        static public void AddSpawnPos(string name, int x, int y, int abstractNode, string roomName)
        {
            OnModsInit();
            SpawnPos a = new SpawnPos();
            a.x = x;
            a.y = y;
            a.abstractNode = abstractNode;
            a.roomName = roomName;
            pos.Add(name, a);
        }

        #region Hook
        static CampaignHook()
        {
            pos = new Dictionary<string, SpawnPos>();
            
        }

        static bool isLoad = false;
        static public void OnModsInit()
        {
            if (!isLoad)
            {
                On.RainWorldGame.SpawnPlayers_bool_bool_bool_bool_WorldCoordinate += RainWorldGame_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate;
            }
            isLoad=true;
        }

        static private AbstractCreature RainWorldGame_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate(On.RainWorldGame.orig_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate orig, RainWorldGame self, bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
        {
            if(pos.ContainsKey(self.session.characterStats.name.value) && self.world.GetAbstractRoom(location.room).name.ToUpper() == pos[self.session.characterStats.name.value].roomName)
            {
                location.x = pos[self.session.characterStats.name.value].x;
                location.y = pos[self.session.characterStats.name.value].y;
                location.abstractNode = pos[self.session.characterStats.name.value].abstractNode;
            }
            return orig(self, player1, player2, player3, player4, location);
        }

        static Dictionary<string, SpawnPos> pos;
        #endregion
    }

    public struct SpawnPos
    {
        public int x;
        public int y;
        public int abstractNode;
        public string roomName;
    }
}
