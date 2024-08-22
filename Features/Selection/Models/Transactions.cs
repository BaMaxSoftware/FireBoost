using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using FireBoost.Domain.Enums;
using FireBoost.Domain;
using System;
using FireBoost.Domain.Data;

namespace FireBoost.Features.Selection.Models
{
    internal class Transactions
    {
        private Document _doc { get; }
        private Parameters _parameters { get; set; }

        public Transactions(Document doc)
        {
            _parameters = new Parameters();
            _doc = doc;
        }

        public TransactionStatus Regenerate()
        {
            TransactionStatus ret = TransactionStatus.Uninitialized;
            using (Transaction t = new Transaction(_doc))
            {
                try
                {
                    t.Start("Обновление активного проекта");
                    _doc.Regenerate();
                    t.Commit();
                }
                catch
                {
                    if (t.HasStarted())
                        t.RollBack();
                }
                ret = t.GetStatus();
            }
            return ret;
        }

        public TransactionStatus LoadFamilySymbol(string rfa, string typeName, ErrorsHandler errorsHandler, out FamilySymbol symbol)
        {
            TransactionStatus result = TransactionStatus.Uninitialized;
            using (Transaction t = new Transaction(_doc))
            {
                try
                {
                    t.Start("Загрузка семейства");
                    bool b = _doc.LoadFamilySymbol(rfa, typeName, out symbol);
                    _doc.Regenerate();
                    t.Commit();
                    result = t.GetStatus();
                }
                catch (Exception e)
                {
                    result = TransactionStatus.Error;
                    symbol = null;
                    if (t.HasStarted())
                        t.RollBack();
                        
                    errorsHandler.ShowQuestion(e.Message);
                }
            }

            return result;
        }

        /// <summary></summary>
        public FamilyInstance CreateNewInstance(FamilySymbol _familySymbol, XYZ locationPoint, Level level)
        {
            FamilyInstance instance = default;
            using (Transaction t = new Transaction(_doc, "Компонент"))
            {
                t.Start();
                if (!_familySymbol.IsActive) _familySymbol.Activate();

                instance = _doc.Create.NewFamilyInstance(
                    locationPoint,
                    _familySymbol,
                    level,
                    level,
                    StructuralType.NonStructural);
                t.Commit();

                if (instance != null && instance.IsValidObject)
                {
                    t.Start();
                    var p = instance.LookupParameter("Статус");
                    if (p != null && !p.IsReadOnly && p.StorageType == StorageType.String)
                    {
                        p.Set("Не проверено");
                    }
                    t.Commit();
                }
            }

            return instance != default && !instance.IsValidObject ? default : instance;
        }

        /// <summary></summary>
        public void ChangeInstanceElevation(ref FamilyInstance instance, double elev)
        {
            if (instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM) is Parameter hostOffset)
            {
                using (Transaction t = new Transaction(_doc, "Редактировать атрибуты элемента"))
                {
                    t.Start();
                    hostOffset.Set(elev);
                    t.Commit();
                }
            }
        }

        public void ChangeJoinOpeningSize(Document doc, ref FamilyInstance newInstance, double height, double width)
        {
            using (Transaction t = new Transaction(doc))
            {
                try
                { 
                    t.Start("Изменение размеров");
                    newInstance.get_Parameter(_parameters.OpeningHeight).Set(height);
                    newInstance.get_Parameter(_parameters.OpeningWidth).Set(width);
                    t.Commit();
                }
                catch 
                {
                    if (t.HasStarted())
                        t.RollBack();
                }
            }
        }

        public void ChangeOpeningsDimensions(OpeningShape shape, ref FamilyInstance instance, double _height, double _width, double _diameter)
        {
            using (Transaction t = new Transaction(_doc))
            {
                try
                {
                    t.Start("Изменение размеров");
                    switch (shape)
                    {
                        case OpeningShape.Reachtangle:
                            instance.get_Parameter(_parameters.OpeningHeight).Set(_height);
                            instance.get_Parameter(_parameters.OpeningWidth).Set(_width);
                            break;
                        case OpeningShape.Round:
                            instance.get_Parameter(_parameters.OpeningDiameter).Set(_diameter);
                            break;
                    }
                    t.Commit();
                }
                catch
                { 
                    if (t.HasStarted())
                        t.RollBack();
                }
            }
        }

        public void RotateInstance(ref FamilyInstance instance, Line axis, double angle)
        {
            using (Transaction t = new Transaction(_doc))
            {
                try
                {
                    t.Start("Поворот элемента");
                    instance.Location.Rotate(axis, angle);
                    t.Commit();
                }
                catch
                {
                    if (t.HasStarted())
                        t.RollBack();
                }
            }
        }

        public void Move(ref FamilyInstance instance, XYZ vector)
        {
            
            using (Transaction t = new Transaction(_doc))
            {
                try
                {
                    t.Start("Смещение элемента");
                    instance.Location.Move(vector);
                    t.Commit();
                }
                catch 
                {
                    if (t.HasStarted())
                        t.RollBack();
                }
            }
        }

        public void ChangeOtherParams(ref FamilyInstance newInstance, string duration, string minutes, SealingMaterials material)
        {
            using (Transaction t = new Transaction(_doc))
            {
                t.Start("Редактировать атрибуты элемента");
                Parameter durationParameter = newInstance.get_Parameter(new Guid("ea2d4cab-8cba-43f6-bcfa-72003c13fd65"));
                Parameter fireParamParameter = newInstance.get_Parameter(new Guid("51548b59-1bdb-4692-ab6e-bbb4a5423aa0"));
                if (durationParameter != null && !durationParameter.IsReadOnly && fireParamParameter != null && !fireParamParameter.IsReadOnly)
                {
                    durationParameter.SetValueString(duration);
                    fireParamParameter.SetValueString(minutes);
                }
                if (material == SealingMaterials.PM)
                {
                    Parameter materialParameter = newInstance.LookupParameter("Минплита");
                    if (materialParameter != null && !materialParameter.IsReadOnly)
                    {
                        materialParameter.Set(1);
                    }
                }
                t.Commit();
            }
        }

        public void ChangeSelectedParams(Document doc, (Parameter Parameter, double Value)[] arr)
        {
            if (arr == null || arr.Length == 0)
                return;

            using (TransactionGroup tg = new TransactionGroup(doc))
            {
                try
                {
                    Transaction t;
                    string name = "SetOtherParams";
                    tg.Start(name);
                    foreach (var item in arr)
                    {
                        if (item.Parameter != null && !item.Parameter.IsReadOnly && item.Parameter.StorageType == StorageType.Double)
                        {
                            using (t = new Transaction(doc))
                            {
                                try
                                {
                                    t.Start(name);
                                    item.Parameter.Set(item.Value);
                                    t.Commit();
                                }
                                catch
                                {
                                    if (t.HasStarted())
                                        t.RollBack();
                                }
                            }
                        }
                    }
                    tg.Commit();
                }
                catch
                {
                    if (tg.HasStarted())
                        tg.RollBack();
                }
            }
        }
    }
}


