using UnityEngine;

[CreateAssetMenu(fileName = "GlobalPhysics", menuName = "Scriptable Objects/GlobalPhysics")]
public class GlobalPhysics : ScriptableObject
{
    [Tooltip("Player Acceleration 3-Vector")]
    public Vector3 playerAcceleration;

    [Tooltip("Speed of light in scene units (m/s).")]
    public float c;
}
