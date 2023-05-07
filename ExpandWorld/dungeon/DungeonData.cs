using System.Collections.Generic;
using System.ComponentModel;
namespace ExpandWorld;
public class DungeonDoorData
{
  public string prefab = "";
  public string connectionType = "";
  public float chance = 1f;
}
public class DungeonData
{
  public string name = "";
  public string algorithm = "";
  public int maxRooms = 1;
  public int minRooms = 1;
  public int minRequiredRooms = 1;
  [DefaultValue("")]
  public string requiredRooms = "";
  [DefaultValue("")]
  public string excludedRooms = "";
  [DefaultValue(false)]
  public bool alternative = false;
  [DefaultValue("")]
  public string themes = "";
  public DungeonDoorData[] doorTypes = new DungeonDoorData[0];
  public float doorChance;
  [DefaultValue(90f)]
  public float maxTilt = 90f;
  public float tileWidth;
  [DefaultValue(1f)]
  public float spawnChance = 1f;
  public float campRadiusMin;
  public float campRadiusMax;
  public float minAltitude;
  public int gridSize;
  public int perimeterSections;
  public float perimeterBuffer;
  [DefaultValue(false)]
  public bool interiorTransform = false;

}

public class FakeDungeonGenerator
{
  public string name = "";
  public DungeonGenerator.Algorithm m_algorithm = DungeonGenerator.Algorithm.Dungeon;
  public int m_maxRooms = 1;
  public int m_minRooms = 1;
  public int m_minRequiredRooms = 1;
  public List<string> m_requiredRooms = new();
  public bool m_alternativeFunctionality = false;
  public Room.Theme m_themes = Room.Theme.Cave;
  public List<DungeonGenerator.DoorDef> m_doorTypes = new();
  public float m_doorChance;
  public float m_maxTilt;
  public float m_tileWidth;
  public float m_spawnChance;
  public float m_campRadiusMin;
  public float m_campRadiusMax;
  public float m_minAltitude;
  public int m_gridSize;
  public int m_perimeterSections;
  public float m_perimeterBuffer;
  public bool m_useCustomInteriorTransform;

}
