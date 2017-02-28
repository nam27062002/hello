using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class GameStoreManager 
{

	#region singleton
    // Singleton ///////////////////////////////////////////////////////////
	private static GameStoreManager s_pInstance = null;

	public static GameStoreManager SharedInstance
    {
        get
        {
            if (s_pInstance == null)
            {
                // Calety implementation is used
				s_pInstance = new GameStoreManagerCalety();
            }

            return s_pInstance;
        }
    }
    #endregion

    public virtual void Initialize(){}
    public virtual bool IsReady(){ return false; }
	public virtual string GetLocalisedPrice( string sku ){ return ""; }
    public virtual bool CanMakePayment(){ return false; }
    public virtual void Buy( string sku ){}


}
