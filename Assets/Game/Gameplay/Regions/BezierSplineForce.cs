
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGOL;
// using Assets.Code.Game.Currents;

namespace Assets.Code.Game.Spline
{
    [RequireComponent(typeof(BezierSpline))]
	// [ExecuteBefore(typeof(Machine))]
    public class BezierSplineForce : MonoBehaviour
    {
        private class SplineForcedObject
        {
            public BezierSpline spline;

            public GameObject obj;

			public AI.MachineOld objMachine;
			public DragonMotion objDragon;

            public float magnitude;

			private float t;

			//Control variables to properly apply decreasing force to the object after exiting the current ( prevents stopping abruptedly )
			private bool isExitingCurrent;
			private float currentTimeOutsideCurrent;
           	private bool hasCompletelyExitCurrent;
			private float timeToLerpForceAfterExit;

			private int previousStep;
			private float timeSinceLastStepChange;
			private Vector3 previousTangent;

            private bool decreaseForceAlongSpline;

			public SplineForcedObject(BezierSpline _spline, GameObject _obj, float _magnitude, float _timeToLerpForceAfterExit, bool decreaseForceAlongSpline)
            {
                spline = _spline;
                obj = _obj;
				magnitude = _magnitude;
				t = 0.0f;
				objMachine = obj.GetComponent<AI.MachineOld>();
				objDragon = obj.GetComponent<DragonMotion>();

				//Control variables to properly apply decreasing force to the object after exiting the current ( prevents stopping abruptedly )
				isExitingCurrent = false;
				hasCompletelyExitCurrent = false;
				currentTimeOutsideCurrent = 0.0f;
				timeToLerpForceAfterExit = _timeToLerpForceAfterExit;

				previousStep = 0;
				timeSinceLastStepChange = 0.0f;

                this.decreaseForceAlongSpline = decreaseForceAlongSpline;
            }

            public void FixedUpdate()
            {
				int step = 0;
				spline.GetClosestPointToPoint (obj.transform.position, NumSteps, out t, out step);

				Vector3 tangent = spline.GetTangent (t);
				Vector3 velocity = tangent * magnitude;	// no need to use deltaTime now this is a velocity

				//When againgst current, if we recently changes segments, lerp the tangent from the current segment's tanget and the previous segmnet's tangent
				if (objMachine != null || objDragon != null)
				{
                    if (step != previousStep)
                    {
                        velocity = EaseInStepChange (velocity, tangent, step);                        
					}

                    if (decreaseForceAlongSpline)
                    {
                        velocity *= (1.0f - t);
                    }                    
				}
					

				//If it has been set as 'hasExitCurrent', it means we have exit the current a few moments ago
				//so we have to apply still some force to the object. Force will decrease with time, until the moment that
				//he will be deleted from the current list completedly
				if(isExitingCurrent)
				{
					//Lerp a decreasing factor that we will multiply to the current force to simulate that the current affects us less and less
					float outOfCurrentFactor = 0;
					if (timeToLerpForceAfterExit > 0)
						Mathf.Lerp ( 1.0f, 0.0f, ( currentTimeOutsideCurrent )/ timeToLerpForceAfterExit );
					velocity *= outOfCurrentFactor;
					currentTimeOutsideCurrent += Time.fixedDeltaTime;

					if( outOfCurrentFactor <= 0.0f )
					{
						isExitingCurrent = false;
						hasCompletelyExitCurrent = true;
					}
				}

                //Debug.Log("Adding velocity = " + velocity);
				if(objMachine != null)
					objMachine.AddExternalForce(velocity);
				if ( objDragon != null )
					objDragon.AddExternalForce( velocity );
            }

			//Lerps the tangent of the current segment of the spline with the tangent of the previous segment for 0.5 second.
			//Prevents direction changes to happen in one frame, looks ugly.
			private Vector3 EaseInStepChange( Vector3 velocity, Vector3 tangent, int step )
			{
				timeSinceLastStepChange += Time.fixedDeltaTime * 5.0f;

                if ( timeSinceLastStepChange < 1.0f )
                {
					tangent = Vector3.Lerp( previousTangent, tangent, timeSinceLastStepChange );
					velocity = tangent * magnitude;

				} else {

					timeSinceLastStepChange = 0.0f;
					previousStep = step;
					previousTangent = tangent;
				}

				return velocity;
			}


			public bool IsInCurrentDirection()
			{
				int step = 0;
				spline.GetClosestPointToPoint(obj.transform.position, NumSteps, out t, out step);

				Vector3 tangent = spline.GetTangent(t);
				Vector3 velocity = tangent * magnitude;

				if(objMachine != null)
				{
					float angle = Vector3.Dot(velocity, objMachine.direction);
                    if(angle > 0)
					{
						return true;
					}
				}
				else if ( objDragon != null ) 
				{
					float angle = Vector3.Dot(velocity, objDragon.direction);
                    if(angle > 0)
					{
						return true;
					}
				}

				return false;
			}

