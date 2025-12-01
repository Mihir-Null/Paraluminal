using UnityEngine;

[System.Serializable]
public class MassShadow : MonoBehaviour
{
    public MassShadowBuffer history;
    public Transform         ghost;    // visual representation
    public Renderer          renderer; // ghost renderer
    [HideInInspector] public MaterialPropertyBlock mpb;
}
