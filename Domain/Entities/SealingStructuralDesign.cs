using FireBoost.Domain.Enums;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingStructuralDesign
    {
        /// <summary></summary>
        public string Mark { get; }
        /// <summary></summary>
        public string Descripcions { get; }
        /// <summary></summary>
        public SealingStructuralDesignType StructuralDesign { get; }

        /// <summary></summary>
        public SealingStructuralDesign(string mark, string descripcions, SealingStructuralDesignType structuralDesign)
        {
            Mark = mark;
            Descripcions = descripcions;
            StructuralDesign = structuralDesign;
        }

        /// <summary></summary>
        public override string ToString() => Descripcions;
    }
}
