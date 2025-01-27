using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour
{
    public bool IsInit;

    public virtual void Initialize()
    {
        IsInit = true;
    }
    public virtual void Show(object args=null)
    {
        gameObject.SetActive(true);
    }
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