			public void OnCurrentExit( ) {
				isExitingCurrent = true;
				currentTimeOutsideCurrent = 0.0f;
			}

			public bool IsCompletelyOutOfCurrent( ) {
				return hasCompletelyExitCurrent;
			}
        }

        private List<SplineForcedObject> m_objsUnderInfluence = new List<SplineForcedObject>();
        private BezierSpline m_spline;

        private List<SplineForcedObject> m_deletedObjs = new List<SplineForcedObject>();
        private List<SplineForcedObject> m_duplicatedObjs = new List<SplineForcedObject>();

        private const int NumSteps = 10;

        [SerializeField]
        private float m_magnitude = 5.0f;

		[SerializeField]
		private float m_timeToLerpForceAfterExit = 2.0f;

        [SerializeField]
        private bool m_decreaseForceAlongSpline;

        void Awake()
        {
            m_spline = this.GetComponent<BezierSpline>();
        }

        public void AddObject(GameObject obj)
        {
            Assert.Expect(obj != null);

            //Debug.Log("Adding obj " + obj.name);

			//If the same object is already on the list, delete that entry and use a fresh new one.
			m_duplicatedObjs.Clear();
			for(int i = 0; i < m_objsUnderInfluence.Count; i++)
			{
				SplineForcedObject duplicatedForcedObj = m_objsUnderInfluence[i];

				if( duplicatedForcedObj.obj == obj ) {
					m_duplicatedObjs.Add( duplicatedForcedObj );
				}
			}

			for(int i = 0; i < m_duplicatedObjs.Count; i++)
			{
				m_objsUnderInfluence.Remove(m_duplicatedObjs[i]);
			}

			SplineForcedObject forcedObj = new SplineForcedObject(m_spline, obj, m_magnitude, m_timeToLerpForceAfterExit, m_decreaseForceAlongSpline);
            m_objsUnderInfluence.Add(forcedObj);
        }

        //Can remove an object from the current instantly, or gently apply an exit force to him over time before deleting it.
		public void RemoveObject(GameObject obj, bool removeInstantly)
		{
			SplineForcedObject removalObj = null;
			//After exiting a current, do not remove straight away from the objects under influence, instead, mark it as a "hasExitCurrent",
			//which will cause him to graduatelly get less force applied from the current ( this prevents to stop abruptedly after exiting a current )
			for(int i = 0; i < m_objsUnderInfluence.Count; i++)
			{
				SplineForcedObject forcedObj = m_objsUnderInfluence[i];
				if(forcedObj.obj == obj)
				{
					if(removeInstantly)
					{
						removalObj = forcedObj;
					} else
					{
						forcedObj.OnCurrentExit( );
					}
					break;
				}
			}

			if(removalObj != null)
			{
				//Debug.Log("Removing obj " + obj.name);
				m_objsUnderInfluence.Remove(removalObj);
			}
		}

		public bool IsInCurrentDirection(GameObject gameObject)
		{
			SplineForcedObject splinedObject = null;
			for(int i = 0; i < m_objsUnderInfluence.Count; i++)
			{
				if(m_objsUnderInfluence[i].obj == gameObject)
				{
					splinedObject = m_objsUnderInfluence[i];
					break;
                }
			}

			if(splinedObject == null)
			{
				return false;
			}
			else
			{
				return splinedObject.IsInCurrentDirection();
			}
		}

        void FixedUpdate()
        {
            m_deletedObjs.Clear();

			for(int i = 0; i < m_objsUnderInfluence.Count; i++)
            {
				SplineForcedObject obj = m_objsUnderInfluence[i];
				//if the object has been  destroyed or disabled ( new! added on 28-10 by Xavi ), we mark it for deletion
                if(obj.obj != null && obj.obj.activeSelf == true )
                {
					//Only mark for deletion the objects that are completely out of the current
					//Which means, they are outside of the influence zone of the current, and the "onExit" force has been applied and finalised.
					if(obj.IsCompletelyOutOfCurrent( ))
					{
						m_deletedObjs.Add(obj);
					} else {
                    	obj.FixedUpdate();
					}
                }
                else
                {
                    m_deletedObjs.Add(obj);
                }
            }

			for(int i = 0; i < m_deletedObjs.Count; i++)
            {
                m_objsUnderInfluence.Remove(m_deletedObjs[i]);
            }
        }
    }
}
