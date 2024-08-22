using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Features.ParametersCopy.ViewModels;
using FireBoost.Features.ParametersCopy.Views;

namespace FireBoost.Features.ParametersCopy
{
    internal class ParametersApp
    {
        public Document Document { get; }
        public UIDocument UIDocument { get; }
        public UIApplication UIApplication { get; }
        public Application Application { get; }
        public ParametersApp(UIApplication uiapp)
        { 
            UIApplication = uiapp;
            Application = uiapp.Application;
            UIDocument = uiapp.ActiveUIDocument;
            Document = UIDocument.Document;
        }


        public void Run()
        {
            var bindings = Document.ParameterBindings;
            string[] projectParameters = new string[bindings.Size];
            if (bindings.Size > 0)
            { 
                var fi = bindings.ForwardIterator();
                fi.Reset();

                for (int i = 0; i < bindings.Size; i++)
                { 
                    fi.MoveNext();
                    projectParameters[i]= fi.Key.Name;
                }
            }

            ParametersVM _vm = new ParametersVM(Document.Title, projectParameters);
            ParametersWindow _view = new ParametersWindow() { DataContext = _vm };
            _view.ShowDialog();
        }
    }
}
