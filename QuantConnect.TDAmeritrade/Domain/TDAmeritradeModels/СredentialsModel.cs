using Newtonsoft.Json;

namespace QuantConnect.TDAmeritrade.Domain.TDAmeritradeModels
{
    public class СredentialsModel
    {
        [JsonProperty(PropertyName = "userid")]
        public string Userid { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "company")]
        public string Company { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "segment")]
        public string Segment { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "cddomain")]
        public string Cddomain { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "usergroup")]
        public string Usergroup { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "accesslevel")]
        public string Accesslevel { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "authorized")]
        public string Authorized { get; set; } = "Y";

        [JsonProperty(PropertyName = "timestamp")]
        public double Timestamp { get; set; }

        [JsonProperty(PropertyName = "appid")]
        public string Appid { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "acl")]
        public string Acl { get; set; } = string.Empty;

        public СredentialsModel()
        { }

        public СredentialsModel(string userid, string token, string company, string segment, string cddomain, string usergroup, string accesslevel,
            double timestamp, string appid, string acl)
        {
            Userid = userid;
            Token = token;
            Company = company;
            Segment = segment;
            Cddomain = cddomain;
            Usergroup = usergroup;
            Accesslevel = accesslevel;
            Timestamp = timestamp;
            Appid = appid;
            Acl = acl;
        }
    }
}
