using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Nutils.hook
{
    public static class DeathSaveDataHook
    {
        public static void OnModsInit()
        {
            if (!isLoaded)
            {
         
                On.DeathPersistentSaveData.FromString += DeathPersistentSaveData_FromString;
                On.DeathPersistentSaveData.SaveToString += DeathPersistentSaveData_SaveToString;
                On.DeathPersistentSaveData.ctor += DeathPersistentSaveData_ctor;
                isLoaded = true;
            }
        }

        private static void DeathPersistentSaveData_ctor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
        {
            orig(self,slugcat);

            var customSave = new Dictionary<string, (string origValue, object newValue)>();
            saveDatas.Add(self, customSave);
        }

        public static void Register<T>(string name) where T : class, new()
        {
    
            dataTypes.Add(name, typeof(T));
        }

        public static void Register(Type type,string name)
        {
            dataTypes.Add(name, type);
        }

        public static bool TryGetCustomValue<T>(this DeathPersistentSaveData data,string name, out T value) where T : class, new()
        {
            value = null;
            if (!dataTypes.ContainsKey(name))
                dataTypes.Add(name, typeof(T));
            if (saveDatas.TryGetValue(data, out var dic) && dataTypes.ContainsKey(name))
            {
                if(!dic.ContainsKey(name))
                    dic.Add(name, (null, new T()));
                if (dic[name].newValue is T t)
                {
                    value = t;
                    return true;
                }
                else
                    return false;
                

            }
            return false;
        }

        public static bool TrySetCustomValue<T>(this DeathPersistentSaveData data, string name, T value) where T : class, new()
        {
            if (!dataTypes.ContainsKey(name))
                dataTypes.Add(name, typeof(T));
            if (saveDatas.TryGetValue(data, out var dic) && dataTypes.ContainsKey(name))
            {
                dic[name] = (dic[name].Item1, value);
                return true;
            }
            return false;
        }
        private static string DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            var re = orig(self,saveAsIfPlayerDied,saveAsIfPlayerQuit);
            if (saveDatas.TryGetValue(self, out var datas))
            {
                foreach (var data in datas)
                {
                    var value = (saveAsIfPlayerDied || saveAsIfPlayerQuit) ? data.Value.origValue : ToString(dataTypes[data.Key], data.Value.newValue);
                    if (value != null)
                    {
                        var save = "NUTILS<dpB>" + data.Key + "<dpB>" + value + "<dpA>";
                        Plugin.Log("Save Data : {0}", save);
                        re += save;
                    }
                }
            }

            return re;
        }

  

        private static void DeathPersistentSaveData_FromString(On.DeathPersistentSaveData.orig_FromString orig,
            DeathPersistentSaveData self, string s)
        {
            orig(self, s);
            List<string> read = new List<string>();
            foreach (var line in self.unrecognizedSaveStrings)
            {
                string[] words = Regex.Split(line, "<dpB>");
                if (words[0] == "NUTILS" && words.Length == 3)
                {
                    if (dataTypes.ContainsKey(words[1]) && saveDatas.TryGetValue(self, out var list))
                    {
                        Plugin.Log("Loaded Data : {0} , {1}", words[1], words[2]);
                        list[words[1]] = (words[2], FromString(dataTypes[words[1]], words[2]));
                        read.Add(line);
                    }
                }
            }
            self.unrecognizedSaveStrings.RemoveAll(i => read.Contains(i));
        }

        private static object FromString(Type type, string s)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(s)))
            {
                XmlSerializer xml = new XmlSerializer(type);
                return xml.Deserialize(ms);
            }
        }
        private static string ToString(Type type, object s)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(type);

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);

                XmlWriter xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings
                {
                    Indent = false,
                    IndentChars = string.Empty,
                    NewLineChars = string.Empty,
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8
                });

                xml.Serialize(xmlWriter, s, ns);

                xmlWriter.Flush();
                xmlWriter.Close();

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static bool isLoaded;

        private static Dictionary<string, Type> dataTypes = new Dictionary<string, Type>();

        private static ConditionalWeakTable<DeathPersistentSaveData, Dictionary<string,(string origValue, object newValue)>> saveDatas =
            new ConditionalWeakTable<DeathPersistentSaveData, Dictionary<string, (string origValue, object newValue)>>();

    }
}
