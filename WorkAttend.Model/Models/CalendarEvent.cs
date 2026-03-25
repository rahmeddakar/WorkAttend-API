using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace WorkAttend.Model.Models
{
    public class CalendarEvent
    {
        [JsonProperty("eventId")]
        public int eventId { get; set; }
        [JsonProperty("locationId")]
        public int locationId { get; set; }
        [JsonProperty("title")]
        public string title { get; set; }
        [JsonProperty("start")]
        public string start { get; set; }
        [JsonProperty("end")]
        public string end { get; set; }
    }
}