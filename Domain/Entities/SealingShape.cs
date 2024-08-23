using FireBoost.Domain.Enums;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingShape
    {
        /// <summary></summary>
        public string Name { get; }
        /// <summary></summary>
        public SealingShapeType Shape { get; }

        /// <summary></summary>
        public SealingShape(string name, SealingShapeType shape)
        { 
            Name = name;
            Shape = shape;
        }

        /// <summary></summary>
        public override string ToString() => Name;
    }
}
