using ICities;
using UnityEngine;


namespace PloppableRICO
{
	public class PloppableRICOMod : IUserMod
	{
		public string Name
		{
			get
			{
                Util.TRACE( "HELLO" );
                return "Ploppable RICO";
			}
		}
		public string Description
		{
			get
			{
                Util.TRACE( "WORLD" );
                return "Allows Plopping of RICO Buildings";
			}
		}
	}
}
