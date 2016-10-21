using FGOL;
using UnityEngine;

public class RegionHolder : MonoBehaviour
{
    public static Current[] m_currents = new Current[0];
    public static MissionArea[] m_missionAreas = new MissionArea[0];

    public Current[] m_levelCurrents = new Current[0];
    public MissionArea[] m_levelMissionAreas = new MissionArea[0];

    void Awake()
    {
        m_currents = m_levelCurrents;
        m_missionAreas = m_levelMissionAreas;
    }
}