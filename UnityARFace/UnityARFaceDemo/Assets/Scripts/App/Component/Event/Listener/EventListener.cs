
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void VoidBaseDataDelegate(BaseEventData data);

public delegate void VoidDataDelegate(PointerEventData data);

public delegate void VoidGameObjectDelegate(GameObject gameObject);

public class EventListener : EventTrigger
{
    public VoidDataDelegate onClickDown;
    public VoidDataDelegate onClickUp;
    public VoidDataDelegate onClick;

    public VoidDataDelegate onInitializePotentialDrag;
    public VoidDataDelegate onBeginDrag;
    public VoidDataDelegate onDrag;
    public VoidDataDelegate onEndDrag;
    public VoidDataDelegate onDrop;

    public VoidDataDelegate onPointerEnter;
    public VoidDataDelegate onPointerExit;

    public VoidDataDelegate onScroll;

    public VoidBaseDataDelegate onSubmit;
    public VoidBaseDataDelegate onSelect;
    public VoidBaseDataDelegate onDeselect;
    public VoidBaseDataDelegate onUpdateSelect;

    [HideInInspector]
    public BoxCollider2D boxCollider2D;
    [HideInInspector]
    public BoxCollider boxCollider;

    public static EventListener Get(GameObject go)
    {
        EventListener listener = go.GetComponent<EventListener>();
        if (listener == null)
            listener = go.AddComponent<EventListener>();
        return listener;
    }

    public static EventListener Get(Component component)
    {
        return Get(component.gameObject);
    }

    public static EventListener GetForm2D(GameObject go, bool isAutoCreateCollider = true)
    {
        if (isAutoCreateCollider)
        {
            if (!go.GetComponent<BoxCollider2D>())
            {
                go.AddComponent<BoxCollider2D>();
            }
        }
        return Get(go);
    }

    public static EventListener GetForm2D(Component component, bool isAutoCreateCollider = true)
    {
        return GetForm2D(component.gameObject, isAutoCreateCollider);
    }

    public static EventListener GetForm3D(GameObject go, bool isAutoCreateCollider = true)
    {
        if (isAutoCreateCollider)
        {
            if (!go.GetComponent<BoxCollider>())
            {
                go.AddComponent<BoxCollider>();
            }
        }
        return Get(go);
    }

    public static EventListener GetForm3D(Component component, bool isAutoCreateCollider = true)
    {
        return GetForm3D(component.gameObject, isAutoCreateCollider);
    }

    public BoxCollider2D GetBoxCollider2D()
    {
        if (!boxCollider2D)
        {
            boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
        }
        return boxCollider2D;
    }

    public BoxCollider GetBoxCollider()
    {
        if (!boxCollider)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        return boxCollider;
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        if (onPointerEnter != null) onPointerEnter(data);
    }
    public override void OnPointerExit(PointerEventData data)
    {
        if (onPointerExit != null) onPointerExit(data);
    }

    public override void OnPointerDown(PointerEventData data)
    {
        if (onClickDown != null) onClickDown(data);
    }
    public override void OnPointerUp(PointerEventData data)
    {
        if (onClickUp != null) onClickUp(data);
    }
    public override void OnPointerClick(PointerEventData data)
    {
        if (onClick != null) onClick(data);
    }

    public override void OnInitializePotentialDrag(PointerEventData data)
    {
        if (onInitializePotentialDrag != null) onInitializePotentialDrag(data);
    }
    public override void OnBeginDrag(PointerEventData data)
    {
        if (onBeginDrag != null) onBeginDrag(data);
    }
    public override void OnDrag(PointerEventData data)
    {
        if (onDrag != null) onDrag(data);
    }
    public override void OnEndDrag(PointerEventData data)
    {
        if (onEndDrag != null) onEndDrag(data);
    }
    public override void OnDrop(PointerEventData data)
    {
        if (onDrop != null) onDrop(data);
    }

    public override void OnScroll(PointerEventData data)
    {
        if (onScroll != null) onScroll(data);
    }

    public override void OnSubmit(BaseEventData data)
    {
        if (onSubmit != null) onSubmit(data);
    }
    public override void OnSelect(BaseEventData data)
    {
        if (onSelect != null) onSelect(data);
    }
    public override void OnDeselect(BaseEventData data)
    {
        if (onDeselect != null) onDeselect(data);
    }
    public override void OnUpdateSelected(BaseEventData data)
    {
        if (onUpdateSelect != null) onUpdateSelect(data);
    }

    public void ClearAllEvent()
    {
        onClickDown = null;
        onClickUp = null;
        onClick = null;

        onInitializePotentialDrag = null;
        onBeginDrag = null;
        onDrag = null;
        onEndDrag = null;
        onDrop = null;

        onPointerEnter = null;
        onPointerExit = null;

        onScroll = null;

        onSubmit = null;
        onSelect = null;
        onDeselect = null;
        onUpdateSelect = null;
    }

    private void OnDestroy()
    {
        ClearAllEvent();
    }
}
