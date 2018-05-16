


using UnityEngine;
using System.Collections;


public class JoystickControls : MonoBehaviour
{
    protected Vector2 m_sharkDesiredVel = Vector2.zero;
    public Vector2 SharkDesiredVel { get { return m_sharkDesiredVel; } }
    float vX;
    float vY;
    bool moving = false;

    public void UpdateJoystickControls()
    {
        vX = Input.GetAxis("Horizontal");
        vY = Input.GetAxis("Vertical");
		moving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    }


    public bool isMoving()
	{
		return moving;
	}

    public bool getAction()
    {
        return Input.GetKey(KeyCode.JoystickButton0);
    }


    public void CalcSharkDesiredVelocity(float speed)
    {
        // normalize the distance of the click in world units away from the shark, by the max click distance
        // m_sharkDesiredVel.x = m_diffVecNorm.x * speed * m_speedDampenMult * m_decelerationMult;
        // m_sharkDesiredVel.y = m_diffVecNorm.y * speed * m_speedDampenMult * m_decelerationMult;
        m_sharkDesiredVel.x = vX * speed;
        m_sharkDesiredVel.y = vY * speed;
    }
}
