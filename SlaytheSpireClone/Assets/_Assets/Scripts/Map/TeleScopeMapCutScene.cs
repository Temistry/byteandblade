using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
// 맵 이미지를 확대하는 연출 클래스
public class TeleScopeMapCutScene : MonoBehaviour
{
    // 확대할 이미지
    public Image teleScopeImage;
    // 확대할 수치
    public float zoomScale = 2.0f;

    UI_StartRelic uiStartRelic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiStartRelic = FindFirstObjectByType<UI_StartRelic>();
    }

    // Update is called once per frame
    void Update()
    {
        // UI_StartRelic 클래스가 활성화 되면 확대
        if (uiStartRelic != null && uiStartRelic.isActiveAndEnabled)
        {
            ZoomOut();
        }
        else
        {
            ZoomIn();
            FadeOut();
        }
    }

    public void ZoomIn()
    {
        // dotween 사용
        DOTween.To(() => teleScopeImage.transform.localScale, x => teleScopeImage.transform.localScale = x, new Vector3(zoomScale, zoomScale, 1), 1.0f);
    }

    public void FadeOut()
    {
        // dotween 사용
        DOTween.To(() => teleScopeImage.color, x => teleScopeImage.color = x, new Color(1, 1, 1, 0), 1.0f);
    }

    public void ZoomOut()
    {
        // dotween 사용
        DOTween.To(() => teleScopeImage.transform.localScale, x => teleScopeImage.transform.localScale = x, new Vector3(1, 1, 1), 1.0f);
    }
}
