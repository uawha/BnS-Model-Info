using System;
using System.IO;
using System.Xml.Linq;

namespace bns_model_info
{
    class DatInfo_Config
    {
        public static string Config_FileName = "bns_model_info.config";
        public static string AliasObj_FileName = "datafile_103.xml";
        public static string AliasHuman_FileName = "datafile_230.xml";
        public static void init()
        {
            if (File.Exists(Config_FileName))
            {
                merge_config();
            }
            else
            {
                write_config();
            }
        }
        static void write_config()
        {
            var root = new XElement(xname_root);
            var op_AliasObj = new XElement(xname_op_AliasObj, AliasObj_FileName);
            root.Add(op_AliasObj);
            var op_AliasHuman = new XElement(xname_op_AliasHuman, AliasHuman_FileName);
            root.Add(op_AliasHuman);
            try
            {
                File.WriteAllText(Config_FileName, root.ToString());
            }
            catch
            {
                Console.WriteLine("[Warning] Cannot write config file.");
            }
        }
        const string xname_root = "Config";
        const string xname_op_AliasObj = "FileName_AliasObject";
        const string xname_op_AliasHuman = "FileName_AliasHuman";
        static void merge_config()
        {
            try
            {
                var config = XElement.Parse(File.ReadAllText(Config_FileName));
                merge_op(config, xname_op_AliasObj);
                merge_op(config, xname_op_AliasHuman);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Warning] Exception when try to read config. Message:");
                Console.WriteLine($"         {e.Message}");
                Console.WriteLine("[Warning] Will use internal config. Errors may occur.");
            }
        }
        static void merge_op(XElement config, string xname)
        {
            var op = config.Element(xname);
            if (op?.Value?.Length > 0)
            {
                string ok = op.Value;
                Console.WriteLine($"[Config] Using \"{ok}\" for option \"{xname}\"");
                switch (xname)
                {
                    case xname_op_AliasObj:
                        AliasObj_FileName = ok;
                        break;
                    case xname_op_AliasHuman:
                        AliasHuman_FileName = ok;
                        break;
                    default:
                        throw new Exception("Coder's fault.");
                }
            }
        }
    }
}
