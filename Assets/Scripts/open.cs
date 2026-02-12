using System.Collections;
using UnityEngine;

public class open : MonoBehaviour
{
    public ___Trigger triggerScript;

    public string requiredTag;

    public GameObject door;

    public GameObject opened;
    public GameObject closed;

    // 0 или меньше => ждать без таймаута
    public float closeTimeoutSeconds = 0f;

    Coroutine closeCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredTag) && door != null)
        {
            // Открыть дверь сразу при входе
            door.SetActive(false);
            opened.SetActive(true);
            closed.SetActive(false);
            // Если была запущена корутина закрытия — остановим её
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Запускаем корутину, которая будет ждать, пока triggerScript.isTouching == false
        if (other.CompareTag(requiredTag) && door != null)
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
            }
            closeCoroutine = StartCoroutine(WaitForNoTouchAndClose(other));
        }
    }

    IEnumerator WaitForNoTouchAndClose(Collider other)
    {
        // Быстрая валидация
        if (other == null || !other.CompareTag(requiredTag) || door == null || triggerScript == null)
        {
            closeCoroutine = null;
            yield break;
        }

        if (closeTimeoutSeconds > 0f)
        {
            float timer = 0f;
            while (triggerScript.isTouching && timer < closeTimeoutSeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            // Если по таймауту всё ещё touching — не закрываем
            if (triggerScript.isTouching)
            {
                closeCoroutine = null;
                yield break;
            }
        }
        else
        {
            // Ждём бесконечно до тех пор, пока не станет false
            while (triggerScript.isTouching)
            {
                yield return null;
            }
        }

        // После того как isTouching == false — закрываем дверь
        door.SetActive(true);
        opened.SetActive(false);
        closed.SetActive(true);

        closeCoroutine = null;
    }
}
