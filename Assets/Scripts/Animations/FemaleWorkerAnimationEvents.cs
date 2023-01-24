using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FemaleWorkerAnimationEvents : MonoBehaviour
{
    public void AddWasteToHand()
    {
        Worker w = GetComponentInParent<Worker>();

        //Transform bone = transform.Find("wokerRigHIPS001/wokerRigSpine1/wokerRigSpine2/wokerRigHIPS002/wokerRigRArmCollarbone/wokerRigRArm11/wokerRigRArm12/wokerRigRArm13/wokerRigRArm21/wokerRigRArm22/wokerRigRArm23/wokerRigRArmPalm");
        Transform bone = transform.Find("wokerRigHIPS003/wokerRigSpine004/wokerRigSpine005/wokerRigHIPS004/wokerRigLArmCollarbone001/wokerRigLArm024/wokerRigLArm025/wokerRigLArm026/wokerRigLArm027/wokerRigLArm028/wokerRigLArm029/wokerRigLArmPalm001"); // lmao
        w.heldWaste.transform.parent = bone;
        w.heldWaste.transform.localPosition = Vector3.zero;
        w.heldWaste.transform.localRotation = Quaternion.identity;
    }

    public void RemoveWasteFromHand()
    {
        Worker w = GetComponentInParent<Worker>();

        Vector3 position = w.heldWaste.transform.position;
        position.y = 0.0f;
        w.heldWaste.transform.parent = null;
        w.heldWaste.hasBeenTargeted = false;
        w.heldWaste.transform.SetPositionAndRotation(position, Quaternion.identity);
        w.heldWaste = null;
    }
}
