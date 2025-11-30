using TMPro;
using UnityEngine;

public class SaveLoadButton : MonoBehaviour
{
    public TMP_InputField TMP_InputField;
    public void SetText()
    {
        TMP_InputField.text = transform.GetChild(0).GetComponent<TMP_Text>().text;
    }
    

}
