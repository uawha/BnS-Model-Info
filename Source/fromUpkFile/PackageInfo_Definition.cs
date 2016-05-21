using System;
using System.Collections.Generic;

namespace bns_model_info
{
    enum UObjectClass { MaterialInstanceConstant, SkeletalMesh, Texture2D }
    enum UObjPurposeClass { Material, Mesh, Texture }
    class UPackage_Info
    {
        public string PackageName;
        public List<UPortedItem_Info> Imports;
        public List<UPortedItem_Info> Exports;
    }

    class UPortedItem_Info
    {
        public UPackage_Info Claimer;
        public UObjectClass ObjClass;
        public string Parent;
        public string ObjName;

        public string Get_GlobalName()
        {
            return $"{Claimer.PackageName}.{ObjName}";
        }
        public bool Is(UObjPurposeClass purpose_class)
        {
            switch (purpose_class)
            {
                case UObjPurposeClass.Material:
                    return ObjClass.Equals(UObjectClass.MaterialInstanceConstant);
                case UObjPurposeClass.Mesh:
                    return ObjClass.Equals(UObjectClass.SkeletalMesh);
                case UObjPurposeClass.Texture:
                    return ObjClass.Equals(UObjectClass.Texture2D);
                default:
                    throw new Exception("Coder's fault.");
            }
        }
    }
}
