using Autodesk.Revit.UI;
using Core;
using Database;
using License.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace License
{
    public static class Licenser
    {
        private static BTDatabase BtBase { get; set; } = new BTDatabase();
        private static LicenseSetting _lisenceSettig;
        public static bool CheckPassword(string operationName = "")
        {        
            bool ret = ValidatePassword();
            _lisenceSettig = JsonSetting<LicenseSetting>.Open();
            if (!ret)
            {
                new LicenseWindow().ShowDialog();
                ret = ValidatePassword(false);
            }
            else if (operationName != string.Empty)
            {
                StatsInfo operationStat = _lisenceSettig.StatsInfos.Where(x => x.OperationName == operationName && x.DateTime == DateTime.Now.Date)
                                                                  ?.FirstOrDefault();

                if(operationStat is null)
                {
                    operationStat = _lisenceSettig.StatsInfos.Where(x => x.OperationName == operationName)
                                                             .FirstOrDefault();

                    if (operationStat is StatsInfo)
                    {
                        _lisenceSettig.StatsInfos.Remove(operationStat);
                    }

                    operationStat = new StatsInfo()
                    {
                        OperationName = operationName,
                        DateTime = DateTime.Now.Date
                    };
                    _lisenceSettig.StatsInfos.Add(operationStat);
                    JsonSetting<LicenseSetting>.Save(_lisenceSettig);
                    BtBase.UpdateStatAsync(_lisenceSettig.CompanyName, operationName);
                }
            }

            return ret;
        }

        public static void ClearStatsInfos()
        {
            _lisenceSettig = JsonSetting<LicenseSetting>.Open();
            _lisenceSettig.StatsInfos = new List<StatsInfo>();
            JsonSetting<LicenseSetting>.Save(_lisenceSettig);
        }

        public static bool ValidatePassword(bool showErrorWindow = true)
        {
            _lisenceSettig = JsonSetting<LicenseSetting>.Open();

            return _lisenceSettig.Password == BtBase.GetSecret() || RetriveLicenceData(showErrorWindow);
        }

        private static bool RetriveLicenceData(bool showErrorWindow)
        {
            bool ret;
            try
            {
                ret = _lisenceSettig.Password == BtBase.GetAuth(_lisenceSettig.CompanyName)?.Password;
            }
            catch (Exception ex)
            {
                ret = false;
                TaskDialog taskDialog;
                if (ex is AggregateException || ex is HttpRequestException || ex is WebException)
                {
                    taskDialog = new TaskDialog(Resources.Error)
                    {
                        MainContent = Resources.NoInternetConnectionErrorText,
                        MainInstruction = Resources.NoInternetConnection
                    };
                }
                else
                {
                    taskDialog = new TaskDialog(Resources.Error)
                    {
                        MainContent = Resources.UnexpectedErrorHasOccurred,
                        MainInstruction = Resources.GoToBTForFixError
                    };
                }

                if (showErrorWindow)
                {
                    taskDialog.Show();
                }
            }

            return ret;
        }
    }
}
