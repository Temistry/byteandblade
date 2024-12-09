using UnityEngine;

public static class ToolFunctions
{
    // 이름으로 자식 컴포넌트 찾기
    static public T FindChild<T>(GameObject obj, string text, bool infinite = false) where T : Component
    {
        if(infinite)
        {
            foreach(Transform child in obj.transform)
            {
                // 자식의 이름이 일치하는지 확인
                if(child.name == text)
                {
                    return child.gameObject.GetComponent<T>();
                }
                else
                {
                    // 자식의 자식도 재귀적으로 탐색
                    T foundChild = FindChild<T>(child.gameObject, text, infinite);
                    if(foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }
        }
        else
        {
            Transform child = obj.transform.Find(text);
            if(child != null)
            {
                return child.gameObject.GetComponent<T>();
            }
        }

        return null;
    }
}