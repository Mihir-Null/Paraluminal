using UnityEngine;

/// <summary>
/// All screens should use this as their base class
/// All screen scripts should be named like:
/// 
/// SD_ScreenName
/// </summary>
public class ScreenData : MonoBehaviour
{
   protected ComputerScreenBase csb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void RunStart()
    {
        csb = transform.GetComponentInParent<ComputerScreenBase>();
    }

    // Update is called once per frame
    public virtual void RunUpdate()
    {
        
    }

    public void Button_GoToScreen(string screen)
    {
        csb.LoadScreen(screen);
    }
}
