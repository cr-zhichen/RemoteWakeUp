using System.Numerics;

namespace RemoteWakeUp.Entity.Unity;

/// <summary>
/// 模拟Unity的Transform
/// </summary>
public class Transform
{
    /// <summary>
    /// 位置
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// 旋转
    /// </summary>
    public Quaternion Rotation { get; set; }

    /// <summary>
    /// 缩放
    /// </summary>
    public Vector3 Scale { get; set; }
}