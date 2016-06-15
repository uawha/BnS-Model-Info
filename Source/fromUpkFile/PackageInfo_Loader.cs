using System;
using System.Text;
using Elan.MISC;
using System.IO;
using UELib;
using System.Collections.Generic;
using System.Linq;

namespace bns_model_info
{
    static class PackageInfo_Loader
    {
        static bool Get_UPIInfo(string class_name, string parent_name, string object_name,
            out UPortedItem_Info upi_info, UPackage_Info claimer)
        {
            if (ignored_pkgs.Contains(parent_name))
            {
                upi_info = null;
                return false;
            }
            else
            {
                switch (class_name)
                {
                    case "MaterialInstanceConstant":
                        upi_info = new UPortedItem_Info() {
                            Claimer = claimer,
                            ObjClass = UObjectClass.MaterialInstanceConstant,
                            Parent = parent_name.ToLower(),
                            ObjName = object_name
                        };
                        return true;
                    case "SkeletalMesh":
                        upi_info = new UPortedItem_Info() {
                            Claimer = claimer,
                            ObjClass = UObjectClass.SkeletalMesh,
                            Parent = parent_name.ToLower(),
                            ObjName = object_name
                        };
                        return true;
                    case "Texture2D":
                        upi_info = new UPortedItem_Info() {
                            Claimer = claimer,
                            ObjClass = UObjectClass.Texture2D,
                            Parent = parent_name.ToLower(),
                            ObjName = object_name
                        };
                        return true;
                    default:
                        upi_info = null;
                        return false;
                }
            }
        }

        #region file name
        static string Get_LeafEntry_Name(string file_path)
        {
            int li = file_path.LastIndexOf('\\');
            if (li < 0)
            {
                return file_path;
            }
            else if (li < file_path.Length - 1)
            {
                return file_path.Substring(li + 1);
            }
            else
            {
                return String.Empty;
            }
        }

        static string Get_PkgName(string file_path)
        {
            string file_name = Get_LeafEntry_Name(file_path);
            int li = file_name.LastIndexOf('.');
            if (li < 0)
            {
                return String.Empty;
            }
            else if (li < file_name.Length - 1)
            {
                return file_name.Substring(0, li);
            }
            else
            {
                return file_name;
            }
        }
        #endregion
        static UPackage_Info LoadInfo_fromFile(string file_path)
        {
            var pkg = UnrealLoader.LoadFullPackage(file_path);
            var _R = new UPackage_Info();

            var imp = new List<UPortedItem_Info>();
            foreach (var v in pkg.Imports)
            {
                UPortedItem_Info upi;
                if (Get_UPIInfo(v.ClassName, v.OuterName, v.ObjectName,
                    out upi, _R))
                {
                    imp.Add(upi);
                }
            }

            var exp = new List<UPortedItem_Info>();
            foreach (var v in pkg.Exports)
            {
                UPortedItem_Info upi;
                if (Get_UPIInfo(v.ClassName, v.OuterName, v.ObjectName,
                    out upi, _R))
                {
                    exp.Add(upi);
                }
            }

            _R.PackageName = Get_PkgName(file_path).ToLower();
            _R.Imports = imp;
            _R.Exports = exp;

            return _R;
        }

        static string[] ignored_pkgs = new string[] {
            "00002620",
            "00002640",
            "00012963",
            "00008613",
            "00001461",
            "00012850",
            "00011426"
        };
        static bool If_LoadFile(string file_path)
        {
            string pkg_name = Get_PkgName(file_path);
            int j;
            bool is_num = Int32.TryParse(pkg_name, out j);
            return file_path.ToLower().EndsWith(".upk") &&
                is_num && !ignored_pkgs.Contains(pkg_name);
        }
        static string log_file = "error.log";

        static bool is_UFolder_set;
        static PackageInfo_Loader()
        {
            is_UFolder_set = false;
        }
        static string u_folder;
        public static string UFolder {
            set {
                u_folder = value;
                is_UFolder_set = true;
            }
        }
        public static List<UPackage_Info> Load_Folder(string log_path = "")
        {
            if (!is_UFolder_set) throw new Exception("Coder's fault.");
            var files = Directory.EnumerateFiles(u_folder, "*", SearchOption.AllDirectories)
                .Where(x => If_LoadFile(x)).ToArray();
            var _R = new List<UPackage_Info>();
            //
            string vac255 = MISC.GetSpaceString_ofLength(255);
            Console.BufferWidth = 300;
            Console.WriteLine();
            var log_sb = new StringBuilder();
            foreach (var v in files)
            {
                Console.CursorTop -= 1;
                Console.WriteLine(vac255);
                Console.CursorTop -= 1;
                Console.Write($"[Reading from] {v}");
                TextWriter tmp = Console.Out;
                StringWriter tsw = new StringWriter();
                Console.SetOut(tsw);
                try
                {
                    _R.Add(LoadInfo_fromFile(v));
                    tsw.Close();
                    Console.SetOut(tmp);
                    Console.WriteLine(" > Success");
                }
                catch
                {
                    tsw.Close();
                    Console.SetOut(tmp);
                    Console.WriteLine($" < Failure, file size: {MISC.GetString_ofFileLength(v)}\n");
                    log_sb.AppendLine($"[Reading Failure] {v} [File Size: {MISC.GetString_ofFileLength(v)}]");
                }
            }
            var log = log_sb.ToString();
            if (log?.Length > 0)
            {
                Console.Write("Writing log to ouput folder...");
                File.WriteAllText($"{log_path}{log_file}", log);
                Console.WriteLine("Done.");
            }
            Console.WriteLine("The loading process is done.");
            return _R;
        }
    }
}
