using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PloppableAI
{
	public class BuildingDetoursHolder
	{
		private static bool deployed = false;

		private static RedirectCallsState _Building_setWidth_state;
		private static MethodInfo _Building_setWidth_original;
		private static MethodInfo _Building_setWidth_detour;

		private static RedirectCallsState _Building_setLength_state;
		private static MethodInfo _Building_setLength_original;
		private static MethodInfo _Building_setLength_detour;

		private static FieldInfo _Building_m_width;
		private static FieldInfo _Building_m_length;

		public static void Deploy() 
		{
			if (!deployed)
			{
				_Building_setWidth_original = typeof(Building).GetProperty("Width").GetSetMethod();
				_Building_setWidth_detour = typeof(BuildingDetoursHolder).GetProperty("Width").GetSetMethod();
				_Building_setWidth_state = RedirectionHelper.RedirectCalls(_Building_setWidth_original, _Building_setWidth_detour);

				_Building_setLength_original = typeof(Building).GetProperty("Length").GetSetMethod();
				_Building_setLength_detour = typeof(BuildingDetoursHolder).GetProperty("Length").GetSetMethod();
				_Building_setLength_state = RedirectionHelper.RedirectCalls(_Building_setLength_original, _Building_setLength_detour);

				_Building_m_width = typeof(Building).GetField("m_width");
				_Building_m_length = typeof(Building).GetField("m_length");

				deployed = true;

				Debug.Log("LargerFootprints: Methods detoured!");
			}
		}

		public static void Revert() 
		{
			if (deployed) 
			{
				RedirectionHelper.RevertRedirect(_Building_setWidth_original, _Building_setWidth_state);
				_Building_setWidth_original = null;
				_Building_setWidth_detour = null;

				RedirectionHelper.RevertRedirect(_Building_setLength_original, _Building_setLength_state);
				_Building_setLength_original = null;
				_Building_setLength_detour = null;

				_Building_m_width = null;
				_Building_m_length = null;

				deployed = false;

				Debug.Log("LargerFootprints: Methods restored!");
			}
		}

		public int Width 
		{
			set 
			{
				

				Debug.LogFormat("setWidth called: value = {0}", value);
			}
		}

		public int Length
		{
			set
			{
				//_Building_m_length.SetValue(this, (byte)value);

				Debug.LogFormat("setLength called: value = {0}", value);
			}

		}
	}
}
