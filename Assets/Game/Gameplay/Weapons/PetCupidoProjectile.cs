using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCupidoProjectile : PetProjectile {

	protected override void DealDamage() {
        Entity entity = m_hitCollider.GetComponent<Entity>();
        if (entity != null) {
            
        }
    }
}
