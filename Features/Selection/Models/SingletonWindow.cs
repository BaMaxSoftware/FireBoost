using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Selection.Views;
using System.Windows;

namespace FireBoost.Features.Selection.Models
{
    internal class SingletonWindow
    {
        private static MainWindow instance;

        private SingletonWindow()
        { }

        public static void Show(SelectionVM vm)
        { 
            (IsValidWindow() ? instance : (instance = new MainWindow(vm))).Show();
            if (!instance.IsFocused)
                instance.Focus();
        }

        public static void SetVisibility(Visibility visibility)
        {
            if (IsValidWindow())
                instance.Visibility = visibility;
        }

        public static void Close()
        {
            if (IsValidWindow())
                instance.Close();
        }

        private static bool IsValidWindow() => instance != null && instance.IsLoaded;
    }
}
