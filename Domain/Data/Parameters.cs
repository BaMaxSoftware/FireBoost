using System;

namespace FireBoost.Domain.Data
{
    /// <summary></summary>
    public class Parameters
    {
        /// <summary></summary>
        public string CableTrayHeight { get; } = "Высота лотка 1";
        /// <summary></summary>
        public string CableTrayWidth { get; } = "Ширина лотка 1";
        /// <summary></summary>
        public string DuctHeight { get; } = "Высота воздуховода 1";
        /// <summary></summary>
        public string DuctWidth { get; } = "Ширина воздуховода 1";
        /// <summary></summary>
        public string MepDiameter { get; } = "Диаметр коммуникации 1";
        /// <summary></summary>
        public string MepDiameter2 { get; } = "Диаметр круглых коммуникаций 1";
        /// <summary></summary>
        public string MepDiameter3 { get; } = "Диаметр круглых коммуникаций";
        /// <summary></summary>
        public string MepHeight { get; } = "Высота модуля 1";
        /// <summary></summary>
        public string MepWidth { get; } = "Ширина модуля 1";

        /// <summary>
        /// Внутренний диаметр отверстия
        /// </summary>
        public Guid OpeningDiameter { get; } = new Guid("fac76ca8-0f8a-4b5f-91ff-018aff5bad25");
        /// <summary>
        /// Высота проема
        /// </summary>
        public Guid OpeningHeight { get; } = new Guid("43b69be4-81ef-4528-95dc-6f5dd4d1c041");
        /// <summary>
        /// Ширина проема
        /// </summary>
        public Guid OpeningWidth { get; } = new Guid("45b14688-97e2-4f09-ba8a-2ddc1bd5cbf3");
    }
}
