using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XHApp.Models
{
    /// <summary>
    /// 小英的Token回执
    /// </summary>
    public class EnglishAssistantAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(".issued")]
        public DateTimeOffset Issued { get; set; }

        [JsonProperty(".expires")]
        public DateTimeOffset Expires { get; set; }
    }
}
