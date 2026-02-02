using UnityEngine;
using UnityEngine.UI;

public class score : MonoBehaviour
{
    public Text scoreText;

    public sequence sequenceScript;

    private int index = 0;

    private int scoreAdd = 3;
    private int ballCount = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("purpleOne") || other.CompareTag("greenOne"))
        {
            if (sequenceScript.colorSequance[index] == 'P' && other.CompareTag("purpleOne")) scoreAdd = 5;
            else if (sequenceScript.colorSequance[index] == 'G' && other.CompareTag("greenOne")) scoreAdd = 5;
            int number = int.Parse(scoreText.text);
            number += scoreAdd;
            scoreText.text = number.ToString();
            scoreAdd = 3;
            ballCount++;
            index++;
        }
    }
}
