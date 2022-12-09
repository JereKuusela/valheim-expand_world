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
  [DefaultValue(false)]
  public bool alternative = false;
  [DefaultValue("")]
  public string themes = "";
  public DungeonDoorData[] doorTypes = new DungeonDoorData[0];
  public float doorChance;
  public float maxTilt;
  public float tileWidth;
  public float spawnChance;
  public float campRadiusMin;
  public float campRadiusMax;
  public float minAltitude;
  public int perimeterSections;
  public float perimeterBuffer;
  [DefaultValue(false)]
  public bool interiorTransform = false;

}
