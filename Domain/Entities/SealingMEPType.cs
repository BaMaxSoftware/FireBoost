using Autodesk.Revit.DB;
using FireBoost.Domain.Enums;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingMEPType
    {
        /// <summary></summary>
        public string MainTypeString { get;}
        /// <summary></summary>
        public sbyte MainType { get; }
        /// <summary></summary>
        public sbyte Type { get; }
        /// <summary></summary>
        public string SubType { get; }
        /// <summary></summary>
        public BuiltInCategory[] AllowCategories { get; }
        /// <summary></summary>
        public SealingShapeType[] OpeningShapes { get; }
        /// <summary></summary>
        public string[] Materials { get; }
        /// <summary></summary>
        public string[] ConstructDesigns { get; }

        
        /// <summary></summary>
        public SealingMEPType(sbyte mainType, string mainTypeString, sbyte type, string subType, BuiltInCategory[] allowCategories)
        {
            MainTypeString = mainTypeString;
            MainType = mainType;
            Type = type;
            SubType = subType;
            AllowCategories = allowCategories;
        }
    }
}
