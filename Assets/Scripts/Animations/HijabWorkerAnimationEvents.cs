using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HijabWorkerAnimationEvents : MonoBehaviour
{
    public void AddWasteToHand()
    {
        Worker w = GetComponentInParent<Worker>();

        Transform bone = transform.Find("wokerRigHIPS005/wokerRigSpine007/wokerRigSpine008/wokerRigHIPS006/wokerRigLArmCollarbone002/wokerRigLArm030/wokerRigLArm031/wokerRigLArm032/wokerRigLArm033/wokerRigLArm034/wokerRigLArm035/wokerRigLArmPalm002"); // lmao
        w.heldWaste.transform.parent = bone;
        w.heldWaste.transform.localPosition = Vector3.zero;
        w.heldWaste.transform.localRotation = Quaternion.identity;
    }

    public void RemoveWasteFromHand()
    {
        Worker w = GetComponentInParent<Worker>();

        Vector3 position = w.heldWaste.transform.position;
        position.y = 0.0f;
        w.heldWaste.transform.SetPositionAndRotation(position, Quaternion.identity);

        w.heldWaste.hasBeenTargeted = false;
        w.heldWaste.transform.parent = null;
        w.heldWaste = null;
    }
}
