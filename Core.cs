﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UIF
{
	public class Item
	{
		public int? id = null;
		public string name = null;

		public int? clothStorageWidth = null, clothStorageHeight = null;

		public int? barricadeStorageWidth = null, barricadeStorageHeight = null;

		public float? buildingHealth = null;

		public string engine = null;

		public float? vehicleHealth = null;

		public float? armor = null;

		public float? range = null;

		public float? headDamage = null, bodyDamage = null;
		public int? playerDamage = null;

		public bool explosive = false, invulnerable = false;

		public float? structureDamage = null;

		public string itemType = null, itemType2 = null;

		public List<string> modes = new List<string>();

		public string slot = null;

		public float? shake = null; // Grip, suppressor and laser (Spread)

		public float? barrelVolume = null;

		public float? barrelDamage = null;
	}

	public static class Core
	{
		public static List<Item> loadedItems;

		public enum CompareModes
		{
			Damage,
			StructureDamage,
			ClothingProtection,
			ClothingStorage,
			VehicleHealth,
			StructureCapacity,
			BuildingHealth,
			Shake,
			BarrelDamage,
			BarrelVolume
		}

		public static int CompareTo(this Item a, Item val, CompareModes mode)
		{
			switch (mode)
			{
				case CompareModes.StructureDamage:
					return (((val.itemType2.TryContains("Charge") || val.itemType.TryContains("Gun")) && val.structureDamage != null) ? val.structureDamage : 0)
						.CompareTo(((a.itemType2.TryContains("Charge") || a.itemType.TryContains("Gun")) && a.structureDamage != null) ? a.structureDamage : 0);
				case CompareModes.Damage:
					return (val.itemType.TryContains("Gun") ? val.GetAverageDamage() : 0)
						.CompareTo(a.itemType.TryContains("Gun") ? a.GetAverageDamage() : 0);
				case CompareModes.ClothingProtection:
					return (a.itemType.TryContains("Clothing") ? a.armor : 1)
						.CompareTo(val.itemType.TryContains("Clothing") ? val.armor : 1);
				case CompareModes.ClothingStorage:
					return (val.itemType.TryContains("Clothing") ? (val.clothStorageHeight * val.clothStorageWidth) : 0)
						.CompareTo(a.itemType.TryContains("Clothing") ? (a.clothStorageHeight * a.clothStorageWidth) : 0);
				case CompareModes.VehicleHealth:
					return (val.itemType2.TryContains("Vehicle") ? val.vehicleHealth : 0)
						.CompareTo(a.itemType2.TryContains("Vehicle") ? a.vehicleHealth : 0);
				case CompareModes.Shake:
					return (a.itemType2.TryContains("Grip", "Barrel", "Tactical") ? a.shake : 1)
						.CompareTo(val.itemType2.TryContains("Grip", "Barrel", "Tactical") ? val.shake : 1);
				case CompareModes.BarrelDamage:
					return (val.itemType2.TryContains("Barrel") ? (val.barrelDamage != null ? val.barrelDamage : 1) : 0)
						.CompareTo(a.itemType2.TryContains("Barrel") ? (a.barrelDamage != null ? a.barrelDamage : 1) : 0);
				case CompareModes.BarrelVolume:
					return (a.itemType2.TryContains("Barrel") ? a.barrelVolume : 1)
						.CompareTo(val.itemType2.TryContains("Barrel") ? val.barrelVolume : 1);
				case CompareModes.StructureCapacity:
					return (val.itemType2.TryContains("Storage") ? (val.barricadeStorageHeight * val.barricadeStorageWidth) : 0)
						.CompareTo(a.itemType2.TryContains("Storage") ? (a.barricadeStorageHeight * a.barricadeStorageWidth) : 0);
				case CompareModes.BuildingHealth:
					return (val.itemType.TryContains("Barricade", "Structure")
						|| val.itemType2.TryContains("Structure", "Barricade") ? val.buildingHealth : 0).

						CompareTo(a.itemType.TryContains("Barricade", "Structure")
						|| a.itemType2.TryContains("Structure", "Barricade") ? a.buildingHealth : 0);
				default:
					throw new Exception("Invalid sort mode");
			}
		}

		public static List<Item> ParseAll(string folderPath, Func<Item, bool> filter = null)
		{
			if (loadedItems == null)
			{
				if (!Directory.Exists(folderPath))
					throw new DirectoryNotFoundException("Folder doesn't exist");

				List<string> dirs = Directory.EnumerateDirectories(folderPath, "*", SearchOption.AllDirectories).ToList();
				List<Item> items = new List<Item>();

				for (int i = 0; i < dirs.Count; i++)
				{
					string EnglishDat = dirs[i] + "\\English.dat";

					if (File.Exists(EnglishDat))
					{
						var files = Directory.EnumerateFiles(dirs[i], "*.dat")
							.ToList();

						var item = ParseDat(files, EnglishDat, filter);
						if (item.id != null)
						{
							items.Add(item);
						}
					}
				}

				return items;
			}
			else
			{
				if (filter != null)
				{
					List<Item> items = new List<Item>();
					foreach (Item _item in loadedItems)
					{
						if (filter(_item))
							items.Add(_item);
					}

					return items;
				}
				else
				{
					return loadedItems;
				}
			}
		}
		public static Item ParseDat(List<string> files, string EnglishDat, Func<Item, bool> filter)
		{
			if (!File.Exists(EnglishDat))
				throw new DirectoryNotFoundException("File doesn't exist");

			foreach (string a in files)
				if (!a.EndsWith("English.dat"))
				{
					string[] linesModDat = File.ReadAllText(a).Split('\n');
					var item = new Item() { name = File.ReadAllText(EnglishDat).Split('\n')[0].Replace("Name ", string.Empty) };

					foreach (string _line in linesModDat)
					{
						string line = _line.Replace("\r", string.Empty);
						try
						{
							if (line.StartsWith("ID "))
								item.id = line.Replace("ID ", string.Empty).ToInt();
							else if (line.StartsWith("Width "))
								item.clothStorageWidth = line.Replace("Width ", string.Empty).ToInt();
							else if (line.StartsWith("Height "))
								item.clothStorageHeight = line.Replace("Height ", string.Empty).ToInt();
							else if (line.StartsWith("Storage_X "))
								item.barricadeStorageWidth = line.Replace("Storage_X ", string.Empty).ToInt();
							else if (line.StartsWith("Storage_Y "))
								item.barricadeStorageHeight = line.Replace("Storage_Y ", string.Empty).ToInt();
							else if (line.StartsWith("Health "))
								item.buildingHealth = item.vehicleHealth = line.Replace("Health ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Useable "))
								item.itemType = line.Replace("Useable ", string.Empty);
							else if (line.StartsWith("Engine "))
								item.engine = line.Replace("Engine ", string.Empty);
							else if (line.StartsWith("Armor "))
								item.armor = line.Replace("Armor ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Player_Spine_Multiplier "))
								item.bodyDamage = line.Replace("Player_Spine_Multiplier ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Player_Skull_Multiplier "))
								item.headDamage = line.Replace("Player_Skull_Multiplier ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Type "))
								item.itemType2 = line.Replace("Type ", string.Empty);
							else if (line.StartsWith("Auto"))
								item.modes.Add("Auto");
							else if (line.StartsWith("Semi"))
								item.modes.Add("Semi");
							else if (line.StartsWith("Burst"))
								item.modes.Add("Burst");
							else if (line.StartsWith("Slot "))
								item.slot = line.Replace("Slot ", string.Empty);
							else if (line.StartsWith("Player_Damage "))
								item.playerDamage = line.Replace("Player_Damage ", string.Empty).Replace(".", ",").ToInt();
							else if (line.StartsWith("Structure_Damage "))
								item.structureDamage = line.Replace("Structure_Damage ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Explosive") || line.StartsWith("Explosion"))
								item.explosive = true;
							else if (line.StartsWith("Range "))
								item.range = line.Replace("Range ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Invulnerable"))
								item.invulnerable = true;
							else if (line.StartsWith("Shake"))
								item.shake = line.Replace("Shake ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Spread"))
								item.shake = line.Replace("Spread ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Volume"))
								item.barrelVolume = line.Replace("Volume ", string.Empty).Replace(".", ",").ToFloat();
							else if (line.StartsWith("Damage"))
								item.barrelDamage = line.Replace("Damage", string.Empty).Replace(".", ",").ToFloat();
						}
						catch { }
					}

					try
					{
						if (filter == null || filter(item))
							return item;
					}
					catch { }
				}

			return new Item();
		}
	}
}