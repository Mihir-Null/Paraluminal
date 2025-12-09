using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class RelativisticCamera : MonoBehaviour
{
    public GlobalPhysics physicsState;
    public double playerTimeU;          // synced with player’s TUniverse
    public List<MassShadow> shadows;
    [Tooltip("Which body defines the observer position/time for lightcones")]
    public RelativisticBody2 observerBody;
    [Tooltip("Fallback transform for observer position if no body is set")]
    public Transform observerTransform;


    void Awake()
    {
        if (observerTransform == null) observerTransform = transform;
        shadows = new List<MassShadow>(Object.FindObjectsByType<MassShadow>(FindObjectsSortMode.None));
    }
    void LateUpdate()
    {
        float3 observerPos = observerTransform != null ? (float3)observerTransform.position : float3.zero;
        if (observerBody != null)
        {
            observerPos = observerBody.GetPosition();
            playerTimeU = observerBody.GetRindlerTime();
        }
        else
        {
            playerTimeU = Time.time;
        }

        foreach (var t in shadows)
        {
            if (t == null || t.history == null || t.ghost == null) continue;

            t.history.c = physicsState.c;
            if (!t.history.Pop(observerPos, playerTimeU, out var g))
                continue;

            // Apparent position is just emission position in player rest frame
            float3 pos = g.position - observerPos;

            // Place ghost relative to camera (camera sits at origin in same frame)
            t.ghost.SetParent(transform, false);
            t.ghost.localPosition = (Vector3)pos;
            t.ghost.localRotation = (Quaternion)g.rotation;

            // Doppler: observer at rest
            float3 n = math.normalize(-pos);  // from source to observer (origin)
            float3 v = g.velocity;
            float beta2 = math.lengthsq(v) / (physicsState.c*physicsState.c);
            float gamma = 1.0f / math.sqrt(math.max(1e-8f, 1.0f - beta2));
            float cos   = math.dot(v / physicsState.c, n);

            float D = gamma * (1 - cos);      // ν_obs / ν_emit
            float B = Mathf.Pow(D, 3f);       // relativistic beaming ~ D^3

            //lorentz contraction
            float v2 = math.lengthsq(v);

            // Only contract if the object has velocity
            if (v2 > 0f)
            {
                float3 dir = math.normalize(v);

                float invGamma = 1f / gamma;

                float3x3 contraction = float3x3.identity +
                    (invGamma - 1f) * new float3x3(
                    dir.x * dir.x, dir.x * dir.y, dir.x * dir.z,
                    dir.y * dir.x, dir.y * dir.y, dir.y * dir.z,
                    dir.z * dir.x, dir.z * dir.y, dir.z * dir.z
                    );

                float3 baseScale = new float3(1f,1f,1f); // or your mesh's original scale
                float3 contracted = math.mul(contraction, baseScale);

                t.ghost.localScale = (Vector3)contracted;
            }
            else
            {
                // No contraction if not moving
                t.ghost.localScale = Vector3.one;
            }

            if (t.mpb == null) t.mpb = new MaterialPropertyBlock();
            t.renderer.GetPropertyBlock(t.mpb);
            t.mpb.SetFloat("_DopplerFactor", D);
            t.mpb.SetFloat("_BeamingFactor", B);
            t.renderer.SetPropertyBlock(t.mpb);
        }
    }
}
