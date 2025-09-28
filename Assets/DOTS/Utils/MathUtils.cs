

using System.Runtime.CompilerServices;
using Unity.Mathematics;

public struct MathUtils
{
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool HasNaN(float3 v)
  {
    return math.any(new bool3(
        float.IsNaN(v.x),
        float.IsNaN(v.y),
        float.IsNaN(v.z)
    ));
  }
}