using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PloppableRICO
{
    public interface ISteamDataProvider
    {
        SteamData getSteamData( string steamId );
    }

    public class SteamData
    {
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
    }
}
