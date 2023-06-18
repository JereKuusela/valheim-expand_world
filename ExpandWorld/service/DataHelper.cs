using UnityEngine;

namespace Service;

public class DataHelper
{
  public static ZPackage? Deserialize(string data) => data == "" ? null : new(data);
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
    pkg.SetPos(0);
    var id = zdo.m_uid;
    var num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_floats, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_floats[id].SetValue(pkg.ReadInt(), pkg.ReadSingle());
    }
    if ((num & 2) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_vec3, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_vec3[id].SetValue(pkg.ReadInt(), pkg.ReadVector3());
    }
    if ((num & 4) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_quats, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_quats[id].SetValue(pkg.ReadInt(), pkg.ReadQuaternion());
    }
    if ((num & 8) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_ints, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_ints[id].SetValue(pkg.ReadInt(), pkg.ReadInt());
    }
    // Intended to come before strings.
    if ((num & 64) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_longs, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_longs[id].SetValue(pkg.ReadInt(), pkg.ReadLong());
    }
    if ((num & 16) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_strings, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_strings[id].SetValue(pkg.ReadInt(), pkg.ReadString());
    }
    if ((num & 128) != 0)
    {
      var count = pkg.ReadByte();
      ZDOHelper.Init(ZDOExtraData.s_byteArrays, id);
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_byteArrays[id].SetValue(pkg.ReadInt(), pkg.ReadByteArray());
    }
  }
}