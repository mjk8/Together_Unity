using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonWave : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Sprite isPressedImage;
    [SerializeField] private Sprite isNotPressedImage;
    private float _animDuration = 0.15f;
    private bool isPressed;
    private int myNum;

    public void Init(int buttonNum, bool isCurrent)
    {
        myNum = buttonNum;
        isPressed = isCurrent;
        if (isCurrent)
        {
            transform.GetChild(0).GetComponent<Image>().sprite = isPressedImage;
        }
        else
        {
            transform.GetChild(0).GetComponent<Image>().sprite = isNotPressedImage;
        }
    }

    public IEnumerator WaveIn(bool right,bool isOut)
    {
        if (isOut)
        {
            if (right)
            {
                _animator.Play("ButtonPushHorizontalRightout");
            }
            else
            {
                _animator.Play("ButtonPushHorizontal Leftout");
            }
        }
        else
        {
            yield return new WaitForSeconds(_animDuration);
            if (right)
            {
                _animator.Play("ButtonPushHorizontalRightin");
            }
            else
            {
                _animator.Play("ButtonPushHorizontal LeftIn");
            }
        }
    }
}
