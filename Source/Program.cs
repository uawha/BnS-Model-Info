using System;
using Elan.Generic;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace bns_model_info
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "BnS Model Info Generator, ver.20160521";
            Program_Parameter para;
            if (Program_Parameter.Parse(args, out para))
            {
                PrepareData(para);
                //now write!
                if (para.IfWrite_ModelInfo)
                {
                    File.WriteAllText($"BnS_ModelInfo.{MISC.GetPathable_YMD(DateTime.Now)}.txt", UPackageInfo_Analyzer.Generate_ModelInfo(false));
                }
                if (para.IfWrite_ModelInfo_withName)
                {
                    File.WriteAllText($"BnS_ModelInfo_withName.{MISC.GetPathable_YMD(DateTime.Now)}.txt", UPackageInfo_Analyzer.Generate_ModelInfo(true));
                }
                if (para.IfWrite_NameModelRelation)
                {
                    File.WriteAllText($"BnS_NameModel_Relation.{MISC.GetPathable_YMD(DateTime.Now)}.txt", DatInfo_Generator.Generate_ObjHumanInfo());
                }
            }
            Console.WriteLine("End of Program.");
            Console.WriteLine();
            Console.WriteLine("   Programming by Elan(or iVAiU, I no longer use that name).");
            Console.WriteLine("   Many thanks to: Eliot van Uytfanghe (UELib), ronny1982 (bnsdat).");
            Console.ReadLine();
        }

        static void PrepareData(Program_Parameter para)
        {
            ///先处理这个。因为UPK处理时间有点长，这个要是之后出错了时间损失要大一些。
            if (para.IfWrite_ModelInfo_withName || para.IfWrite_NameModelRelation)
            {
                DatInfo_Generator.init();
                if (para.IfWrite_ModelInfo_withName)
                {
                    DatInfo_Generator.BuildMap_ObjRenderInfo_CNName();
                }
            }
            if (para.IfWrite_ModelInfo)
            {
                UPackageInfo_Analyzer.init(PackageInfo_Loader.Load_Folder());
            }
        }
    }
    /// <summary>
    /// 这个类分析用户输入并分配相关参数
    /// </summary>
    class Program_Parameter
    {
        const string op_build_modelInfo_withChnName = "-mcn";
        const string op_build_modelInfo = "-m";
        const string op_build_chnNameModelRelation = "-cn";
        const string path_prefix_upk = "-up=";
        const string path_prefix_dat = "-dp=";

        public static bool Parse(string[] args, out Program_Parameter parameter)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Not enough parameters.");
                parameter = null;
                return false;
            }
            else
            {
                var lcs = args.Select(x => x.ToLower()).ToList();
                parameter = new Program_Parameter() {
                    IfWrite_ModelInfo = lcs.Contains(op_build_modelInfo),
                    IfWrite_ModelInfo_withName = lcs.Contains(op_build_modelInfo_withChnName),
                    IfWrite_NameModelRelation = lcs.Contains(op_build_chnNameModelRelation),
                    FolderPath_Upk = get_firstContent_prefixedBy(lcs, path_prefix_upk),
                    FolderPath_Dat = get_firstContent_prefixedBy(lcs, path_prefix_dat)
                };
                if (parameter.dispatch())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        bool dispatch()
        {
            /// req path valid?
            if (IfWrite_ModelInfo || IfWrite_ModelInfo_withName)
            {
                if (FolderPath_Upk == null)
                {
                    Console.WriteLine("You wanted to generate Model Info but you did not specify a UPK folder.");
                    return false;
                }
                else
                {
                    if (Directory.Exists(FolderPath_Upk))
                    {
                        PackageInfo_Loader.UFolder = FolderPath_Upk;
                    }
                    else
                    {
                        Console.WriteLine($"The UPK folder you provided ({FolderPath_Upk}) does not exist.");
                        return false;
                    }
                }
            }

            if (IfWrite_ModelInfo_withName || IfWrite_NameModelRelation)
            {
                if (FolderPath_Dat == null)
                {
                    Console.WriteLine("You wanted to generate info with Name-Model relation but you did not specify a folder contains the requested XMLs.");
                    return false;
                }
                else
                {
                    if (Directory.Exists(FolderPath_Dat))
                    {
                        var files = Directory.EnumerateFiles(FolderPath_Dat, "*.xml", SearchOption.AllDirectories);
                        string xml103 = get_file(files, "datafile_103.xml");
                        string xml230 = get_file(files, "datafile_230.xml");
                        if (xml103 != null && xml230 != null)
                        {
                            DatInfo_Generator.Set_XmlFile_Path(xml103, xml230);
                        }
                        else
                        {
                            Console.WriteLine($"Can not find one or two XML(s) in {FolderPath_Dat}");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"The folder you provided for XMLs ({FolderPath_Dat}) does not exist.");
                        return false;
                    }
                }
            }

            if (!IfWrite_ModelInfo && !IfWrite_ModelInfo_withName && !IfWrite_NameModelRelation)
            {
                Console.WriteLine("You didn't provide a single option. Check your input.");
                return false;
            }

            Console.WriteLine("Parameter Dispatched Successfully.");
            return true;
        }
        static string get_firstContent_prefixedBy(List<string> args, string prefix)
        {
            string s = args.Find(x => x.StartsWith(prefix));
            if (s?.Length > prefix.Length)
            {
                return s.Substring(prefix.Length);
            }
            else
            {
                return null;
            }
        }
        static string get_file(IEnumerable<string> files, string file_name_lowercase)
        {
            string s;
            if (files.FindFirst_byCondition(x => x.ToLower().EndsWith($"\\{file_name_lowercase}"), out s))
            {
                return s;
            }
            else
            {
                return null;
            }
        }
        public bool IfWrite_ModelInfo;
        public bool IfWrite_ModelInfo_withName;
        public bool IfWrite_NameModelRelation;
        public string FolderPath_Upk;
        public string FolderPath_Dat;
    }
}
