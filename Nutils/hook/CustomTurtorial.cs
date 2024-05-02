using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nutils.hook
{
    /// <summary>
    /// 一个自定义的教程
    /// 在构造函数里传入Message数组就可以自动播放
    /// '/'字符可以分割翻译文本，这样方便添加自定义按键这样的动态文本（可见thewanderer里）
    /// </summary>
    public class CustomTurtorial : UpdatableAndDeletable
    {
        public CustomTurtorial(Room room, Message[] list)
        {
            this.room = room;
            if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
                Destroy();
            messageList = list;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null &&
                room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
            {
                for (int index = 0;index < messageList.Length;index++)
                {
                    var texts = messageList[index].text.Split('/');
                    string transTest = "";
                    foreach (var text in texts)
                        transTest += Custom.rainWorld.inGameTranslator.Translate(text);


                    room.game.cameras[0].hud.textPrompt.AddMessage(transTest, messageList[index].wait, messageList[index].time, false, ModManager.MMF);
                   
                }
           
                slatedForDeletetion = true;
            }
        }
        public class Message
        {
            public string text;
            public int wait;
            public int time;
            Message(string s, int w, int t)
            {
                text = s;
                wait = w;
                time = t;
            }
            static public Message NewMessage(string s, int w, int t)
            {
                return new Message(s, w, t);
            }
        }

        readonly Message[] messageList;
    }
}
