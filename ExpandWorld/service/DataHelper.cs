using UnityEngine;

namespace Service;

public class DataHelper
{
  public static ZPackage? Deserialize(string data)
  {
    if (data == "") return null;
    ZPackage pkg = new(data);
    return pkg;
  }
  public static ZDO? InitZDO(Vector3 pos, Quaternion rot, Vector3? scale, ZPackage? data, GameObject obj)
  {
    if (!obj.TryGetComponent<ZNetView>(out var view)) return null;
    return InitZDO(pos, rot, scale, data, view);
  }

  public static ZDO? InitZDO(Vector3 pos, Quaternion rot, Vector3? scale, ZPackage? data, ZNetView view)
  {
    // No override needed.
    if (data == null && scale == null) return null;
    var prefab = view.GetPrefabName().GetStableHashCode();
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos, prefab);
    if (data != null)
      Load(data, ZNetView.m_initZDO);
    if (scale.HasValue)
      ZNetView.m_initZDO.Set(ZDOVars.s_scaleHash, scale.Value);
    ZNetView.m_initZDO.m_rotation = rot.eulerAngles;
    ZNetView.m_initZDO.Type = view.m_type;
    ZNetView.m_initZDO.Distant = view.m_distant;
    ZNetView.m_initZDO.Persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = prefab;
    ZNetView.m_initZDO.DataRevision = 1;
    return ZNetView.m_initZDO;
  }
  private static void Load(ZPackage pkg, ZDO zdo)
  {
    var id = zdo.m_uid;
    var num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      var count = pkg.ReadByte();
      var floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id] : new();
      for (var i = 0; i < count; ++i)
        floats[pkg.ReadInt()] = pkg.ReadSingle();
    }
    if ((num & 2) != 0)
    {
      var count = pkg.ReadByte();
      var vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id] : new();
      for (var i = 0; i < count; ++i)
        vecs[pkg.ReadInt()] = pkg.ReadVector3();
    }
    if ((num & 4) != 0)
    {
      var count = pkg.ReadByte();
      var quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id] : new();
      for (var i = 0; i < count; ++i)
        quats[pkg.ReadInt()] = pkg.ReadQuaternion();
    }
    if ((num & 8) != 0)
    {
      var count = pkg.ReadByte();
      var ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id] : new();
      for (var i = 0; i < count; ++i)
        ints[pkg.ReadInt()] = pkg.ReadInt();
    }
    // Intended to come before strings.
    if ((num & 64) != 0)
    {
      var count = pkg.ReadByte();
      var longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id] : new();
      for (var i = 0; i < count; ++i)
        longs[pkg.ReadInt()] = pkg.ReadLong();
    }
    if ((num & 16) != 0)
    {
      var count = pkg.ReadByte();
      var strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id] : new();
      for (var i = 0; i < count; ++i)
        strings[pkg.ReadInt()] = pkg.ReadString();
    }
    if ((num & 128) != 0)
    {
      var count = pkg.ReadByte();
      var byteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id] : new();
      for (var i = 0; i < count; ++i)
        byteArrays[pkg.ReadInt()] = pkg.ReadByteArray();
    }
  }
}