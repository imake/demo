
using UnityEngine;
using UnityEngine.EventSystems;

public class DragListener : MonoBehaviour, IEventSystemHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IUpdateSelectedHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public VoidDataDelegate onInitializePotentialDrag;
    public VoidDataDelegate onBeginDrag;
    public VoidDataDelegate onDrag;
    public VoidDataDelegate onEndDrag;
    public VoidDataDelegate onDrop;

    public VoidDataDelegate onClickDown;
    public VoidDataDelegate onClickUp;
    public VoidDataDelegate onClick;

    public VoidBaseDataDelegate onUpdateSelect;

    [HideInInspector]
    public BoxCollider2D boxCollider2D;
    [HideInInspector]
    public BoxCollider boxCollider;

    public static DragListener Get(GameObject go)
    {
        DragListener listener = go.GetComponent<DragListener>();
        if (listener == null)
            listener = go.AddComponent<DragListener>();
        return listener;
    }

    public static DragListener Get(Component component)
    {
        return Get(component.gameObject);
    }

    public static DragListener GetFormCC2D(GameObject go, bool isAutoCreateCollider = true)
    {
        if (isAutoCreateCollider)
        {
            if (!go.GetComponent<CapsuleCollider2D>())
            {
                go.AddComponent<CapsuleCollider2D>();
            }
        }
        return Get(go);
    }

    public static DragListener GetForm2D(GameObject go, bool isAutoCreateCollider = true)
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

    public static DragListener GetForm2D(Component component, bool isAutoCreateCollider = true)
    {
        return GetForm2D(component.gameObject, isAutoCreateCollider);
    }

    public static DragListener GetForm3D(GameObject go, bool isAutoCreateCollider = true)
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

    public static DragListener GetForm3D(Component component, bool isAutoCreateCollider = true)
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

    public void OnInitializePotentialDrag(PointerEventData data)
    {
        if (onInitializePotentialDrag != null) onInitializePotentialDrag(data);
    }
    public void OnBeginDrag(PointerEventData data)
    {
        if (onBeginDrag != null) onBeginDrag(data);
    }
    public void OnDrag(PointerEventData data)
    {
        if (onDrag != null) onDrag(data);
    }
    public void OnEndDrag(PointerEventData data)
    {
        if (onEndDrag != null) onEndDrag(data);
    }
    public void OnDrop(PointerEventData data)
    {
        if (onDrop != null) onDrop(data);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (onClickDown != null) onClickDown(data);
    }
    public void OnPointerUp(PointerEventData data)
    {
        if (onClickUp != null) onClickUp(data);
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (onClick != null) onClick(data);
    }

    public void OnUpdateSelected(BaseEventData data)
    {
        if (onUpdateSelect != null) onUpdateSelect(data);
    }

    public void ClearAllEvent()
    {
        onInitializePotentialDrag = null;
        onBeginDrag = null;
        onDrag = null;
        onEndDrag = null;
        onDrop = null;

        onClickDown = null;
        onClickUp = null;
        onClick = null;

        onUpdateSelect = null;
    }

    private void OnDestroy()
    {
        ClearAllEvent();
    }
}