using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbyExodus
{
    public class Config
    {
        public string embyUrlBase { get; set; }
        public string embyApiKey { get; set; }

        public string jellyfinUrlBase { get; set; }
        public string jellyfinApiKey { get; set; }

        //Constructor that loads config from an ini file
        public Config(string path)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }
                var key = parts[0];
                var value = parts[1];
                switch (key)
                {
                    case "embyUrlBase":
                        embyUrlBase = value;
                        break;
                    case "embyApiKey":
                        embyApiKey = value;
                        break;
                    case "jellyfinUrlBase":
                        jellyfinUrlBase = value;
                        break;
                    case "jellyfinApiKey":
                        jellyfinApiKey = value;
                        break;
                }
            }
        }

        //Constructor that loads config from input
        public Config(string embyUrlBase, string embyApiKey, string jellyfinUrlBase, string jellyfinApiKey)
        {
            this.embyUrlBase = embyUrlBase;
            this.embyApiKey = embyApiKey;
            this.jellyfinUrlBase = jellyfinUrlBase;
            this.jellyfinApiKey = jellyfinApiKey;
        }


    }

}
