using UnityEngine;


/// <summary>
/// nothing much is done here yet, but having a parent class will probably be useful at somepoint
/// </summary>
public class SModule : MonoBehaviour
{
    public string moduleName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnStart();  
    }
    protected virtual void OnStart()
    {

    }
    // Update is called once per frame
    void Update()
    {
        OnUpdate();
    }
    protected virtual void OnUpdate()
    {

    }
}
