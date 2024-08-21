namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class DBSeal
    {
        /// <summary></summary>
        public int Id { get; }
        /// <summary></summary>
        public string Name { get; }
        /// <summary></summary>
        public string HostName { get; }
        /// <summary></summary>
        public string ShapeName { get; }
        /// <summary></summary>
        public int Type { get; }
        /// <summary></summary>
        public string Material { get; }
        /// <summary></summary>
        public string Design { get; }
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
        public DBSeal(int id, string name, string hostName, string shapeName, int type, string material, string design, int fireProtection)
        {
            Id = id;
            Name = name;
            HostName = hostName;
            ShapeName = shapeName;
            Type = type;
            Material = material;
            Design = design;
            FireProtection = fireProtection;
        }
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
