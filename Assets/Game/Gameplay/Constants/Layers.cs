using UnityEngine;

// Why we do this? because always we call Unity's Vector3.zero it calls a function and generates call stack memory etc etc
namespace GameConstants {
    public static class Layers {
        /// <summary>Ground, GroundVisible</summary>
        public static readonly int GROUND = LayerMask.GetMask("Ground", "GroundVisible");

        /// <summary>Ground, GroundVisible, PreyOnlyCollisions</summary>
        public static readonly int GROUND_PREYCOL = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions");

        /// <summary>Ground, GroundVisible, PreyOnlyCollisions, Obstacle</summary>
        public static readonly int GROUND_PREYCOL_OBSTACLE = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions", "Obstacle");

        /// <summary>Ground, GroundVisible, PreyOnlyCollisions, Water</summary>
        public static readonly int GROUND_PREYCOL_WATER = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions", "Water");

        /// <summary>Ground, GroundVisible, Water</summary>
        public static readonly int GROUND_WATER = LayerMask.GetMask("Ground", "GroundVisible", "Water");

        /// <summary>Ground, GroundVisible, Water, AirPreys, GroundPreys, WaterPreys</summary>
        public static readonly int GROUND_WATER_APREYS_GPREYS_WPREYS = LayerMask.GetMask("Ground", "GroundVisible", "Water", "AirPreys", "GroundPreys", "WaterPreys");

        /// <summary>Ground, GroundVisible, Water, FireBlocker</summary>
        public static readonly int GROUND_WATER_FIREBLOCK = LayerMask.GetMask("Ground", "GroundVisible", "Water", "FireBlocker");

        ///<summary>Map</summary>
        public static readonly int MAP = LayerMask.GetMask("Map");

        /// <summary>Player</summary>
        public static readonly int PLAYER = LayerMask.GetMask("Player");

        /// <summary>Triggers</summary>
        public static readonly int TRIGGERS = LayerMask.GetMask("Triggers");

        /// <summary>Water</summary>
        public static readonly int WATER = LayerMask.GetMask("Water");

    }
}
