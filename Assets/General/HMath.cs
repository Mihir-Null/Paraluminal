using UnityEngine;

/// <summary>
/// A class full of unrelated static math functions 
/// just so I can write them once and reuse them
/// </summary>
public static class HMath
{
    /// <summary>
    /// return a decimal 0 to 1 that shows cur's position between high and low
    /// 
    /// "Between Two Points"
    /// </summary>
    /// <param name="high"></param>
    /// <param name="low"></param>
    /// <param name="cur"></param>
    /// <returns></returns>
    public static float BTP(float high, float low, float cur)
    {
        return (cur - low) / (high - low);
    }
    public static float BTP_Checked(float high, float low, float cur)
    {
        float r = BTP(high, low, cur);
        if (r > 1)
        {
            return 1;
        }
        else if (r < 0)
        {
            return 0;
        }
        return r;
    }
    
    public static int BPT_CheckedInt(int high, int low, int cur)
    {
        return (int)BTP_Checked(high, low, cur);    
    }

    /// <summary>
    /// Not a real math thing, but it is used in some of the code for the Thrust Control System  /// 
    ///    return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    /// </summary>
    /// <returns></returns>
    public static Vector3 VectorMult(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

   public static float  posLogic(float max, float min, float cur)
    {
        if (cur > max)
        {
            return 1;
        }
        if (cur < min)
        {
            return -1;
        }
        if (cur > 0)
        {
            return HMath.BTP(max, 0, cur);
        }
        if (cur < 0)
        {
            return -HMath.BTP(min, 0, cur);
        }


        return 0;
    }

}
