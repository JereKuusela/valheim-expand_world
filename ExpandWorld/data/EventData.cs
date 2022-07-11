using System.ComponentModel;
namespace ExpandWorld;

public class EventData {
  public string name = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue(60f)]
  public float duration = 60f;
  [DefaultValue(true)]
  public bool nearBaseOnly = true;
  [DefaultValue(new string[0])]
  public string[] biome = new string[0];
  [DefaultValue(new string[0])]
  public string[] requiredGlobalKeys = new string[0];
  [DefaultValue(new string[0])]
  public string[] notRequiredGlobalKeys = new string[0];
  [DefaultValue("")]
  public string startMessage = "";
  [DefaultValue("")]
  public string endMessage = "";
  public string forceMusic = "";
  [DefaultValue("")]
  public string forceEnvironment = "";
  public SpawnData[] spawns = new SpawnData[0];
  [DefaultValue(true)]
  public bool pauseIfNoPlayerInArea = true;
  [DefaultValue(true)]
  public bool random = true;
}
