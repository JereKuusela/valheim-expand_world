using System.Globalization;
using UnityEngine;
namespace ExpandWorld;

public class Range<T>
{
  public T Min;
  public T Max;
  public Range(T value)
  {
    Min = value;
    Max = value;
  }
  public Range(T min, T max)
  {
    Min = min;
    Max = max;
  }
  public bool Uniform = true;
}

///<summary>Contains functions for parsing arguments, etc.</summary>
public static class Parse
{
  public static int Int(string arg, int defaultValue = 0)
  {
    if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static int Int(string[] args, int index, int defaultValue = 0)
  {
    if (args.Length <= index) return defaultValue;
    return Int(args[index], defaultValue);
  }
  public static float Float(string arg, float defaultValue = 0f)
  {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static float Float(string[] args, int index, float defaultValue = 0f)
  {
    if (args.Length <= index) return defaultValue;
    return Float(args[index], defaultValue);
  }

  public static Quaternion AngleYXZ(string[] args, int index) => AngleYXZ(args, index, Vector3.zero);
  public static Quaternion AngleYXZ(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = Float(args, index, defaultValue.x);
    vector.z = Float(args, index + 1, defaultValue.z);
    vector.y = Float(args, index + 2, defaultValue.y);
    return Quaternion.Euler(vector);
  }

  ///<summary>Parses YXZ vector starting at given index. Zero is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index) => VectorXZY(args, index, Vector3.zero);
  ///<summary>Parses YXZ vector starting at given index. Default values is used for missing values.</summary>
  public static Vector3 VectorXZY(string[] args, int index, Vector3 defaultValue)
  {
    var vector = Vector3.zero;
    vector.x = Float(args, index, defaultValue.x);
    vector.z = Float(args, index + 1, defaultValue.z);
    vector.y = Float(args, index + 2, defaultValue.y);
    return vector;
  }
  public static Vector2 VectorXY(string arg)
  {
    var vector = Vector2.zero;
    var args = arg.Split(',');
    vector.x = Float(args, 0);
    vector.y = Float(args, 1);
    return vector;
  }
  public static Vector3 Scale(string args) => Scale(args.Split(','), 0);
  ///<summary>Parses scale starting at zero index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args) => Scale(args, 0);
  ///<summary>Parses scale starting at given index. Includes a sanity check and giving a single value for all axis.</summary>
  public static Vector3 Scale(string[] args, int index) => SanityCheck(VectorXZY(args, index));
  private static Vector3 SanityCheck(Vector3 scale)
  {
    // Sanity check and also adds support for setting all values with a single number.
    if (scale.x == 0) scale.x = 1;
    if (scale.y == 0) scale.y = scale.x;
    if (scale.z == 0) scale.z = scale.x;
    return scale;
  }
}
