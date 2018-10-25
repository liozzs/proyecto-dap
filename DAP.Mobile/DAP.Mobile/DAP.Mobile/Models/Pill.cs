using Newtonsoft.Json;

namespace DAP.Mobile.Models
{
    public class Pill : Entity
    {
        [JsonProperty(PropertyName = "pillName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "plateID")]
        public int Container { get; set; }

        [JsonProperty(PropertyName = "stock")]
        public int Quantity { get; set; }
    }
}