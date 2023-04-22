using System.ComponentModel;

namespace ExpandWorld;

public class RoomConnectionData
{
  public string position = "";
  [DefaultValue("")]
  public string type = "";
  [DefaultValue(false)]
  public bool entrance = false;
  [DefaultValue("true")]
  public string door = "true";
}
public class RoomData
{
  public string name = "";
  public string theme = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue(false)]
  public bool entrance = false;
  [DefaultValue(false)]
  public bool endCap = false;
  [DefaultValue(false)]
  public bool divider = false;
  [DefaultValue(0)]
  public int endCapPriority = 0;
  [DefaultValue(0)]
  public int minPlaceOrder = 0;
  [DefaultValue(1f)]
  public float weight = 1f;
  [DefaultValue(false)]
  public bool faceCenter = false;
  [DefaultValue(false)]
  public bool perimeter = false;
  public string size = "";
  public RoomConnectionData[] connections = new RoomConnectionData[0];
  [DefaultValue(null)]
  public string[]? objects = null;
}
