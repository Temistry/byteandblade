using UnityEngine;

public class UI_CharacterShop : MonoBehaviour
{
    Animator windowAnimator;
    public bool isOn = false;

    void Start()
    {
        windowAnimator = gameObject.GetComponent<Animator>();
         WindowIn();
    }

    public void OnEnable()
    {
        WindowIn();
    }

    public void OnDisable()
    {
        WindowOut();
    }

    public void AnimateWindow()
    {
        if (isOn == false)
        {
            if(windowAnimator != null)
            {
                windowAnimator.CrossFade("Window In", 0.1f);
                isOn = true;
            }
        }

        else
        {
            if(windowAnimator != null)
            {
                windowAnimator.CrossFade("Window Out", 0.1f);
                isOn = false;
            }
        }
    }

    public void WindowIn()
    {
        if (isOn == false)
        {
            if(windowAnimator != null)
            {
                windowAnimator.CrossFade("Window In", 0.1f);
                isOn = true;
            }
        }
    }

    public void WindowOut()
    {
        if (isOn == true)
        {
            if(windowAnimator != null)
            {
                windowAnimator.CrossFade("Window Out", 0.1f);
                isOn = false;
            }
        }
    }
}
