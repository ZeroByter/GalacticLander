using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ChatEntryController : MonoBehaviour {
    public ScrollRect parentChatScroll;
    public LerpCanvasGroup lerpGroup;

    private void Awake() {
        lerpGroup.ForceAlpha(0);
    }

    private IEnumerator ScrollToBottom() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => gameObject.activeSelf);

        Canvas.ForceUpdateCanvases();

        parentChatScroll.normalizedPosition = new Vector2(0, 0);
        lerpGroup.target = 1;

        Canvas.ForceUpdateCanvases();
    }

    public void Setup(string text) {
        gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        GetComponent<TMP_Text>().text = text;

        StartCoroutine(ScrollToBottom());

        Canvas.ForceUpdateCanvases();

        parentChatScroll.normalizedPosition = new Vector2(0, 0);

        Canvas.ForceUpdateCanvases();

        //yes... YES! I know this is a giant fucking mess, but this is the only it will work! :(
    }
}
