using System;
using Elan.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bns_model_info
{
    static class Ext
    {
        public static bool Contains_ObjOfClass(this IEnumerable<UPortedItem_Info> input,
            UObjPurposeClass purpose_class)
        {
            foreach (var v in input)
            {
                if (v.Is(purpose_class)) return true;
            }
            return false;
        }
    }

    static class UPackageInfo_Analyzer
    {
        static UPackageInfo_Analyzer()
        {
            init_done = false;
        }

        static bool ifQueryName;
        const string NotExist = "-= not exist :) ";
        const string TexMarker = ".dds";
        const string MatMarker = ".mat";
        const string MeshMarker = ".psk";
        public static string Generate_ModelInfo(bool if_query_name = false)
        {
            if (!init_done) throw new Exception("Coder's fault.");
            Console.Write("Now arranging output...be patient...");
            if (if_query_name && !DatInfo_Generator.QueryName_Ready)
            {
                Console.WriteLine("[Error] The document contains NAME cannot be generated because of lack of information.");
                return "";
            }
            else
            {
                ifQueryName = if_query_name;
            }
            var sb = new StringBuilder();
            foreach (var mesh_pkg in meshPkgs)
            {
                var mat_imps = mesh_pkg.Imports.Where(x => x.Is(UObjPurposeClass.Material)).ToList();
                if (mat_imps?.Count > 0)
                {
                    ///get all col pkg names referenced by this mesh_pkg
                    var matPkgNames_refByMesh = mat_imps.Select(x => x.Parent).ToSet();
                    if (mesh_pkg.Exports.Contains_ObjOfClass(UObjPurposeClass.Material))
                    {
                        matPkgNames_refByMesh.Add(mesh_pkg.PackageName);
                    }

                    ///get all tex pkg name referenced by the above col pkgs.
                    var texPkgNames_refByMat = new HashSet<string>();
                    if (matPkgNames_refByMesh?.Count > 0)
                    {
                        foreach (var mat_name in matPkgNames_refByMesh)
                        {
                            if (map_nameUpk.ContainsKey(mat_name))
                                texPkgNames_refByMat.UnionWith(
                                    map_nameUpk[mat_name].Imports
                                    .Where(x => x.Is(UObjPurposeClass.Texture) && x.Parent != String.Empty)
                                    .Select(x => x.Parent));
                        }
                    }

                    ///now let's write.
                    var meshObjs = mesh_pkg.Exports.Where(x => x.Is(UObjPurposeClass.Mesh)).ToList();
                    foreach (var meshObj in meshObjs)
                        sb.AppendLine($"{meshObj.ObjName}{MeshMarker} > {mesh_pkg.PackageName}");
                    string tab = "\t";
                    if (texPkgNames_refByMat.Count > 0)
                    {
                        foreach (var tex_pkg_name in texPkgNames_refByMat)
                        {
                            sb.AppendLine($"-----= Texture UPK: {tex_pkg_name} =-");
                            var teXs_in_texPkg = new HashSet<string>();
                            if (map_nameUpk.ContainsKey(tex_pkg_name))
                                teXs_in_texPkg = map_nameUpk[tex_pkg_name].Exports.Where(x => x.Is(UObjPurposeClass.Texture)).Select(x => x.ObjName).ToSet();
                            var texSet_ref_ed = new HashSet<string>();
                            var infos = get_texReference_madeByMat(tex_pkg_name, pkgs_who_reqTex);
                            if (infos.Count > 0)
                            {
                                /// to get a minor file out, enable this!
                                if (infos.Count > 50) continue;
                                foreach (var info in infos)
                                {
                                    if (info.Item2.Count > 0)
                                    {
                                        foreach (var matObj in info.Item2)
                                        {
                                            sb.AppendLine($"{tab}{tab}{matObj.ObjName}{MatMarker} > {info.Item1}{query_name(meshObjs, matObj)}");
                                        }
                                    }
                                    foreach (var texObj in info.Item3)
                                    {
                                        sb.AppendLine($"{tab}{tab}{tab}{(teXs_in_texPkg.Contains(texObj.ObjName) ? String.Empty : NotExist)}{texObj.ObjName}{TexMarker} > {tex_pkg_name}");
                                        texSet_ref_ed.Add(texObj.ObjName);
                                    }
                                }
                                teXs_in_texPkg.ExceptWith(texSet_ref_ed);
                                if (teXs_in_texPkg.Count > 0)
                                {
                                    sb.AppendLine($"{tab}{tab}-= Not Referenced =-");
                                    foreach (var v in teXs_in_texPkg)
                                    {
                                        sb.AppendLine($"{tab}{tab}{tab}{v}{TexMarker} > {tex_pkg_name}");
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendLine($"{tab}{tab}-= None Reference to This =-");
                            }
                            sb.AppendLine();
                        }
                    }
                    sb.AppendLine();
                }
            }
            Console.WriteLine("Done.");
            return sb.ToString();
        }

        // fast retrival by name
        static Dictionary<string, UPackage_Info> map_nameUpk;
        // to make loop objs' count smaller
        static List<UPackage_Info> pkgs_who_reqTex;
        static IEnumerable<UPackage_Info> meshPkgs;
        static List<UPackage_Info> matPkgs;
        static bool init_done;
        public static void init(List<UPackage_Info> input)
        {
            Console.Write("Now initializing model info generator...");
            pkgs_who_reqTex = input.Where(x => x.Imports.Contains_ObjOfClass(UObjPurposeClass.Texture)).ToList();
            map_nameUpk = input.Get_ReverseSampleMap_of(x => x.PackageName);
            meshPkgs = input.Where(x => x.Exports.Contains_ObjOfClass(UObjPurposeClass.Mesh));
            matPkgs = input.Where(x => x.Exports.Contains_ObjOfClass(UObjPurposeClass.Material)).ToList();
            init_done = true;
            Console.WriteLine("Done.");
        }

        static string[] ignored_texObjNames = new string[] {
            "engineresources",
            "enginematerials"
        };
        //////////////// col pkg,   cols,                 tex s the col pkg required.
        static List<Tuple<string, List<UPortedItem_Info>, List<UPortedItem_Info>>>
            get_texReference_madeByMat(string texPackageName, List<UPackage_Info> infos)
        {
            var _R = new List<Tuple<string, List<UPortedItem_Info>, List<UPortedItem_Info>>>();
            foreach (var pkg in infos)
            {
                var tex_refs = pkg.Imports
                    .Where(x => x.Is(UObjPurposeClass.Texture) && x.Parent == texPackageName && !ignored_texObjNames.Contains(x.Parent))
                    .ToList();
                if (tex_refs?.Count() > 0)
                {
                    var mats = pkg.Exports
                        .Where(x => x.Is(UObjPurposeClass.Material))
                        .ToList();
                    _R.Add(new Tuple<string, List<UPortedItem_Info>, List<UPortedItem_Info>>(
                        pkg.PackageName, mats, tex_refs));
                }
            }
            return _R;
        }

        static string get_names(IEnumerable<string> input)
        {
            var sb = new StringBuilder();
            foreach (var name in input)
            {
                sb.Append($" <{name}>");
            }
            return sb.ToString();
        }

        static string query_name(List<UPortedItem_Info> meshObjs, UPortedItem_Info matObj)
        {
            if (ifQueryName && meshObjs.Count == 1)
            {
                if (!DatInfo_Generator.QueryName_Ready) throw new Exception("Coder's fault.");
                var hs = new HashSet<string>();
                hs.Add(meshObjs.ElementAt(0).Get_GlobalName());
                hs.Add(matObj.Get_GlobalName());
                if (hs.Count == 2)
                {
                    foreach (var kvp in DatInfo_Generator.Dict_ObjRenderInfo_HumanName)
                    {
                        if (hs.IsSubsetOf(kvp.Key))
                        {
                            return get_names(kvp.Value);
                        }
                    }
                    return String.Empty;
                }
                else return String.Empty;
            }
            else return String.Empty;
        }
    }
}
