using UnityEngine;
using System.Collections;

public class SpawnerCage : Spawner {

    protected override void RegisterInEntityManager(IEntity _e) {
		EntityManager.instance.RegisterEntityCage(_e as Cage);
    }

    protected override void UnregisterFromEntityManager(IEntity _e) {
		EntityManager.instance.UnregisterEntityCage(_e as Cage);
    }
}
