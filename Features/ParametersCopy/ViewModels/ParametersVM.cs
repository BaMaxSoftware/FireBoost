using System;

namespace FireBoost.Features.ParametersCopy.ViewModels
{
    public class ParametersVM
    {
        public string ActiveDocTitle { get; }
        public string[] ProjectParameters { get; }
        public SharedParameter[] SharedParameters { get; }

        public ParametersVM(string title, string[] projectParams)
        {
            ActiveDocTitle = title;
            ProjectParameters = projectParams;
            if (true)
            {
                SharedParameters = GetDefaultParameters();
            }
        }

        private SharedParameter[] GetDefaultParameters() => new SharedParameter[]
        {
            new SharedParameter(new Guid("04d4b6fd-819f-4662-b2c4-8b079d75f634"), "Тип коммуникаций"),
            new SharedParameter(new Guid("51548b59-1bdb-4692-ab6e-bbb4a5423aa0"), "Предел огнестойкости"),
            new SharedParameter(new Guid("3349bc7a-b31f-4766-aa71-442095947d36"), "Материал заделки"),
            new SharedParameter(new Guid("db6bb413-2c3f-4ee8-9512-d707b8326d30"), "Конструктивное исполнение"),
            new SharedParameter(new Guid("e9716a58-f488-4ee8-8639-f04b4f0f7d40"), "Объем минплиты"),
            new SharedParameter(new Guid("2499f96f-ca47-4844-8f92-9668531b7908"), "Глубина заделки"),
            new SharedParameter(new Guid("4ad60a93-705c-4809-9ad8-948c1def52b7"), "Площадь минплиты"),
            new SharedParameter(new Guid("04badd0d-341b-4462-b92d-e9b53e39fdef"), "Количество герметика, шт"),
            new SharedParameter(new Guid("ea2d4cab-8cba-43f6-bcfa-72003c13fd65"), "Толщина ограждения"),
            new SharedParameter(new Guid("3ab21d17-74f9-4b7e-857e-587d2c8294bc"), "Суммарная площадь коммуникаций"),
            new SharedParameter(new Guid("e27f45ae-fd89-4d7e-9d88-ebe73ff179d2"), "Диаметр коммуникации 1"),
            new SharedParameter(new Guid("32e552d1-265e-4097-a7f3-5f4d20d903ec"), "Диаметр коммуникации 2"),
            new SharedParameter(new Guid("df54c453-9f66-429a-8e12-022e34181924"), "Диаметр коммуникации 3"),
            new SharedParameter(new Guid("9de4dc47-61e3-4820-a4c0-c9bc115c4608"), "Площадь ГКЛО"),
            new SharedParameter(new Guid("7d30b075-3007-4f93-ab85-2857233e059b"), "Количество шпаклевки"),
            new SharedParameter(new Guid("43b69be4-81ef-4528-95dc-6f5dd4d1c041"), "Высота проема"),
            new SharedParameter(new Guid("45b14688-97e2-4f09-ba8a-2ddc1bd5cbf3"), "Ширина проема"),
            new SharedParameter(new Guid("ce7c1a65-300f-40b3-87fd-2a0d969b13a6"), "Объем пеноблока"),
            new SharedParameter(new Guid("5ca852c1-a1fc-4076-be4e-a5f8ea9273e2"), "Объем пены"),
            new SharedParameter(new Guid("483dbc3e-060e-47e7-8e1e-b801a10cd869"), "Площадь пены"),
            new SharedParameter(new Guid("e38cef73-8d45-42f9-a94d-f266140ed414"), "Количество картриджей пены, шт"),
            new SharedParameter(new Guid("0892e8c4-2bf5-4d07-a140-569c8cb5fdf0"), "Количество герметика в кг"),
            new SharedParameter(new Guid("e020b744-a4fb-4fc7-91ab-37ea438af178"), "Объем пены, л"),
            new SharedParameter(new Guid("7a718b7f-5f50-485a-b433-5d2983f8b849"), "Площадь кабелей"),
            new SharedParameter(new Guid("23e49c4f-1c96-4dc4-9467-7feb7ce4d697"), "Процент кабелей"),
            new SharedParameter(new Guid("816cce36-8937-4da5-943c-ea093305bf4f"), "Диаметр отверстия в проходке"),
            new SharedParameter(new Guid("fac76ca8-0f8a-4b5f-91ff-018aff5bad25"), "Внутренний диаметр отверстия"),
        };

        public class SharedParameter
        { 
            public string Name { get; }
            public Guid GuidValue { get; }

            public SharedParameter(Guid guidValue, string name)
            {
                Name = name;
                GuidValue = guidValue;
            }
        }
    }
}
