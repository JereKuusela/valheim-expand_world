using System.ComponentModel;
using UnityEngine;

namespace ExpandWorld;

public class RoomData
{
  public string theme = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue(false)]
  public bool entrance = false;
  [DefaultValue(false)]
  public bool endCap = false;
  [DefaultValue(false)]
  public bool divider = false;
  public Vector3 size;
}
