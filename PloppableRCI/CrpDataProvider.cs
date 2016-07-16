using System;
using System.IO;

// Boy, that took a while to debug. Looks like Unity croaks if you use
// using System.Windows.Media.Imaging; <-- BAD
// Propably because it needs an extra reference to PresentationCore.dll

namespace PloppableRICO
{
    public interface ICrpDataProvider
    {
        CrpData getCrpData( string file );
        CrpData getCrpData( FileInfo file );
    }
    
    public class CrpData
    {
        public Object PreviewImage { get; set; }
        public string Tags { get; set; }
        public string Type { get; set; }
        public string AuthorID { get; set; }
        public string SteamId { get; set; }
        public string AuthorName { get; set; }
        public string BuildingName { get; set; }
        public FileInfo sourceFile { get; set; }
    }
}
