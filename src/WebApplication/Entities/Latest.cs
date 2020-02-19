using System.Text.Json.Serialization;

namespace WebApplication.Entities
{
    /*
     * The API tester wants to set a `latest` value,
     * that can then be queried to see that requests
     * are actually handled. This row is used for that -
     * it is essentially a singleton. I'm sorry -
     * it is part of the requirements.
     */

    public class Latest
    {
        [JsonIgnore]
        public int id { get; set; }
        public int latest { get; set; }
    }
}
