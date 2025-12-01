using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class MassShadowBuffer : MonoBehaviour
{
    [Header("History Settings")]
    public int capacity = 1024;
    public float c      = 299792458f;

    public NativeArray<Graviton> Buffer;

    private int head;   // oldest logical element
    private int count;  // number of valid elements

    // For Pop interpolation when older data has been ejected
    private bool     hasLastReturned;
    private Graviton lastReturned;

    void Awake()
    {
        Buffer = new NativeArray<Graviton>(capacity, Allocator.Persistent);
        head   = 0;
        count  = 0;
        hasLastReturned = false;
    }

    void OnDestroy()
    {
        if (Buffer.IsCreated) Buffer.Dispose();
    }

    public int Capacity => capacity;
    public int Count    => count;

    // Push a new snapshot at the "newest" end (logical index Count-1)
    public void Push(float3 pos, float3 vel, double t, quaternion rot)
    {
        var g = new Graviton(pos, vel, t, rot);

        int tail = (head + count) % capacity;
        Buffer[tail] = g;

        if (count == capacity)
        {
            // overwrite oldest
            head = (head + 1) % capacity;
        }
        else
        {
            count++;
        }
    }

    // Schedule the Burst job to update all gravitons in parallel
    public JobHandle ScheduleAdvanceJob(float dt, float3 playerAccel, float accelMag, JobHandle dependency = default)
    {
        if (count == 0) return dependency;

        float3 accelDir = accelMag > 1e-6f ? math.normalize(playerAccel) : new float3(1,0,0);

        var job = new AdvanceMassShadowJob
        {
            Buffer    = Buffer,
            Head      = head,
            Capacity  = capacity,
            Count     = count,
            dt        = dt,
            accelDir  = accelDir,
            accelMag  = accelMag,
            c         = c
        };

        // Schedule over 'count' logical elements
        return job.Schedule(count, 64, dependency);
    }

    /// <summary>
    /// Find the graviton event that lies on (or near) the player's past light cone,
    /// given the player's universe-frame position and time.
    /// 
    /// If the event we want has already been ejected from the ring buffer, we
    /// interpolate between the lastReturned graviton and the oldest current graviton.
    /// </summary>
    public bool Pop(float3 playerPos, double playerTime, out Graviton result)
    {
        result = default;

        if (count == 0)
            return false;

        // Helper to index ring by logical index [0..count-1]
        Graviton GetLogical(int i)
        {
            int idx = (head + i) % capacity;
            return Buffer[idx];
        }

        // f(t) = c*(tNow - tEmit) - |xNow - xEmit|
        // We want f ~ 0 for a lightlike separation.
        double fPrev = 0;
        bool   hasPrev = false;
        int    prevIndex = -1;

        int startIndex = 0; // could be optimized using lastReturned.time if you want

        for (int i = startIndex; i < count; i++)
        {
            Graviton g = GetLogical(i);

            double dt = playerTime - g.time;
            double dist = math.distance(playerPos, g.position);
            double f = c * dt - dist;

            if (!hasPrev)
            {
                fPrev = f;
                prevIndex = i;
                hasPrev = true;
                continue;
            }

            // Look for a sign change in f between prev and current
            if ((fPrev <= 0 && f >= 0) || (fPrev >= 0 && f <= 0))
            {
                Graviton gPrev = GetLogical(prevIndex);

                // Secant-like interpolation factor
                double denom = (fPrev - f);
                float alpha = denom != 0.0
                    ? (float)(fPrev / denom)
                    : 0.5f;

                alpha = math.clamp(alpha, 0f, 1f);

                // Interpolate between gPrev and g
                float3 pos = math.lerp(gPrev.position, g.position, alpha);
                double t   = math.lerp((float)gPrev.time, (float)g.time, alpha);
                float3 vel = math.lerp(gPrev.velocity, g.velocity, alpha);
                quaternion rot = math.slerp(gPrev.rotation, g.rotation, alpha);

                Graviton interp = new Graviton(pos, vel, t, rot);

                result = interp;
                lastReturned = interp;
                hasLastReturned = true;
                return true;
            }

            fPrev = f;
            prevIndex = i;
        }

        // If we arrive here, we did not find a sign change bracket.
        // This often means the desired event lies earlier than the oldest stored,
        // or later than the newest. We'll use the fallback with lastReturned.

        Graviton oldest = GetLogical(0);
        Graviton newest = GetLogical(count - 1);

        // Case: desired emission time is earlier than oldest stored,
        // and we've already returned something in the past.
        if (hasLastReturned && oldest.time > lastReturned.time)
        {
            double t0 = lastReturned.time;
            double t1 = oldest.time;

            double denom = (t1 - t0);
            float alpha = denom > 0.0
                ? (float)((playerTime - t0) / denom)
                : 0f;

            alpha = math.clamp(alpha, 0f, 1f);

            float3 pos = math.lerp(lastReturned.position, oldest.position, alpha);
            double t   = math.lerp((float)lastReturned.time, (float)oldest.time, alpha);
            float3 vel = math.lerp(lastReturned.velocity, oldest.velocity, alpha);
            quaternion rot = math.slerp(lastReturned.rotation, oldest.rotation, alpha);

            Graviton interp = new Graviton(pos, vel, t, rot);

            result = interp;
            lastReturned = interp;
            hasLastReturned = true;
            return true;
        }

        // Otherwise, just return the closest we have (e.g. newest)
        result = newest;
        lastReturned = newest;
        hasLastReturned = true;
        return true;
    }
}
