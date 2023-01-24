using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Commented out because this has all been moved into Alerts.cs
 *    Amie 04/04/22
 *
public class AlertMovement : MonoBehaviour
{
    public Vector3 targetTransform;

    private Camera cam;

    private RectTransform rectTransform;

    private void Start()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        /* COMMENTED OUT CAUSE THERE'S NO ARROW TO ROTATE (YET)
        //rotate the arrow
        Vector3 fromPosition = cam.transform.position;
        fromPosition.z = 0;
        Vector3 direction = (targetTransform - fromPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; //this may need to be changed to use X and Z instead of X and Y
        if (angle < 0) angle += 360;
        rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        float borderOffset = 100f;
        //check if the alert's target is on screen or not
        Vector3 targetScreenPoint = cam.WorldToScreenPoint(targetTransform);
        bool isTargetOffScreen = 
            targetScreenPoint.x <= borderOffset || 
            targetScreenPoint.x >= Screen.width - borderOffset || 
            targetScreenPoint.y <= borderOffset || 
            targetScreenPoint.y >= Screen.height - borderOffset;

        //if the target is offscreen, move alert around the edge of the screen
        if (isTargetOffScreen)
        {
            Vector3 cappedTargetScreenPosition = targetScreenPoint;
            cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderOffset, Screen.width - borderOffset);
            cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderOffset, Screen.height - borderOffset);

            rectTransform.position = cappedTargetScreenPosition;
        }
        else
        {
            Debug.Log("Else triggered, TODO: make alert disappear and the clear waste button red.");
        }
    }
}
        */
