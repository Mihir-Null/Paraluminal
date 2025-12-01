using System;
using UnityEngine;

public struct DVector4
{

    public double t { get; set; }
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }
    public DVector4(double t, double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.t = t;
    }
    public DVector4(DVector4 dVector)
    {
        this.x = dVector.x;
        this.y = dVector.y;
        this.z = dVector.z;
        this.t = dVector.t;
    }
    /// <summary>
    /// Creates new with t = 0
    /// </summary>
    /// <param name="vector"></param>
    public DVector4(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
        this.t = 0;
    }
    /// <summary>
    /// creates new with t = w
    /// </summary>
    /// <param name="vector"></param>
    public DVector4(Vector4 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
        this.t = vector.w;
    }
    public Vector4 ToUnityVector4()
    {
        return new Vector4((float)x, (float)y, (float)z, (float)t);
    }
    public Vector3 ToVector3()
    {
        return ToUnityVector4();
    }

    public static DVector4 operator +(DVector4 a, DVector4 b)
    {
        return new DVector4(a.t + b.t, a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public double[] ToArray() => new double[] { t, x, y, z };

    public static DVector4 operator *(DVector4 a, double b)
    {
        return new DVector4(a.t * b, a.x * b, a.y * b, a.z * b);
    }
    public static DVector4 operator *(double b, DVector4 a)
    {
        return a * b;
    }
    
    public static DVector4 operator -(DVector4 a, DVector4 b)
    {
        return a + (b * -1);
    }
    public static DVector4 operator /(DVector4 a, double b)
    {
        return a * (1 / b);
    }

    /// <summary>
    /// Minkowski Dot product 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double operator *(DVector4 a, DVector4 b)
    {
        return -a.t * b.t + a.x * b.x + a.y * b.y + a.z * b.z;
    }

    /// <summary>
    /// Euclidean Distance between two vectors with no respect for time
    /// </summary>
    public static double Distance(DVector4 a, DVector4 b)
    {
        return Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2) + Math.Pow(a.z - b.z, 2));
    }
    /// <summary>
    /// returns  new DVector4(Math.Pow(a.t, scalar), Math.Pow(a.x, scalar), Math.Pow(a.y, scalar), Math.Pow(a.z, scalar));  
    /// </summary>
    /// <param name="a"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static DVector4 Pow(DVector4 a, double scalar = 2)
    {
        return new DVector4(Math.Pow(a.t, scalar), Math.Pow(a.x, scalar), Math.Pow(a.y, scalar), Math.Pow(a.z, scalar));
    }
    /// <summary>
    /// shortcut for calling Pow(a, 1/2)
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static DVector4 Sqrt(DVector4 a)
    {
        return Pow(a, 1 / 2);
    }

}
