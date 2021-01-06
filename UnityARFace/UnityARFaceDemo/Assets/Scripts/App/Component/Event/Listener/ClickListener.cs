
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickListener : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public VoidDataDelegate onPointerEnter;
    public VoidDataDelegate onPointerExit;

    public VoidDataDelegate onClickDown;
    public VoidDataDelegate onClickUp;
    public VoidDataDelegate onClick;
    public VoidDataDelegate onDblClick;

    public VoidGameObjectDelegate onLongPress;
    public VoidDataDelegate onLongPressUp;

    public float longPressDuration = 1.0f;

    private bool isPointerDown;
    private bool isLongPressTriggered;
    private float pointerDownTime;

    [HideInInspector]
    public BoxCollider2D boxCollider2D;
    [HideInInspector]
    public BoxCollider boxCollider;

    public static ClickListener Get(GameObject go)
    {
        ClickListener listener = go.GetComponent<ClickListener>();
        if (listener == null)
            listener = go.AddComponent<ClickListener>();
        return listener;
    }

    public static ClickListener Get(Component component)
    {
        return Get(component.gameObject);
    }

    public static ClickListener GetFormCC2D(GameObject go, bool isAutoCreateCollider = true)
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

    public static ClickListener GetForm2D(GameObject go, bool isAutoCreateCollider = true)
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

    public static ClickListener GetForm2D(Component component, bool isAutoCreateCollider = true)
    {
        return GetForm2D(component.gameObject, isAutoCreateCollider);
    }

    public static ClickListener GetForm3D(GameObject go, bool isAutoCreateCollider = true)
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

    public static ClickListener GetForm3D(Component component, bool isAutoCreateCollider = true)
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

    private void Update()
    {
        if (isPointerDown && !isLongPressTriggered)
        {
            if (Time.time - pointerDownTime >= longPressDuration)
            {
                isLongPressTriggered = true;
                //长按
                if (onLongPress != null) onLongPress(gameObject);
            }
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (onPointerEnter != null) onPointerEnter(data);
    }
    public void OnPointerExit(PointerEventData data)
    {
        if (onPointerExit != null) onPointerExit(data);

        isPointerDown = false;
        isLongPressTriggered = false;
    }

    private void InvokeOnClick(PointerEventData data)
    {
        if (onClick != null) onClick(data);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (onClickDown != null)
        {
            onClickDown(data);
        }

        isPointerDown = true;
        isLongPressTriggered = false;
        pointerDownTime = Time.time;
    }
    public void OnPointerUp(PointerEventData data)
    {
        if (onClickUp != null)
        {
            onClickUp(data);
        }

        isPointerDown = false;
        if (isLongPressTriggered)
        {
            //长按抬起
            if (onLongPressUp != null) onLongPressUp(data);
        }
    }
    public void OnPointerClick(PointerEventData data)
    {
        //双击
        if (onDblClick != null)
        {
            if (data.clickCount == 2)
            {
                onDblClick(data);
            }
        }
        //单击
        if (onLongPress != null)
        {
            if (!isLongPressTriggered)
            {
                InvokeOnClick(data);
            }
        }
        else
        {
            InvokeOnClick(data);
        }
    }

    public void ClearAllEvent()
    {
        onPointerEnter = null;
        onPointerExit = null;

        onClickDown = null;
        onClickUp = null;
        onClick = null;
        onDblClick = null;

        onLongPress = null;
        onLongPressUp = null;
    }

    private void OnDestroy()
    {
        ClearAllEvent();
    }
}