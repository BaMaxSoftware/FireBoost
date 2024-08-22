using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using FireBoost.Properties;

namespace FireBoost
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalApplication : IExternalApplication
    {
        private readonly string _thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
        private RibbonPanel panel;
        /// <summary>
        /// Implements the on Shutdown event
        /// </summary>
        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        /// <summary>
        /// Implements the OnStartup event
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            if (TryCreateRibbonPanel(application, out panel, "FP Boost", "Проходки"))
            {
                AddButton(
                    "SelectionButton",
                    "FireBoost.ExternalCommands.SelectionCommand",
                    "Подбор",
                    "Создание экземпляров огнестойких проходок, на основе выбранных исходных данных.",
                    Resources.PenetrationSeal4);
                AddButton(
                    "SpecificationsButton",
                    "FireBoost.ExternalCommands.SpecificationsCommand",
                    "Спецификации",
                    "Копирование спецификации из выбранного шаблона.",
                    Resources.Schedules);
                AddButton(
                    "ParametersButton",
                    "FireBoost.ExternalCommands.ParametersCommand",
                    "Настройки параметров",
                    "Настройки копирования значений из общих параметров проходок в параметры проекта.",
                    Resources.Parameters);
                AddButton(
                    "ManagerButton",
                    "FireBoost.ExternalCommands.ManagerCommand",
                    "Менеджер",
                    "Список всех существующих экземпляров огнестойких проходок с отображением их статуса.",
                    Resources.Manager);
            }

            return Result.Succeeded;
        }

        /// <summary></summary>
        public bool TryCreateRibbonPanel(UIControlledApplication a, out RibbonPanel panel, string tabName, string ribbonPanel)
        {
            try
            {
                a.CreateRibbonTab(tabName);
                panel = a.CreateRibbonPanel(tabName, ribbonPanel);
                return true;
            }
            catch
            {
                panel = default;
                return false;
            }
        }

        /// <summary></summary>
        private void AddButton(string btnName, string btnClassName, string btnTxt, string tooltip, Bitmap img, string longTooltip = null)
        {
            PushButton button = panel.AddItem(new PushButtonData(btnName, btnTxt, _thisAssemblyPath, btnClassName)) as PushButton;
            button.ToolTip = tooltip;
            button.LargeImage = BitmapToImageSource(img);
            if (longTooltip != null)
            { 
                button.LongDescription = longTooltip;
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            BitmapImage bitmapimage;
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
            }
            return bitmapimage;
        }
    }
}
