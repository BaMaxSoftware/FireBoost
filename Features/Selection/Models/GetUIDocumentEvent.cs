﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FireBoost.Features.Selection.Models
{
    /// <summary></summary>
    public class GetUIDocumentEvent : IExternalEventHandler
    {
        /// <summary></summary>
        public UIDocument ActiveUIDocument { get; private set; }
        /// <summary></summary>
        public Document ActiveDocument { get; private set; }

        /// <summary></summary>
        public void Execute(UIApplication app)
        {
            ActiveUIDocument = app.ActiveUIDocument;
            ActiveDocument = ActiveUIDocument.Document;
        }

        /// <summary></summary>
        public string GetName() => "GetUIDocumentEvent";
    }
}
