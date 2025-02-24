using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DivineSkies.Modules.SaveGame
{
    public class SaveGameBase
    {
        public long CreationTimeStamp;

        public SaveGameBase()
        {
            CreationTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new StringEnumConverter());
        }
    }
}
