using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
    namespace Behaviour {

        [System.Serializable]
        public class SearchEntityTargetData : StateComponentData {
            [Comment("Comma Separated list", 5)]
            public string preferedEntitiesList;
            public string searchButNoEatEntityList;
            public bool ignoreNotListedUnits = false;
            public float sightRadius = 10;
        }

        [CreateAssetMenu(menuName = "Behaviour/Attack/Search Entity Target")]
        public class SearchEntityTarget : StateComponent {

            [StateTransitionTrigger]
            private static readonly int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");


            private DragonTier m_eaterTier;

            private Entity[] m_checkEntities = new Entity[40];
            private int m_numCheckEntities = 0;

            private int m_collidersMask;

            EatBehaviour m_eatBehaviour;

            private SearchEntityTargetData m_data;
            private List<string> m_preferedEntities = new List<string>();
            private List<string> m_searchButNoEatList = new List<string>();


            public override StateComponentData CreateData() {
                return new SearchEntityTargetData();
            }

            public override System.Type GetDataType() {
                return typeof(SearchEntityTargetData);
            }

            protected override void OnInitialise() {
                // Temp
                MachineEatBehaviour machineEat = m_pilot.GetComponent<MachineEatBehaviour>();
                if (machineEat)
                    m_eaterTier = machineEat.eaterTier;

                m_collidersMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Obstacle");

                base.OnInitialise();


                m_data = m_pilot.GetComponentData<SearchEntityTargetData>();

                if (!string.IsNullOrEmpty(m_data.preferedEntitiesList)) {
                    // Use the separator string to split the string value
                    string[] splitResult = m_data.preferedEntitiesList.Split(new string[] { "," }, StringSplitOptions.None);
                    m_preferedEntities = new List<string>(splitResult);
                }

                if (!string.IsNullOrEmpty(m_data.searchButNoEatEntityList)) {
                    // Use the separator string to split the string value
                    string[] splitResult = m_data.searchButNoEatEntityList.Split(new string[] { "," }, StringSplitOptions.None);
                    m_searchButNoEatList = new List<string>(splitResult);
                }

                // if prefered entieies we should tell the mouth
                m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();

                // This will allow to eat them ignoring tier limit
                for (int i = 0; i < m_preferedEntities.Count; i++) {
                    m_eatBehaviour.AddToEatExceptionList(m_preferedEntities[i]);
                }

                // This will make eater to not eat it. If the same sku is in both lists it will not eat it!
                for (int i = 0; i < m_searchButNoEatList.Count; i++) {
                    m_eatBehaviour.AddToIgnoreList(m_searchButNoEatList[i]);
                }

            }

            protected override void OnUpdate() {
                // if prefered entieies check first
                if (m_preferedEntities.Count > 0 || m_searchButNoEatList.Count > 0) {
                    m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(m_machine.position, m_data.sightRadius, m_checkEntities);
                    for (int e = 0; e < m_numCheckEntities; e++) {
                        Entity entity = m_checkEntities[e];
                        bool inSearchButNotEat = m_searchButNoEatList.Contains(entity.sku);
                        if (inSearchButNotEat || m_preferedEntities.Contains(entity.sku)) {
                            IMachine machine = entity.machine;
                            if (machine != null && !machine.isPetTarget) {
                                if (inSearchButNotEat || machine.CanBeBitten()) {
                                    // Check if physics reachable
                                    RaycastHit hit;
                                    Vector3 dir = entity.circleArea.center - m_machine.position;
                                    bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
                                    if (!hasHit) {
                                        // Check if closed? Not for the moment
                                        Transition(onEnemyInRange);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!m_data.ignoreNotListedUnits) {
                    m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(m_machine.position, m_data.sightRadius, m_checkEntities);
                    for (int e = 0; e < m_numCheckEntities; e++) {
                        Entity entity = m_checkEntities[e];
                        IMachine machine = entity.machine;
                        EatBehaviour.SpecialEatAction specialAction = m_eatBehaviour.GetSpecialEatAction(entity.sku);

                        if (entity.IsEdible()
                        && specialAction != EatBehaviour.SpecialEatAction.CannotEat
                        && entity.IsEdible(m_eaterTier)
                        && machine != null
                        && machine.CanBeBitten()
                        && !machine.isPetTarget) {
                            // Check if physics reachable
                            RaycastHit hit;
                            Vector3 dir = entity.circleArea.center - m_machine.position;
                            bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
                            if (!hasHit) {
                                Transition(onEnemyInRange);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}