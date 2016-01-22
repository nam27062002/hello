using UnityEngine;
using System.Collections;

public class DeltaTimer
{

		//--------------------------------------------------------------------//
		// GENERIC METHODS													  //
		//--------------------------------------------------------------------//
		/**
		 * Default constructor
		 */
        public DeltaTimer()
        {
			mStartTime = 0;
			mLoop = false;
			mDuration = 0;
			mStopTime = 0;
			mStopped = false;
			mCurves = new CustomEase(CustomEase.EaseType.linear_01);
			mSpeedMultiplier = 1;
        }
		
		/**
		 *
		 */
        public virtual void Start(float duration, bool loop = false)
        {
			mDuration = duration;
			mStopTime = 0;
			mLoop = loop;
			mStopped = false;

			mStartTime = GetSystemTime();

			// Apply current speed multiplier to duration vars
			SetSpeedMultiplier(mSpeedMultiplier);

        }
		
		/**
		 * Stops the timer, subsequent calls to getTime/getDelta will return the same values until resume or start are called
		 */
		public virtual void Stop()
		{
			if (mStopped) return;
			
			mStopped = true;
			mStopTime = GetSystemTime() - mStartTime;
		}
		
		/**
		 * Resume a previously stopped timer
		 */
        public void Resume()
        {
			if (!mStopped) return;

			mStopped = false;
			mStartTime = GetSystemTime() - mStopTime;
        }
		
		/**
		 * Elapsed time since start in seconds
		 */
        public virtual float GetTime()
        {
			if (!mLoop)
			{
				if ( mStopped )
				{
					return mStopTime;
				}
				else
				{
					return GetSystemTime() - mStartTime;	
				}

			}
			else
			{
				if (mStopped)
            		return (float)(mStopTime%mDuration);
        		else
            		return (float)((GetSystemTime() - mStartTime)%mDuration);
			}
        }
		
		
		/**
		 * Get percentage of the timer completed. In 0.0...1.0 range. A curve function can be specified.
		 */
		public float GetDelta(CustomEase.EaseType curve = CustomEase.EaseType.linear_01)
        {
			mCurves.SetType(curve);
    		return mCurves.Get(GetTime(),mDuration);

        }
		
		/**
		 * True if the timer has reached it's end or it has never started
		 */
        public bool Finished()
        {
			return !mLoop && GetTime() > mDuration;
        }
		
		/**
		 * True if the timer has been manually stopped using the stop() method
		 */
        public bool IsStopped()
        {
			return mStopped;
        }
		
		/**
		 * Add time to the timer, useful to start animation from a middle point for instance
		 */
        public void AddTime(float time)
        {
        	mStartTime -= time;
        }
		
		/**
		 *
		 */
        public float GetTimeLeft()
        {
			float d = GetDelta();
		    if (d == 0) return mDuration;
		    float t = GetTime();
		    return ((1 - d) / d) * t;
        }
		
		/**
		 *
		 */
		public float GetDuration()
		{
			// Correct speed multiplier modification
			return mDuration * mSpeedMultiplier;
		}
		
		/**
		 *
		 */
		public void SetSpeedMultiplier(float _fSpeedMultiplier)
		{
			// Store new speed multiplier
			mSpeedMultiplier = _fSpeedMultiplier;
			
			// Change mDuration vars based on the new multiplier
			mDuration /= mSpeedMultiplier;
		}

		
		//--------------------------------------------------------------------//
        // STATIC INIT METHODS												  //
		//--------------------------------------------------------------------//
		/**
		 * [static] Returns current device's time in milliseconds
		 */
        static float GetSystemTime()
        {
        	return Time.realtimeSinceStartup;
        }


        float mStartTime;
        float mStopTime;

        float     mDuration;
        bool      mStopped;
        bool      mLoop;
		CustomEase     mCurves;
		float     mSpeedMultiplier;


}
