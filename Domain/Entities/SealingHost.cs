using Autodesk.Revit.DB;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingHost
    {
        /// <summary></summary>
        public string Name { get; }
        /// <summary></summary>
        public BuiltInCategory[] AllowedCategories { get; }
        /// <summary></summary>
        public int DBId { get; }

        /// <summary></summary>
        public SealingHost(int dBId, string name,  BuiltInCategory[] allowedCategories)
        {
            Name = name;
            AllowedCategories = allowedCategories;
            DBId = dBId;
        }

        /// <summary></summary>
        public override string ToString() => Name;
    }
}
