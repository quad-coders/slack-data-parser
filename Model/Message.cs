using System;
using System.Linq;
using Newtonsoft.Json;

public class Message
{
    public string client_msg_id { get; set; }
    public string type { get; set; }
    public string text { get; set; }
    public FileAttachment[] files { get; set; }
    public string upload { get; set; }
    public string user { get; set; }
    public bool display_as_bot { get; set; }
    public string upload_reply_to { get; set; }
    [JsonConverter(typeof(EpochConverter))]
    public DateTime ts { get; set; }
    public string inviter { get; set; }
    public string subtype { get; set; }
    public string purpose { get; set; }
    public string filesName
    {
        get
        {
            if (files == null)
            {
                return string.Empty;
            }

            return this.files.Select(x => x.name).Aggregate((a, b) => a + ", " + b);
        }
    }
}