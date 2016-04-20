using UnityEngine;
using System.Collections;

public class MicTest : MonoBehaviour 
{
	string m_MicDeviceName = null;
	int m_ReadHead = 0;
	AudioClip m_AudioClip = null;
	const int m_BufferSize = 1024;
	float[] m_Samples = null;

	DragonBreathBehaviour m_dragon;
	
	// Use this for initialization
	void Start () 
	{
		m_Samples = new float[m_BufferSize];
		
		int minFreq;
		int maxFreq;	
		
		Microphone.GetDeviceCaps(m_MicDeviceName, out minFreq, out maxFreq);
		m_AudioClip = Microphone.Start(m_MicDeviceName, true, 5, minFreq); 
		m_ReadHead = 0;


		m_dragon = GetComponent<DragonBreathBehaviour>();
	}
	
	void Update()
	{
		if ( Microphone.IsRecording(m_MicDeviceName) )
		{
			int writeHead =  Microphone.GetPosition( m_MicDeviceName );
			if( m_ReadHead == writeHead )
				return;
				
			int floatsToGet =  ( m_AudioClip.samples  +  writeHead - m_ReadHead ) % m_AudioClip.samples;
			
			while( floatsToGet >= m_BufferSize )
			{
				m_AudioClip.GetData( m_Samples, m_ReadHead);
				m_ReadHead = ( m_ReadHead + m_BufferSize ) % m_AudioClip.samples;	
				floatsToGet -= m_BufferSize;
				
				// Check audio
				for( int i = 0; i<m_BufferSize; i++ )
				{
					if ( m_Samples[i] > 0.9f )
					{
						// Try to fire Dragon!!!!
						m_dragon.AddFury(5000);
					}
				}
			}
			
		}
	}
	
	void OnDestroy()
	{
		Microphone.End( m_MicDeviceName );
		m_AudioClip = null;
	}
}
