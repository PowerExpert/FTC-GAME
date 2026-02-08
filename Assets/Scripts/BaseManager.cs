using UnityEngine;
using UnityEngine.UI;

public class BaseManager : MonoBehaviour
{
    static public void Count(RedBase objR0, RedBase objR1, RedBase objR2, RedBase objR3, BlueBase objB0, BlueBase objB1, BlueBase objB2, BlueBase objB3, Text redScore, Text blueScore)
    {
        int redScoreValue = 0;
        int blueScoreValue = 0;
        if (objR0.isActivated && objR1.isActivated && objR2.isActivated && objR3.isActivated)
        {
            redScoreValue += 10;
            objR0.isActivated = objR1.isActivated = objR2.isActivated = objR3.isActivated = false;
        }
        else if (objR0.isActivated || objR1.isActivated || objR2.isActivated || objR3.isActivated)
        {
            redScoreValue += 5;
            objR0.isActivated = objR1.isActivated = objR2.isActivated = objR3.isActivated = false;
        }

        if (objB0.isActivated && objB1.isActivated && objB2.isActivated && objB3.isActivated)
        {
            blueScoreValue += 10;
            objB0.isActivated = objB1.isActivated = objB2.isActivated = objB3.isActivated = false;
        }
        else if (objB0.isActivated || objB1.isActivated || objB2.isActivated || objB3.isActivated)
        {
            blueScoreValue += 5;
            objB0.isActivated = objB1.isActivated = objB2.isActivated = objB3.isActivated = false;
        }

        redScore.text = redScoreValue.ToString();
        blueScore.text = blueScoreValue.ToString();
    }
}
