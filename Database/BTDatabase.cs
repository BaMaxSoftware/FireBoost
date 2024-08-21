using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;

namespace Database
{
    public class BTDatabase
    {
        private readonly IFirebaseClient _client;
        private const string metricNodeName = "CompanyMetric";

        public BTDatabase() => _client = new FirebaseClient(new FirebaseConfig
        {
            AuthSecret = BaseConfig.AuthSecret,
            BasePath = BaseConfig.BasePath
        });

        public AuthParam GetAuth(string companyName) => _client.Get(companyName).ResultAs<AuthParam>();

        public async void UpdateStatAsync(string companyName, string operationName)
        {
            int curentOperationCount = 0;
            string now = DateTime.Now.Date.ToString("dd-MM-yyyy");
            FirebaseResponse comapnymetric = await _client.GetAsync($"{metricNodeName}/{companyName}/{now}/{operationName}");
            if (comapnymetric.Body != null && comapnymetric.Body != "null")
            {
                curentOperationCount = comapnymetric.ResultAs<int>();
            }
            await _client.SetAsync($"{metricNodeName}/{companyName}/{now}/{operationName}", ++curentOperationCount);
        }

        public string GetSecret() => BaseConfig.Secret;
    }
}
