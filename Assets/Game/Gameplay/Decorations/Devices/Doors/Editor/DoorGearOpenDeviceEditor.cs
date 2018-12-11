using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(DoorGearOpenDevice))]
public class DoorGearOpenDeviceEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Advance animation")) {
            DoorGearOpenDevice device = target as DoorGearOpenDevice;
            device.transform.position += GameConstants.Vector3.zero;
            device.DebugAnimation(Time.fixedDeltaTime);
        }
    }
}
