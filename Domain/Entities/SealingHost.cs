using Autodesk.Revit.DB;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingHost
    {
        /// <summary></summary>
        public string Name { get; }
        /// <summary></summary>
        public BuiltInCategory BuiltInCategory { get; }
        /// <summary></summary>
        public int DBId { get; }

        /// <summary></summary>
        public SealingHost(string name,  BuiltInCategory builtInCategory, int dBId)
        {
            Name = name;
            BuiltInCategory = builtInCategory;
            DBId = dBId;
        }

        /// <summary></summary>
        public override string ToString() => Name;
    }
}
