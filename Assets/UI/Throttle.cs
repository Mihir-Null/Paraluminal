using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Throttle : MonoBehaviour
{
    public RectTransform throttleHigh;
    public RectTransform throttleLow;
    public RectTransform CurText;
    //this is moved by the player
    public RectTransform throttleSelected;
    //this is moved by the TCP
    public RectTransform throttleCurrent;
    public float max, min;

    public float throttleMoveSpeed;
    public float playerThrottleMoveSpeed;

    public int index;
    public List<Vector2> levels;

    public float currentValue;

    private void Start()
    {
        SwitchThrottle(index);
    }
    public void ThrottleLogic()
    {
        float y = throttleCurrent.position.y;
        if (throttleSelected.position.y > y)
        {

            y += throttleMoveSpeed;

            //account for overshoots 
            if (throttleSelected.position.y < y)
            {
                y = throttleSelected.position.y;
            }
        }
        else if (throttleSelected.position.y < y)
        {
            y -= throttleMoveSpeed;

            //account for overshoots 
            if (throttleSelected.position.y > y)
            {
                y = throttleSelected.position.y;
            }
        }
        throttleCurrent.position = new Vector3(throttleCurrent.position.x, y, throttleCurrent.position.z);

        currentValue = Mathf.Lerp(min, max, HMath.BTP(throttleHigh.position.y, throttleLow.position.y, y));
        currentValue = Mathf.Round(currentValue);

        CurText.GetComponent<TMP_Text>().text = currentValue.ToString();
    }

    void SwitchThrottle(int index, bool up = true)
    {


        if (index >= 0 && index < levels.Count)
        {
            min = levels[index].x;
            max = levels[index].y;
            throttleHigh.GetComponent<TMP_Text>().text = max.ToString();
            throttleLow.GetComponent<TMP_Text>().text = min.ToString();


            if (up)
            {
                throttleSelected.transform.position = throttleLow.transform.position;
                throttleCurrent.transform.position = throttleLow.transform.position;
            }
            else
            {
                throttleSelected.transform.position = throttleHigh.transform.position;
                throttleCurrent.transform.position = throttleHigh.transform.position;
            }
        }



    }
    public void Control_IncreaseDecreaseThrottle(int direction = 1)
    {

        if (currentValue == max && direction == 1 && index + 1 < levels.Count)
        {
            index += 1;
            SwitchThrottle(index, true);
        }
        else if (currentValue == min && direction == -1 && index - 1 >= 0)
        {
            index -= 1;
            SwitchThrottle(index, false);
        }
        else
        {
            float y = throttleSelected.position.y;
            y += playerThrottleMoveSpeed * direction;// * (float)RelBody.ReadOnly_deltaTau;
            if (y > throttleHigh.position.y)
            {
                y = throttleHigh.position.y;
            }
            else if (y < throttleLow.position.y)
            {
                y = throttleLow.position.y;
            }

            throttleSelected.position = new Vector3(throttleSelected.position.x, y, throttleSelected.position.z);
        }

    }
}
