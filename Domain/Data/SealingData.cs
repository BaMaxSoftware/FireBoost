using Autodesk.Revit.DB;
using FireBoost.Domain.Entities;
using FireBoost.Domain.Enums;

namespace FireBoost.Domain.Data
{
    /// <summary></summary>
    public class SealingData
    {
        /// <summary></summary>
        public SealingHost[] CreateHostsTypesArray() => new SealingHost[]
        {
            new SealingHost("Стена", BuiltInCategory.OST_Walls, 1),
            new SealingHost("Перекрытие", BuiltInCategory.OST_Floors, 2),
            new SealingHost("Стена - импорт из IFC", BuiltInCategory.OST_GenericModel, 1),
            new SealingHost("Перекрытие - импорт из IFC", BuiltInCategory.OST_GenericModel, 2),
        };

        /// <summary></summary>
        public SealingMEPType[] CreateMEPTypesArray() => new SealingMEPType[]
        {
            new SealingMEPType(1, "1 - Без закладных", 1, "Нет коммуникаций", new BuiltInCategory[] { }),
            new SealingMEPType(1, "1 - Без закладных", 1, "Кабели без закладных",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "2 - Закладные гильзы/ячеистые модули", 2, "Кабели в закладных трубах",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "2 - Закладные гильзы/ячеистые модули", 2, "Закладная труба без коммуникаций",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "2 - Закладные гильзы/ячеистые модули", 2, "Кабели в закладных ячеистых модулях",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "2 - Закладные гильзы/ячеистые модули", 2, "Ячеистые закладные модули без коммуникаций",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "3-6 - Кабели в лотках/коробах", 3, "Кабели в листовом неперфорированном лотке или коробе КП",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "3-6 - Кабели в лотках/коробах", 3, "Кабели в коробе ККБ",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "3-6 - Кабели в лотках/коробах", 4, "Кабели в листовом перфорированном лотке",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "3-6 - Кабели в лотках/коробах", 5, "Кабели в проволочном лотке",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(1, "3-6 - Кабели в лотках/коробах", 6, "Кабели в лестничном лотке",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(2, "7 - Шинопроводы", 7, "Шинопроводы без фаербарьеров",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(2, "7 - Шинопроводы", 7, "Шинопроводы с фаербарьерами",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit
                }),
            new SealingMEPType(3, "8 - Трубопроводы", 8, "Трубопроводы металлические",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_PipeCurves
                }),
            new SealingMEPType(3, "8 - Трубопроводы", 8, "Трубопроводы металлические в негорючей теплоизоляции",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_PipeCurves
                }),
            new SealingMEPType(3, "8 - Трубопроводы", 8, "Трубопроводы металлические в горючей теплоизоляции",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_PipeCurves
                }),
            new SealingMEPType(4, "9 - Воздуховоды/газоходы", 9, "Воздуховоды в теплоогнезащитной негорючей изоляции",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_DuctCurves
                }),
            new SealingMEPType(4, "9 - Воздуховоды/газоходы", 9, "Газоходы круглые",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_DuctCurves
                }),
            new SealingMEPType(5, "10 - Комбинированные проходки", 10,"Комбинированные коммуникации с токопроводящими изделиями",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_DuctCurves,
                }),
            new SealingMEPType(5, "10 - Комбинированные проходки", 10, "Комбинированные коммуникации без токопроводящих изделий",
                new BuiltInCategory[]
                {
                    BuiltInCategory.OST_CableTray,
                    BuiltInCategory.OST_Conduit,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_DuctCurves,
                })
        };

        /// <summary></summary>
        public SealingFireResistance[] CreateFireResistancesArray() => new SealingFireResistance[]
        {
            new SealingFireResistance(45, "45 минут - глубина заделки 60 мм", 60),
            new SealingFireResistance(60, "60 минут - глубина заделки 100 мм", 100),
            new SealingFireResistance(90, "90 минут - глубина заделки 120 мм", 120),
            new SealingFireResistance(120, "120 минут - глубина заделки 150 мм", 150),
            new SealingFireResistance(150, "150 минут - глубина заделки 190 мм", 190),
            new SealingFireResistance(180, "180 минут - глубина заделки 300 мм", 300)
        };

        /// <summary></summary>
        public Entities.SealingMaterial[] CreateMaterialsArray() => new Entities.SealingMaterial[]
        {
            new Entities.SealingMaterial("П", "Пена", SealingMaterialType.P),
            new Entities.SealingMaterial("М", "Минеральная плита", SealingMaterialType.M),
            new Entities.SealingMaterial("ПМ", "Пена и минеральная плита", SealingMaterialType.PM)
        };

        /// <summary></summary>
        public Entities.SealingStructuralDesign[] CreateStructuralDesignsArray() => new SealingStructuralDesign[]
        {
            new Entities.SealingStructuralDesign("(1)", "Односторонняя заделка проходки", SealingStructuralDesignType.OneSide),
            new Entities.SealingStructuralDesign("(2)", "Двухсторонняя заделка проходки", SealingStructuralDesignType.TwoSided),
            new Entities.SealingStructuralDesign("(М)", "С установкой кабельных разделительных металлорешеток", SealingStructuralDesignType.M),
            new Entities.SealingStructuralDesign("(Т)", "С установкой узла увеличения глубины проемов", SealingStructuralDesignType.T),
            new Entities.SealingStructuralDesign("(Э)", "Эксплуатируемая сборно-разборная заделка проходки", SealingStructuralDesignType.E),
            new Entities.SealingStructuralDesign("(Р)", "Резервная заделка проходки без коммуникаций", SealingStructuralDesignType.R)
        };

        /// <summary></summary>
        public SealingShape[] CreateShapesArray() => new SealingShape[]
        {
            new SealingShape("Круг", SealingShapeType.Round),
            new SealingShape("Прямоугольник", SealingShapeType.Reachtangle),
        };
    }
}