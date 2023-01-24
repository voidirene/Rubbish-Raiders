using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(Lane))]
public class LaneEditor : Editor
{
    private Lane lane;

    private void OnSceneGUI()
    {
        lane = target as Lane;
        ref Transform ms = ref lane.managerSpawn;

        Vector3 labelOffset = 2.0f * Vector3.up;

        Vector3 add = Vector3.zero;
        if (!Application.isPlaying)
		{
            add = lane.transform.position;
		}

        Handles.Label(ms.position + add + labelOffset , "Manager Spawn");
        EditorGUI.BeginChangeCheck();
        Vector3 msp = Handles.DoPositionHandle(ms.position + add, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ms, "Lane Manager Point");
            EditorUtility.SetDirty(ms);
            msp -= add;
            ms.position = msp;
        }

        EditorGUI.BeginChangeCheck();
        Handles.ConeHandleCap(0, ms.position + add + 1f * (ms.rotation * Vector3.forward), ms.rotation, 1.3f, EventType.Repaint);
        Quaternion msr = Handles.DoRotationHandle(ms.rotation, ms.position + add);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ms, "Lane Manager Point");
            EditorUtility.SetDirty(ms);
            Vector3 eulers = msr.eulerAngles;
            eulers.x = 0.0f;
            eulers.z = 0.0f;
            ms.rotation = Quaternion.Euler(eulers);
        }

        Handles.Label(lane.truckSkipPosition + add + labelOffset, "Truck skip position");
        EditorGUI.BeginChangeCheck();
        Vector3 truckSkipPosition = Handles.DoPositionHandle(lane.truckSkipPosition + add, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
		{
            Undo.RecordObject(lane, "Moved Skip Position");
            EditorUtility.SetDirty(lane);
            truckSkipPosition -= add;
            lane.truckSkipPosition = truckSkipPosition;
		}

        Handles.Label(lane.workerSkipPosition + add + labelOffset, "Worker skip position");
        EditorGUI.BeginChangeCheck();
        Vector3 workerSkipPosition = Handles.DoPositionHandle(lane.workerSkipPosition + add, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(lane, "Moved Skip Position");
            EditorUtility.SetDirty(lane);
            workerSkipPosition -= add;
            lane.workerSkipPosition = workerSkipPosition;
        }

        Handles.Label(lane.farEndPosition + add + labelOffset, "Far end position");
        EditorGUI.BeginChangeCheck();
        Vector3 farEndPosition = Handles.DoPositionHandle(lane.farEndPosition + add, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(lane, "Moved Far End Position");
            EditorUtility.SetDirty(lane);
            farEndPosition -= add;
            lane.farEndPosition = farEndPosition;
        }
    }
}