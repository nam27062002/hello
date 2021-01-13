package com.fgol;

import java.util.Date;

import com.unity3d.player.UnityPlayer;

import android.content.Context;

public class FGOLPhonecallReciever extends PhonecallReceiver {
	
	private static String m_callbackObjectName = null;
	
	public static void Init(String callbackObjectName)
	{
		m_callbackObjectName = callbackObjectName;
		System.out.println("FGOLPhonecallReciever::Init " + callbackObjectName);
	}

	@Override
	protected void onIncomingCallReceived(Context ctx, String number, Date start) { 
		System.out.println("FGOLPhonecallReciever::onIncomingCallReceived");
		
		if (m_callbackObjectName != null) {
			UnityPlayer.UnitySendMessage(m_callbackObjectName, "onIncomingCallReceived", "");
		} else {
			System.out.println("FGOLPhonecallReciever::NotValidReciever");
		}
	}

	@Override
	protected void onIncomingCallAnswered(Context ctx, String number, Date start) { 
		System.out.println("FGOLPhonecallReciever::onIncomingCallAnswered");
		
		if (m_callbackObjectName != null) {
			UnityPlayer.UnitySendMessage(m_callbackObjectName, "onIncomingCallAnswered", "");
		} else {
			System.out.println("FGOLPhonecallReciever::NotValidReciever");
		}
	}

	@Override
	protected void onIncomingCallEnded(Context ctx, String number, Date start, Date end) {
		System.out.println("FGOLPhonecallReciever::onIncomingCallEnded");
		
		if (m_callbackObjectName != null) {
			UnityPlayer.UnitySendMessage(m_callbackObjectName, "onIncomingCallEnded", "");
		} else {
			System.out.println("FGOLPhonecallReciever::NotValidReciever");
		}
	}

	@Override
	protected void onOutgoingCallStarted(Context ctx, String number, Date start) {
		System.out.println("FGOLPhonecallReciever::onOutgoingCallStarted");
		
		if (m_callbackObjectName != null) {
			UnityPlayer.UnitySendMessage(m_callbackObjectName, "onOutgoingCallStarted", "");
		} else {
			System.out.println("FGOLPhonecallReciever::NotValidReciever");
		}
	}

	@Override
	protected void onOutgoingCallEnded(Context ctx, String number, Date start, Date end) {
		System.out.println("FGOLPhonecallReciever::onOutgoingCallEnded ");
		
		if (m_callbackObjectName != null) {
			UnityPlayer.UnitySendMessage(m_callbackObjectName, "onOutgoingCallEnded", "");
		} else {
			System.out.println("FGOLPhonecallReciever::NotValidReciever");
		}
	}

	@Override
	protected void onMissedCall(Context ctx, String number, Date start) {
		System.out.println("FGOLPhonecallReciever::onMissedCall");
		
		if (m_callbackObjectName != null) {
			UnityPlayer.UnitySendMessage(m_callbackObjectName, "onMissedCall", "");
		} else {
			System.out.println("FGOLPhonecallReciever::NotValidReciever");
		}
	}

}
