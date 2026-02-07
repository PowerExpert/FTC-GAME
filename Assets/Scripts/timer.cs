using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    public GameObject objR0;
    public GameObject objR1;
    public GameObject objR2;
    public GameObject objR3;

    public GameObject objB0;
    public GameObject objB1;
    public GameObject objB2;
    public GameObject objB3;

    public GameObject redWin;
    public GameObject blueWin;
    public GameObject tie;
    public GameObject button;

    public Text redScore;
    public Text blueScore;

    public Text timerText;

    public spawner spawnerScript;

    public float time;
    private float tempTime;

    private void Start()
    {
        tempTime = time;
    }
    void Update()
    {
        int minutes = Mathf.FloorToInt(tempTime / 60F);
        int seconds = Mathf.FloorToInt(tempTime - minutes * 60);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);

        tempTime -= Time.deltaTime;
        if(tempTime<=0)
        {
            if(redScore.text >= blueScore.text) tie.SetActive(true);
            else if (redScore.text > blueScore.text) redWin.SetActive(true);
            else blueWin.SetActive(true);
            button.SetActive(true);
            
        }
    }
}
