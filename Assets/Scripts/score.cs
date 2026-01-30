using UnityEngine;
using UnityEngine.UI;

public class score : MonoBehaviour
{
    public Text scoreText;

    private int scoreAdd = 3;
    private int ballCount = 0;
    private void OnTriggerEnter(Collider other)
    {
        if(ballCount >= 9)
        {
            scoreAdd = 1;
        }
        if (other.CompareTag("purpleOne") || other.CompareTag("greenOne"))
        {
            int number = int.Parse(scoreText.text);
            number += scoreAdd;
            scoreText.text = number.ToString();
            ballCount++;
        }
    }
}
