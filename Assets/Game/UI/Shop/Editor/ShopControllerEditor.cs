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
                shop.useOptimization = true;
                // Optimization will be applied in the next update
            }

            if (GUILayout.Button("Disable Optimization"))
            {
                shop.useOptimization = false;
                shop.SetOptimizationActive(false);
            }
        }
    }
}