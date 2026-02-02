using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    public Text timerText;

    public float time;
    public float tempTime;
    void Update()
    {
        tempTime = time;

        int minutes = Mathf.FloorToInt(tempTime / 60F);
        int seconds = Mathf.FloorToInt(tempTime - minutes * 60);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        tempTime -= Time.deltaTime;
        if(tempTime>=0)
        {
            tempTime = time;
        }
    }
}
