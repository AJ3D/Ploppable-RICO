using System;
using ColossalFramework;
using System.IO;
using ColossalFramework.IO;
using ICities;
using UnityEngine;

namespace PloppableRICO
{

	/// <summary>
	/// A simple example how to save custom data in savegames.
	/// This uses 3 components: 
	/// 
	/// 1. SerializableDataExtensionBase
	///     This class is provided by the Mod API to load and save raw byte data 
	///     from mods in savegames
	/// 2. DataSerializer
	///     This helper is part of the ColossalFramework, not of the Mod API. 
	///     It helps you convert single objects or array of objects to bytes.
	/// 3. IDataContainer
	///     This interface of ColossalFramework must be implemented by the objects 
	///     you want to serialize.
	///     One method must be implemented for serializing, one for deserializing.
	/// </summary>
	public class BuildingDataManager : SerializableDataExtensionBase
	{
		// this is the array that contains the extra data for all buildings
		// You can access data like this:
		/*
    BuildingData data = BuildingDataManager.buildingData?[buildingId];
    if (data != null)
    {
        // do stuff with data
    }
    */
		public static BuildingData[] buildingData;

	public static BuildingDataManager Instance;

		// The key for our data in the savegame
		public const string DataId = "PRCIO";

		// Version of data save format
		// This is important when you add new fields to BuildingData
		// You can start with 0
		public const uint DataVersion = 0;

		public override void OnCreated(ISerializableData serializableData)
		{


			Debug.Log ("Created Array");
		base.OnCreated(serializableData);

		
			// Create new empty data array (length of the buildings array, ~32k)
			buildingData = new BuildingData[BuildingManager.instance.m_buildings.m_size];


		}

		public override void OnLoadData()
		{
			base.OnLoadData();

			// Get bytes from savegame
			byte[] bytes = serializableDataManager.LoadData(DataId);
			if (bytes == null) return;

			// Convert the bytes to an array of BuildingData objects
			using (var stream = new MemoryStream(bytes))
			{
				buildingData = DataSerializer.DeserializeArray<BuildingData>(stream, DataSerializer.Mode.Memory);
			}

			Debug.LogFormat("Data loaded (Size in bytes: {0})", bytes.Length);
		}

		public override void OnSaveData()
		{
			base.OnSaveData();

			byte[] bytes;


			//var data = BuildingDataManager.buildingData[1000];
			//if (data != null)
			//{

			//buildingData = new BuildingData[BuildingManager.instance.m_buildings.m_size];

			//buildingData[1000].fieldB = 3;

				//data.fieldB = 3;
			//Debug.Log (buildingData[1000].fieldB);
			//}

			// Convert the array of BuildingData objects to bytes
			using (var stream = new MemoryStream())
			{
				DataSerializer.SerializeArray(stream, DataSerializer.Mode.Memory, DataVersion, buildingData);
				bytes = stream.ToArray();
			}

			// Save bytes in savegame
			serializableDataManager.SaveData(DataId, bytes);

			Debug.LogFormat("Data Saved (Size in bytes: {0})", bytes.Length);
			//Debug.Log (data.fieldB.ToString ());

		}

		public override void OnReleased()
		{
			base.OnReleased();

			// Clear to save memory
			buildingData = null;
		}
	}

	/// <summary>
	/// The data class that holds the extra data for each building.
	/// When you add new fields, change the DataVersion and do version checks in
	/// the Serialize/Deserialize methods. That's important to support data 
	/// that was saved with previous versions of the mod.
	/// </summary>
	public class BuildingData : IDataContainer
	{
		public string fieldA;
		public int fieldB;

		public int fieldCAddedInV1;

		public bool fieldDAddedInV2;
		public string fieldEAddedInV2;

		// This serializes the object (to bytes)
		public void Serialize(DataSerializer s)
		{
			s.WriteSharedString(fieldA);
			s.WriteInt32(fieldB);

			if (s.version >= 1)
			{
				s.WriteInt32(fieldCAddedInV1);
			}

			if (s.version >= 2)
			{
				s.WriteBool(fieldDAddedInV2);
				s.WriteSharedString(fieldEAddedInV2);
			}
		}

		// This reads the object (from bytes)
		public void Deserialize(DataSerializer s)
		{
			fieldA = s.ReadSharedString();
			fieldB = s.ReadInt32();

			if (s.version >= 1)
			{
				fieldCAddedInV1 = s.ReadInt32();
			}

			if (s.version >= 2)
			{
				fieldDAddedInV2 = s.ReadBool();
				fieldEAddedInV2 = s.ReadSharedString();
			}
		}

		public void AfterDeserialize(DataSerializer s) {}
	}

}

