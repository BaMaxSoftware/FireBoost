namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingFireResistance
    {
        /// <summary></summary>
        public byte Minutes { get; }
        /// <summary></summary>
        public int Depth { get; }

        private readonly string _description;
        /// <summary></summary>
        public string Description => _description;

        private (string EI, string EIT) _marks = ("EI", "EIT");
        /// <summary></summary>
        public (string EI, string EIT) Marks => _marks;

        /// <summary></summary>
        public SealingFireResistance(byte minutes, string description, int depth)
        {
            Minutes = minutes;
            Depth = depth;
            _description = description;
        }

        /// <summary></summary>
        public override string ToString() => Description;
    }
}
