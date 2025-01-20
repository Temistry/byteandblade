using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HorizontalScrollSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float snapDuration = 0.3f; // 스냅 이동 속도
    private float[] snapPositions;
    private int targetIndex = 0;
    private Vector2 startTouchPos, endTouchPos;
    private bool isSwiping = false;

    public int GetIndex()
    {
        return targetIndex;
    }

    void Start()
    {
        int childCount = content.childCount;
        snapPositions = new float[childCount];

        for (int i = 0; i < childCount; i++)
        {
            snapPositions[i] = (float)i / (childCount - 1);
        }
    }

    void Update()
    {
        DetectSwipe();
    }

    void DetectSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            endTouchPos = Input.mousePosition;
            isSwiping = false;

            float swipeDelta = endTouchPos.x - startTouchPos.x;

            if (Mathf.Abs(swipeDelta) > 50) // 최소 스와이프 거리 설정
            {
                if (swipeDelta > 0) // 왼쪽 → 오른쪽 스와이프
                {
                    targetIndex = Mathf.Max(0, targetIndex - 1);
                }
                else if (swipeDelta < 0) // 오른쪽 → 왼쪽 스와이프
                {
                    targetIndex = Mathf.Min(snapPositions.Length - 1, targetIndex + 1);
                }

                SnapToTarget(targetIndex);
            }
        }
    }

    void SnapToTarget(int index)
    {
        float targetPos = snapPositions[index];
        scrollRect.DONormalizedPos(new Vector2(targetPos, 0), snapDuration);
    }
}
