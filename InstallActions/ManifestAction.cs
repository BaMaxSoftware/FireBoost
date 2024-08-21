using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.RevitAddIns;
using Microsoft.Deployment.WindowsInstaller;

namespace InstallActions
{
    public class ManifestAction
    {
        [CustomAction]
        public static ActionResult CreateManifest(Session session)
        {
            const string manifestName = @"\FireBoost.addin";
            List<RevitProduct> revitProducts = RevitProductUtility.GetAllInstalledRevitProducts();

            string installPath = session.CustomActionData["InstallPath"];
            string version = session.CustomActionData["Version"];

            RevitAddInManifest manifest = new RevitAddInManifest();

            //create an external application
            RevitAddInApplication application = new RevitAddInApplication("Bimteam AddIn", installPath + @"FireBoost.dll", Guid.NewGuid(), "FireBoost.ExternalApplication", "FireBoost")
            {
                Name = "FireBoost addin",
                VendorDescription = "bimteam",
                ProductVersion = version,
            };

            manifest.AddInApplications.Add(application);

            foreach(RevitProduct revitProduct in revitProducts)
            {
                switch (revitProduct.Version)
                {
                    case RevitVersion.Unknown:
                        continue;
                    case RevitVersion.Revit2019:
                    case RevitVersion.Revit2020:
                    case RevitVersion.Revit2021:
                    case RevitVersion.Revit2022:
                    case RevitVersion.Revit2023:
                        if (File.Exists(revitProduct.AllUsersAddInFolder + manifestName))
                        {
                            File.Delete(revitProduct.AllUsersAddInFolder + manifestName);
                        }

                        if (Directory.Exists(revitProduct.AllUsersAddInFolder))
                        {
                            manifest.SaveAs(revitProduct.AllUsersAddInFolder + manifestName);
                        }
                break;
                    default:
                        break;
                }
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult DeleteManifest(Session session)
        {
            const string manifestName = @"\FireBoost.addin";

            List<RevitProduct> revitProducts = RevitProductUtility.GetAllInstalledRevitProducts();

            foreach (RevitProduct revitProduct in revitProducts)
            {
                if (revitProduct.Version == RevitVersion.Unknown)
                {
                    continue;
                }
                if (File.Exists(revitProduct.AllUsersAddInFolder + manifestName))
                {
                    File.Delete(revitProduct.AllUsersAddInFolder + manifestName);
                }
            }
            return ActionResult.Success;
        }
    }
}
