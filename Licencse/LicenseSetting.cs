using Newtonsoft.Json;
using System.Collections.Generic;

namespace License
{
    public class LicenseSetting : Core.IJsonSetting
    {
        [JsonIgnore]
        public string Filename => "Licencse.json";
        [JsonProperty("CompanyName")]
        public string CompanyName { get; set; } = "";
        [JsonProperty("Password")]
        public string Password { get; set; } = "";
        [JsonProperty("StatsInfos")]
        public List<StatsInfo> StatsInfos { get; set; } = new List<StatsInfo>();
    }
}
