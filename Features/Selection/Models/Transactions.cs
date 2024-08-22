using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using FireBoost.Domain.Enums;
using System;
using FireBoost.Domain.Data;
using System.Collections.Generic;
using System.Linq;

namespace FireBoost.Features.Selection.Models
{
    internal class Transactions
    {
        private Document Doc { get; }
        private Parameters _parameters { get; set; }

        public Transactions(Document doc)
        {
            _parameters = new Parameters();
            Doc = doc;
        }

        public TransactionStatus Regenerate()
        {
            TransactionStatus ret = TransactionStatus.Uninitialized;
            using (Transaction t = new Transaction(Doc))
            {
                try
                {
                    t.Start("Обновление активного проекта");
                    Doc.Regenerate();
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
            using (Transaction t = new Transaction(Doc))
            {
                try
                {
                    t.Start("Загрузка семейства");
                    bool b = Doc.LoadFamilySymbol(rfa, typeName, out symbol);
                    Doc.Regenerate();
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
            using (Transaction t = new Transaction(Doc, "Компонент"))
            {
                t.Start();
                if (!_familySymbol.IsActive) _familySymbol.Activate();

                instance = Doc.Create.NewFamilyInstance(
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
        public void ChangeInstanceElevation(ref FamilyInstance instance, double elevation)
        {
            if (instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM) is Parameter hostOffset)
            {
                using (Transaction t = new Transaction(Doc, "Редактировать атрибуты элемента"))
                {
                    t.Start();
                    hostOffset.Set(elevation);
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

        public void ChangeOpeningsDimensions(OpeningShape shape, ref FamilyInstance instance, double height, double width, double diameter)
        {
            using (Transaction t = new Transaction(Doc))
            {
                try
                {
                    t.Start("Изменение размеров");
                    switch (shape)
                    {
                        case OpeningShape.Reachtangle:
                            instance.get_Parameter(_parameters.OpeningHeight).Set(height);
                            instance.get_Parameter(_parameters.OpeningWidth).Set(width);
                            break;
                        case OpeningShape.Round:
                            instance.get_Parameter(_parameters.OpeningDiameter).Set(diameter);
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
            using (Transaction t = new Transaction(Doc))
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
            
            using (Transaction t = new Transaction(Doc))
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
            using (Transaction t = new Transaction(Doc))
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

        public void ChangeProjectParams(FamilyInstance instance, (string Name, string SelectedParameter)[] parametersArr)
        {
            Parameter p1, p2;
            Transaction t;
            var statuses = new List<TransactionStatus>();
            using (TransactionGroup tg = new TransactionGroup(Doc))
            {
                tg.Start();

                foreach (var (Name, SelectedParameter) in parametersArr)
                {
                    if (SelectedParameter == null)
                        continue;

                    p1 = instance.LookupParameter(Name);
                    if (p1 == null || p1.IsReadOnly)
                        continue;

                    p2 = instance.LookupParameter(SelectedParameter);
                    if (p2 == null || p2.IsReadOnly)
                        continue;

                    if (p1.StorageType != p2.StorageType)
                        continue;

                    using (t = new Transaction(Doc, "Редактировать атрибуты элемента"))
                    {
                        try
                        {
                            t.Start();
                            switch (p1.StorageType)
                            {
                                case StorageType.Double:
                                    p2.Set(p1.AsDouble());
                                    break;
                                case StorageType.ElementId:
                                    p2.Set(p1.AsElementId());
                                    break;
                                case StorageType.Integer:
                                    p2.Set(p1.AsInteger());
                                    break;
                                case StorageType.String:
                                    p2.Set(p1.AsString());
                                    break;
                                default: t.RollBack(); break;
                            }
                            if (!t.HasEnded())
                            { 
                                t.Commit();
                            }
                        }
                        catch
                        {
                            if (t.HasStarted())
                            {
                                t.RollBack();
                            }
                        }
                        statuses.Add(t.GetStatus());
                    }    
                }

                if (statuses.Any(x => x == TransactionStatus.Committed))
                {
                    tg.Assimilate();
                }
                else
                {
                    tg.RollBack();
                }
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


