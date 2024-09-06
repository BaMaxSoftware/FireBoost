using System;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using FireBoost.Properties;
using License;
using Autodesk.Revit.DB;
using FireBoost.ExternalCommands;

namespace FireBoost
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalApplication : IExternalApplication
    {
        private readonly string _tabName = "FP Boost";
        private RibbonPanel _panel;

        /// <summary>
        /// Implements the on Shutdown event
        /// </summary>
        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        /// <summary>
        /// Implements the OnStartup event
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            try 
            {
                application.CreateRibbonTab(_tabName);
            }
            catch 
            {
                return Result.Cancelled;
            }

            if (TryCreateRibbonPanel(application, "Проходки"))
            {
                AddButton(typeof(SelectionCommand),
                    "Selection",
                    "Подбор",
                    "Создание экземпляров огнестойких проходок, на основе выбранных исходных данных.",
                    Resources.PenetrationSeal4);
                AddButton(typeof(SpecificationsCommand),
                    "Specifications",
                    "Спецификации",
                    "Копирование спецификации из выбранного шаблона.",
                    Resources.Schedules);
                AddButton(typeof(ParametersCommand),
                    "Parameters",
                    "Настройки параметров",
                    "Настройки копирования значений из общих параметров проходок в параметры проекта.",
                    Resources.Parameters);
                AddButton(typeof(ManagerCommand),
                    "Manager",
                    "Менеджер",
                    "Список всех существующих экземпляров огнестойких проходок с отображением их статуса.",
                    Resources.Manager);
            }
            if (TryCreateRibbonPanel(application, "Лицензия"))
            {
                AddButton(typeof(LicenseUI),
                    "License",
                    "Ключ",
                    "Задать ключ лицензии.",
                    Resources.LicenseLogo);
            }
            return Result.Succeeded;
        }

        /// <summary></summary>
        private bool TryCreateRibbonPanel(UIControlledApplication a, string ribbonPanel)
        {
            try
            {
                _panel = a.CreateRibbonPanel(_tabName, ribbonPanel);
            }
            catch
            {
                _panel = null;
            }
            return _panel != null;
        }

        /// <summary></summary>
        private void AddButton(Type assemblyType, string name, string text, string toolTip, Bitmap bitmap)
        {
            _panel.AddItem(new PushButtonData(name, text, assemblyType.Assembly.Location, $"{assemblyType.Namespace}.{assemblyType.Name}")
            { 
                ToolTip = toolTip,
                LargeImage = BitmapToSource(bitmap),
            });
        }

        private BitmapSource BitmapToSource(Bitmap bitmap) => Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }
}
