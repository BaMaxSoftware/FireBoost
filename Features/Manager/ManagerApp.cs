using Autodesk.Revit.DB;

namespace FireBoost.Features.Manager
{
    internal class ManagerApp
    {
        public Document Document { get; }
        private ManagerWindow _view;
        public ManagerApp(Document document) 
        {
            Document = document;
        }

        public void Run()
        {
            _view = new ManagerWindow()
            { 
                DataContext = new ManagerVM(Document)
            };
            
            _view.Show();
        }
    }
}
