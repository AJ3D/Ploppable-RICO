using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PloppableAI
{

	public static class BuildingDecorationDetoursHolder
	{
		private static bool deployed = false;

		private static RedirectCallsState _BuildingDecoration_LoadDecorations_state;
		private static MethodInfo _BuildingDecoration_LoadDecorations_original;
		private static MethodInfo _BuildingDecoration_LoadDecorations_detour;

		private static RedirectCallsState _BuildingDecoration_SaveDecorations_state;
		private static MethodInfo _BuildingDecoration_SaveDecorations_original;
		private static MethodInfo _BuildingDecoration_SaveDecorations_detour;

		public static void Deploy()
		{
			if (!deployed)
			{
				_BuildingDecoration_LoadDecorations_original = typeof(BuildingDecoration).GetMethod("LoadDecorations", BindingFlags.Static | BindingFlags.Public);
				_BuildingDecoration_LoadDecorations_detour = typeof(BuildingDecorationDetoursHolder).GetMethod("LoadDecorations", BindingFlags.Static | BindingFlags.Public);
				_BuildingDecoration_LoadDecorations_state = RedirectionHelper.RedirectCalls(_BuildingDecoration_LoadDecorations_original, _BuildingDecoration_LoadDecorations_detour);

				_BuildingDecoration_SaveDecorations_original = typeof(BuildingDecoration).GetMethod("SaveDecorations", BindingFlags.Static | BindingFlags.Public);
				_BuildingDecoration_SaveDecorations_detour = typeof(BuildingDecorationDetoursHolder).GetMethod("SaveDecorations", BindingFlags.Static | BindingFlags.Public);
				_BuildingDecoration_SaveDecorations_state = RedirectionHelper.RedirectCalls(_BuildingDecoration_SaveDecorations_original, _BuildingDecoration_SaveDecorations_detour);

				deployed = true;

				Debug.Log("LargerFootprints: BuildingDecoration Methods detoured!");
			}
		}

		public static void Revert()
		{
			if (deployed)
			{
				RedirectionHelper.RevertRedirect(_BuildingDecoration_LoadDecorations_original, _BuildingDecoration_LoadDecorations_state);
				_BuildingDecoration_LoadDecorations_original = null;
				_BuildingDecoration_LoadDecorations_detour = null;

				RedirectionHelper.RevertRedirect(_BuildingDecoration_SaveDecorations_original, _BuildingDecoration_SaveDecorations_state);
				_BuildingDecoration_SaveDecorations_original = null;
				_BuildingDecoration_SaveDecorations_detour = null;

				deployed = false;

				Debug.Log("LargerFootprints: BuildingDecoration Methods restored!");
			}
		}

		public static void LoadDecorations(BuildingInfo source)
		{
			Debug.Log("LoadDecorations called.");

			Building building = default(Building);
			building.m_position = new Vector3(0f, 60f, 0f);
			building.m_width = (byte)source.m_cellWidth;
			building.m_length = (byte)source.m_cellLength;
			BuildingDecoration.LoadPaths(source, 0, ref building, 0f);
			BuildingDecoration.LoadProps(source, 0, ref building);
		}
		public static void SaveDecorations(BuildingInfo target)
		{
			Debug.Log("SaveDecorations called.");

			Building building = default(Building);
			building.m_position = new Vector3(0f, 60f, 0f);
			building.m_width = (byte)target.m_cellWidth;
			building.m_length = (byte)target.m_cellLength;
			BuildingDecoration.SavePaths(target, 0, ref building);
			BuildingDecoration.SaveProps(target, 0, ref building);
		}
	}
}

