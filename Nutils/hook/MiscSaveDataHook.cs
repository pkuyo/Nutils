using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace Nutils.hook
{
    internal static class MiscSaveDataHook
    {
        public static void OnModsInit()
        {
            if (!isLoaded)
            {
               
                On.MiscWorldSaveData.FromString += MiscWorldSaveData_FromString;
                On.MiscWorldSaveData.ToString += MiscWorldSaveData_ToString;
                On.MiscWorldSaveData.ctor += MiscWorldSaveData_ctor;
                isLoaded = true;
            }
        }

        private static void MiscWorldSaveData_ctor(On.MiscWorldSaveData.orig_ctor orig, MiscWorldSaveData self, SlugcatStats.Name slugcat)
        {
            orig(self, slugcat);

            if (saveDatas.TryGetValue(self, out _))
                saveDatas.Remove(self);

            var customSave = new Dictionary<string, object>();
            saveDatas.Add(self, customSave);
        }

        public static void Register<T>(string name) where T : class, new()
        {
            dataTypes.Add(name, typeof(T));
        }

        public static void Register(Type type, string name)
        {
            dataTypes.Add(name, type);
        }

        public static bool TryGetCustomValue<T>(this MiscWorldSaveData data, string name, out T value) where T : class, new()
        {
            value = null;
            if(!dataTypes.ContainsKey(name))
                dataTypes.Add(name,typeof(T));
            if (saveDatas.TryGetValue(data, out var dic))
            {
                if (!dic.ContainsKey(name))
                    dic.Add(name, new T());
                if (dic[name] is T t)
                {
                    value = t;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }

            }
            return false;
        }

        public static bool TrySetCustomValue<T>(this MiscWorldSaveData data, string name, T value) where T : class, new()
        {
            if (!dataTypes.ContainsKey(name))
                dataTypes.Add(name, typeof(T));
            if (saveDatas.TryGetValue(data, out var dic) && dataTypes.ContainsKey(name))
            {
                dic[name] = value;
                return true;
            }
            return false;
        }
        private static string MiscWorldSaveData_ToString(On.MiscWorldSaveData.orig_ToString orig, MiscWorldSaveData self)
        {
            var re = orig(self);
            if (saveDatas.TryGetValue(self, out var datas))
            {
                foreach (var data in datas)
                {
                    var value =  ToString(dataTypes[data.Key], data.Value);
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



        private static void MiscWorldSaveData_FromString(On.MiscWorldSaveData.orig_FromString orig,
            MiscWorldSaveData self, string s)
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

        private static ConditionalWeakTable<MiscWorldSaveData, Dictionary<string,object>> saveDatas = new ConditionalWeakTable<MiscWorldSaveData, Dictionary<string, object>>();

    }

}
