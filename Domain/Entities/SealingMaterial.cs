﻿using FireBoost.Domain.Enums;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class SealingMaterial
    {
        /// <summary></summary>
        public string Mark { get; }
        /// <summary></summary>
        public string Description { get; }
        /// <summary></summary>
        public SealingMaterials SealingMaterialType { get; set; }
        /// <summary></summary>
        public SealingStructuralDesign[] StructuralDesigns { get; set; }

        /// <summary></summary>
        public SealingMaterial(string mark, string description, SealingMaterials sealingMaterial)
        {
            Mark = mark;
            Description = description;
            SealingMaterialType = sealingMaterial;
        }

        /// <summary></summary>
        public override string ToString() => $"{Mark} - {Description}";
    }
}