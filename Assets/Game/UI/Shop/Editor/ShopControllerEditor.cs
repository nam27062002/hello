using UnityEngine;
using UnityEditor;

namespace Shop { 
    [CustomEditor(typeof(ShopController))]
    public class ShopControllerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ShopController shop = (ShopController)target;
            if (GUILayout.Button("Enable Optimization"))
            {
                shop.SetOptimizationActive(true);
            }

            if (GUILayout.Button("Disable Optimization"))
            {
                shop.SetOptimizationActive(false);
            }
        }
    }
}