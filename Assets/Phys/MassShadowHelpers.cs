using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public struct Graviton
{
    public float3 position;     // world/universe-frame position
    public float3 velocity;     // optional, if you want
    public double time;         // universe or Rindler coordinate time
    public quaternion rotation; // orientation (if needed)

    public Graviton(float3 pos, float3 vel, double t, quaternion rot)
    {
        position = pos;
        velocity = vel;
        time     = t;
        rotation = rot;
    }
}

[BurstCompile]
public struct AdvanceMassShadowJob : IJobParallelFor
{
    public NativeArray<Graviton> Buffer;

    public int   Head;       // ring buffer head (oldest)
    public int   Capacity;   // ring capacity
    public int   Count;      // number of valid entries

    public float dt;
    public float3 accelDir;  // playerAcceleration (normalized)
    public float accelMag;   // playerAccelerationMagnitude
    public float c;

    // Yoshida 4th order coefficients
    static readonly float c1 =  0.5153528374311229364f;
    static readonly float c2 = -0.085782019412973646f;
    static readonly float c3 =  0.4415830236164665242f;
    static readonly float c4 =  0.1288461583653841854f;

    static readonly float d1 =  0.1344961992774310892f;
    static readonly float d2 = -0.2248198030794208058f;
    static readonly float d3 =  0.7563200005156682911f;
    static readonly float d4 =  0.3340036032863214255f;

    public void Execute(int logicalIndex)
    {
        if (logicalIndex >= Count) return;

        int idx = (Head + logicalIndex) % Capacity;
        Graviton g = Buffer[idx];

        g.position = IntegrateStepYoshida(g.position, dt, accelDir, accelMag, c);
        Buffer[idx] = g;
    }

    // ---- Helper methods ----

    static float3 IntegrateStepYoshida(float3 xG,
                                       float dt,
                                       float3 accelDir,
                                       float accelMag,
                                       float c)
    {
        float3 x = xG;
        float3 v = float3.zero; // if you want velocity, you can pass it in

        // Build an orthonormal basis where local +X = accelDir
        float3 xHat = math.normalize(accelDir);
        float3 tmp  = math.abs(xHat.y) < 0.99f ? new float3(0,1,0) : new float3(1,0,0);
        float3 zHat = math.normalize(math.cross(xHat, tmp));
        float3 yHat = math.cross(zHat, xHat);

        float3x3 worldFromLocal = new float3x3(xHat, yHat, zHat);
        float3x3 localFromWorld = math.transpose(worldFromLocal);

        float3 xLocal = math.mul(localFromWorld, x);
        float3 vLocal = math.mul(localFromWorld, v);
        float tau = 0f;

        // Stage 1
        Drift(ref xLocal, ref vLocal, ref tau, dt * c1, accelMag, c);
        Kick (ref vLocal, xLocal, dt * d1, accelMag, c);

        // Stage 2
        Drift(ref xLocal, ref vLocal, ref tau, dt * c2, accelMag, c);
        Kick (ref vLocal, xLocal, dt * d2, accelMag, c);

        // Stage 3
        Drift(ref xLocal, ref vLocal, ref tau, dt * c3, accelMag, c);
        Kick (ref vLocal, xLocal, dt * d3, accelMag, c);

        // Stage 4
        Drift(ref xLocal, ref vLocal, ref tau, dt * c4, accelMag, c);
        Kick (ref vLocal, xLocal, dt * d4, accelMag, c);

        return math.mul(worldFromLocal, xLocal);
    }

    static void Drift(ref float3 xLocal,
                      ref float3 vLocal,
                      ref float   tau,
                      float h,
                      float accelMag,
                      float c)
    {
        xLocal += vLocal * h;

        float X  = xLocal.x;
        float v2 = math.lengthsq(vLocal);
        float onePlusAX = 1.0f + (accelMag * X) / (c * c);
        float inside = onePlusAX * onePlusAX - v2 / (c * c);
        inside = math.max(inside, 0.0f);
        float dtau_dT = math.sqrt(inside);
        tau += dtau_dT * h;
    }

    static void Kick(ref float3 vLocal,
                     float3 xLocal,
                     float h,
                     float accelMag,
                     float c)
    {
        float3 aSpatial = ComputeSpatialAccelRindler(xLocal, vLocal, accelMag, c);
        vLocal += aSpatial * h;
    }

    static float3 ComputeSpatialAccelRindler(float3 xLocal,
                                             float3 vLocal,
                                             float accelMag,
                                             float c)
    {
        float X  = xLocal.x;
        float vX = vLocal.x;
        float vY = vLocal.y;
        float vZ = vLocal.z;

        float v2 = math.lengthsq(vLocal);

        float u    = 1.0f + (accelMag * X) / (c * c);
        float invU = 1.0f / math.max(1e-6f, u);

        float dvX =
              2.0f * (accelMag / (c * c)) * vX * vX * invU
            - accelMag * u;

        float dvY =
              2.0f * (accelMag / (c * c)) * vX * vY * invU;

        float dvZ =
              2.0f * (accelMag / (c * c)) * vX * vZ * invU;

        return new float3(dvX, dvY, dvZ);
    }
}

