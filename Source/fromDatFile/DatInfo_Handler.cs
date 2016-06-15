using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using Elan.Generic;

namespace bns_model_info
{
    class LookUps
    {
        public LookUps(string xml_file_path)
        {
            xfp = xml_file_path;
        }
        string xfp;
        public IEnumerable<HashSet<string>> Entity()
        {
            var reader = XmlReader.Create(xfp);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "lookup")
                {
                    bool enc = false;
                    var set = new HashSet<string>();
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "word")
                        {
                            enc = true;
                            reader.Read();
                            set.Add(reader.Value);
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "lookup")
                        {
                            break;
                        }
                    }
                    if (enc)
                    {
                        yield return set;
                    }
                }
            }
            reader.Close();
        }
        public int Count()
        {
            return Entity().Count();
        }
        public int Total()
        {
            int j = 0;
            foreach (var set in Entity()) j += set.Count;
            return j;
        }
    }

    class Alias_Human
    {
        LookUps alu;
        public Alias_Human(string fp_iName)
        {
            alu = new LookUps(fp_iName);
        }

        IEnumerable<HashSet<string>> RelationEntity()
        {
            Func<string, bool> Is_ItemName = (string x) => (x.StartsWith("Item.Name2."));
            Func<string, string> Raw_toAlias = (string x) => (x.Substring(11));
            foreach (var set in alu.Entity())
            {
                if (set.Count == 2 &&
                    Is_ItemName(set.ElementAt(0)) &&
                    set.ElementAt(1) != "")
                {
                    var result = new HashSet<string>();
                    result.Add(Raw_toAlias(set.ElementAt(0)));
                    result.Add(set.ElementAt(1));
                    yield return result;
                }
            }
        }
        public int Count()
        {
            return RelationEntity().Count();
        }
        public int Total()
        {
            int j = 0;
            foreach (var set in RelationEntity()) j += set.Count;
            return j;
        }

        public Dictionary<string, HashSet<string>> DictEntity()
        {
            return RelationEntity().JoinMap_RangeAsSet(
                (HashSet<string> set) => (set.ElementAt(0)),
                (HashSet<string> set) => (set.ElementAt(1)));
        }

        public void Print_toFile()
        {
            var sw = new StreamWriter("ah.txt");
            foreach (var set in RelationEntity())
            {
                sw.WriteLine(set.ElementAt(0));
                sw.WriteLine($"\t{set.ElementAt(1)}");
                sw.WriteLine();
            }
            sw.Close();
        }
    }

    class Alias_ObjSet
    {
        public Dictionary<string, HashSet<string>> ah_map;
        LookUps alu;
        public Alias_ObjSet(string fp_iModel, Alias_Human ah)
        {
            ah_map = ah.DictEntity();
            alu = new LookUps(fp_iModel);
        }

        IEnumerable<HashSet<string>> RelationEntity()
        {
            Func<string, bool> Is_ItemName = (string x) => (ah_map.ContainsKey(x));
            foreach (var set in alu.Entity())
            {
                if (set.Count >= 2 &&
                    Is_ItemName(set.ElementAt(0)))
                {
                    yield return set;
                }
            }
        }

        static Func<HashSet<string>, HashSet<string>> domain_map =
            (HashSet<string> x) => {
                var _R = new HashSet<string>();
                int i = 0;
                foreach (var s in x)
                {
                    if (i > 0) _R.Add(s);
                    i++;
                }
                return _R;
            };

        static Func<HashSet<string>, string> range_map =
            (HashSet<string> x) => x.ElementAt(0);

        public Dictionary<HashSet<string>, HashSet<string>> DictEntity()
        {
            return RelationEntity().JoinMap_RangeAsSet(
                domain_map, range_map, EqualityComparer.HashSetEqualityComparer<string>.Get());
        }
        public int Count()
        {
            return RelationEntity().Count();
        }
    }

    class DatInfo_Generator
    {
        static DatInfo_Generator()
        {
            init_done = false;
            is_xml_location_set = false;
            QueryName_Ready = false;
        }

        static bool is_xml_location_set;
        public static void Set_XmlFile_Path(string fp_iModel, string fp_iName)
        {
            filePath_itemModelLookup = fp_iModel;
            filePath_itemNameLookup = fp_iName;
            is_xml_location_set = true;
        }

        static string filePath_itemModelLookup;
        static string filePath_itemNameLookup;
        static Dictionary<string, HashSet<string>> dict_aliasHuman;
        static Dictionary<HashSet<string>, HashSet<string>> dict_aliasObj;

        static bool init_done;
        public static void init()
        {
            if (!is_xml_location_set) throw new Exception("Coder's fault.");
            Console.Write("Initializing Name-Model relation builder...");
            var ah = new Alias_Human(filePath_itemNameLookup);
            var ao = new Alias_ObjSet(filePath_itemModelLookup, ah);
            dict_aliasHuman = ao.ah_map;
            dict_aliasObj = ao.DictEntity();
            Console.WriteLine($"Done, AliasHuman: {dict_aliasHuman.Count}, AliasObj: {dict_aliasObj.Count}");
            if (dict_aliasHuman.Count == 0)
            {
                Console.WriteLine($"[Error] AliasHuman info has nothing.");
            }
            if (dict_aliasObj.Count == 0)
            {
                Console.WriteLine($"[Error] AliasObj info has nothing.");
            }
            if (dict_aliasHuman.Count == 0 || dict_aliasObj.Count == 0)
            {
                Console.WriteLine($"  Find the right file, and change the relative program config option in the file \"{DatInfo_Config.Config_FileName}\".");
                Console.WriteLine("  One or more function(s) will not be useable.");
            }
            init_done = true;
        }

        public static string Generate_ObjHumanInfo()
        {
            if (!init_done) init();
            if (dict_aliasHuman.Count == 0 || dict_aliasObj.Count == 0)
            {
                Console.WriteLine($"[Error] Name Model Info could not be built, because one or two of AliasHuman and AliasObj has null info.");
                return "";
            }
            Console.Write("Generating Name Model Info...");
            var sb = new StringBuilder();
            foreach (var kvp_ao in dict_aliasObj)
            {
                foreach (var alias in kvp_ao.Value)
                {
                    sb.AppendLine(alias);
                    foreach (var human in dict_aliasHuman[alias])
                    {
                        sb.AppendLine(human);
                    }
                }
                foreach (var link in kvp_ao.Key)
                {
                    sb.AppendLine($"\t{link}");
                }
                sb.AppendLine();
            }
            Console.WriteLine("Done.");
            return sb.ToString();
        }

        public static bool QueryName_Ready;
        public static Dictionary<HashSet<string>, HashSet<string>> Dict_ObjRenderInfo_HumanName;
        public static void BuildMap_ObjRenderInfo_CNName()
        {
            if (!init_done) init();
            if (dict_aliasHuman.Count == 0 || dict_aliasObj.Count == 0)
            {
                Console.WriteLine($"[Error] Model-Name Map could not be built, because one or two of AliasHuman and AliasObj has null info.");
                QueryName_Ready = false;
                return;
            }
            Console.Write("Building Model-Name Map...");
            Dict_ObjRenderInfo_HumanName = new Dictionary<HashSet<string>, HashSet<string>>();
            foreach (var kvp in dict_aliasObj)
            {
                var val = kvp.Value.SelectMany(x => dict_aliasHuman[x]).ToSet();
                Dict_ObjRenderInfo_HumanName.Add(kvp.Key, val);
            }
            Console.WriteLine($"Done, {Dict_ObjRenderInfo_HumanName.Count} entries.");
            QueryName_Ready = true;
        }
    }
}
