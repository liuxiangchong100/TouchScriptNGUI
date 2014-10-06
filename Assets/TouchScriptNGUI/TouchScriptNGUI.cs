using UnityEngine;
using System.Collections;
using TouchScript;
using TouchScript.Gestures;

public class TouchScriptNGUI : MonoBehaviour
{

    UICamera uiCamera;

    void Start()
    {
        uiCamera = GameObject.FindObjectOfType(typeof(UICamera)) as UICamera;

        // Disable standard NGUI inputs
        uiCamera.useMouse = false;
        uiCamera.useTouch = false;

        TouchManager.Instance.TouchesBegan += TouchManagerBegan;
        TouchManager.Instance.TouchesMoved += TouchManagerMoved;
        TouchManager.Instance.TouchesEnded += TouchManagerEnded;
        TouchManager.Instance.TouchesCancelled += TouchManagerCancelled;
    }

    private void TouchManagerBegan(object sender, TouchEventArgs eventArgs)
    {
        TouchManagerChanged(sender, eventArgs, TouchPhase.Began);
    }
    private void TouchManagerMoved(object sender, TouchEventArgs eventArgs)
    {
        TouchManagerChanged(sender, eventArgs, TouchPhase.Moved);
    }
    private void TouchManagerEnded(object sender, TouchEventArgs eventArgs)
    {
        TouchManagerChanged(sender, eventArgs, TouchPhase.Ended);
    }
    private void TouchManagerCancelled(object sender, TouchEventArgs eventArgs)
    {
        TouchManagerChanged(sender, eventArgs, TouchPhase.Canceled);
    }
    private void TouchManagerChanged(object sender, TouchEventArgs eventArgs, TouchPhase touchPhase)
    {
        TouchScript.TouchManager gesture = sender as TouchScript.TouchManager;

        foreach (TouchScript.ITouch touchPoint in eventArgs.Touches)
        {
            UICamera.currentTouchID = uiCamera.allowMultiTouch ? touchPoint.Id : 1;
            UICamera.currentTouch = UICamera.GetTouch(UICamera.currentTouchID);

            bool pressed = (touchPhase == TouchPhase.Began) || UICamera.currentTouch.touchBegan;
            bool unpressed = (touchPhase == TouchPhase.Canceled) || (touchPhase == TouchPhase.Ended);
            UICamera.currentTouch.touchBegan = false;

            if (pressed)
            {
                UICamera.currentTouch.delta = Vector2.zero;
            }
            else
            {
                UICamera.currentTouch.delta = touchPoint.PreviousPosition - touchPoint.Position;
            }

            UICamera.currentTouch.pos = touchPoint.Position;
            UICamera.hoveredObject = UICamera.Raycast(UICamera.currentTouch.pos) ? UICamera.lastHit.collider.gameObject : UICamera.fallThrough;
            if (UICamera.hoveredObject == null) UICamera.hoveredObject = UICamera.genericEventHandler;
            UICamera.currentTouch.current = UICamera.hoveredObject;
            UICamera.lastTouchPosition = UICamera.currentTouch.pos;

            // We don't want to update the last camera while there is a touch happening
            if (pressed) UICamera.currentTouch.pressedCam = UICamera.currentCamera;
            else if (UICamera.currentTouch.pressed != null) UICamera.currentCamera = UICamera.currentTouch.pressedCam;

            // Double-tap support
            //      if (input.tapCount > 1) currentTouch.clickTime = Time.realtimeSinceStartup;

            // Process the events from this touch
            uiCamera.ProcessTouch(pressed, unpressed);

            // If the touch has ended, remove it from the list
            if (unpressed) UICamera.RemoveTouch(UICamera.currentTouchID);
            UICamera.currentTouch = null;

            

            // Don't consider other touches
            if (!uiCamera.allowMultiTouch) break;
        }
    }
}