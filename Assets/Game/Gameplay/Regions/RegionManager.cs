using FGOL;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Game.Currents
{
    public class RegionManager : ScopedSingleton<RegionManager>
    {
		public Current CheckIfObjIsInCurrent(GameObject obj, float _multiplier)
        {
            Current curr = null;
            if(IsObjInsideCurrent(obj, out curr))
            {
				curr.splineForce.AddObject(obj, _multiplier);
            }
            return curr;
        }

        public bool IsObjInsideCurrent(GameObject obj, out Current resultCurrent)
        {
            Current[] currs = RegionHolder.m_currents;
            for(int i = 0; i < currs.Length; i++)
            {
                Current curr = currs[i];

                Vector3 objPos = obj.transform.position;
                if(curr.Contains(objPos.x, objPos.y))
                {
                    resultCurrent = curr;
                    return true;
                }
            }
            resultCurrent = null;
            return false;
        }
        
        public bool IsObjInsideMissionArea(GameObject obj, MissionArea area, out MissionArea resultCurrent)
        {
            Region[] currs = RegionHolder.m_missionAreas;
            for (int i = 0; i < currs.Length; i++)
            {
                
            }
            resultCurrent = null;
            return false;
        }
    }
}
