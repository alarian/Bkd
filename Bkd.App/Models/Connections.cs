using System.Collections.Generic;

namespace Bkd.App.Repository
{
    public class Connections : Dictionary<string, string>
    {
        public Connections()
        {
            this["Avid"] = "Server=DESKTOP-EP1A1V5;Database=AvidWebsite;Trusted_Connection=True";
        }
    }
}