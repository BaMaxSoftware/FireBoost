namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class DBSeal
    {
        /// <summary></summary>
        public int Type { get; }
        /// <summary></summary>
        public int FireProtection { get; }
        /// <summary></summary>
        public int HostInteger { get; }
        /// <summary></summary>
        public int ShapeNameInteger { get; }
        /// <summary></summary>
        public int MaterialInteger { get; }
        /// <summary></summary>
        public int DesignInteger { get; }

        /// <summary></summary>
        public DBSeal(int hostId, int shapeId, int typeId, int materialId, int designId, int fireProtection)
        {
            HostInteger = hostId;
            ShapeNameInteger = shapeId;
            Type = typeId;
            MaterialInteger = materialId;
            DesignInteger = designId;
            FireProtection = fireProtection;
        }
    }
}
