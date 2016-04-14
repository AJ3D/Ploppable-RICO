using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PloppableRICO
{
 
    public enum Category
    {
        Monument,
        Beautification
    }

    public class CategoryIcons
    {

        public static readonly string[] atlases = {"Ingame", "Ingame" };

        public static readonly string[] spriteNames = { "ToolbarIconMonuments", "ToolbarIconBeautification" };

        public static readonly string[] tooltips = {"Monuments","Beautification" };
    }
}