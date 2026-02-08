using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    public RedBase objR0;
    public RedBase objR1;
    public RedBase objR2;
    public RedBase objR3;

    public BlueBase objB0;
    public BlueBase objB1;
    public BlueBase objB2;
    public BlueBase objB3;

    public BaseManager BaseManagerScript;

    private bool checker = false;

    public GameObject redWin;
    public GameObject blueWin;
    public GameObject tie;
    public GameObject button;

    public Text redScore;
    public Text blueScore;

    public Text timerText;

    public spawner spawnerScript;
    public sequence sequenceScript;

    public float time;
    public float tempTime;

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
            if (!checker)
            {
                BaseManager.Count(objR0, objR1, objR2, objR3, objB0, objB1, objB2, objB3, redScore, blueScore);
                if (int.Parse(redScore.text) == int.Parse(blueScore.text)) tie.SetActive(true);
                else if (int.Parse(redScore.text) >= int.Parse(blueScore.text)) redWin.SetActive(true);
                else if (int.Parse(redScore.text) <= int.Parse(blueScore.text)) blueWin.SetActive(true);
                button.SetActive(true);
                checker = true;
            }
            tempTime = 0;
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            OnRestart();
        }
    }
    public void OnRestart()
    {
        tempTime = time;
        spawnerScript.respawn(spawnerScript.parent);
        sequenceScript.create_sequence();
        button.SetActive(false);
        redWin.SetActive(false);
        blueWin.SetActive(false);
        tie.SetActive(false);
        checker = false;
    }
}
