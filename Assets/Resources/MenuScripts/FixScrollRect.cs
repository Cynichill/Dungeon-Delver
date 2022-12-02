using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class FixScrollRect: MonoBehaviour, IBeginDragHandler,  IDragHandler, IEndDragHandler, IScrollHandler
{
    public ScrollRect MainScroll;
 
 
    public void OnBeginDrag(PointerEventData eventData)
    {
        MainScroll.OnBeginDrag(eventData);
    }
 
 
    public void OnDrag(PointerEventData eventData)
    {
        MainScroll.OnDrag(eventData);
    }
 
    public void OnEndDrag(PointerEventData eventData)
    {
        MainScroll.OnEndDrag(eventData);
    }
 
 
    public void OnScroll(PointerEventData data)
    {
        MainScroll.OnScroll(data);
    }
 
 
}
 
 