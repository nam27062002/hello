using System;
using System.Collections.Generic;

namespace HDLiveData {
    public enum State {
        EMPTY = 0,
        PARTIAL,
        WAITING_RESPONSE,
        VALID,
        ERROR
    }

    [Serializable]
    public class Reward {
        //---[Attributes]-------------------------------------------------------
        public Metagame.Reward reward;
        public long target;

        //---[Methods]----------------------------------------------------------
        public Reward() { reward = null; target = 0; }

        /// <summary>
        /// Constructor from json data.
        /// </summary>
        /// <param name="_data">Data to be parsed.</param>
        public virtual void ParseJson(SimpleJSON.JSONNode _data, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {
            reward = Metagame.Reward.CreateFromJson(_data, _economyGroup, _source);
            target = _data["target"].AsLong;
        }

        /// <summary>
        /// Serialize into json.
        /// </summary>
        /// <returns>The json.</returns>
        public virtual SimpleJSON.JSONClass ToJson() {
            SimpleJSON.JSONClass data = reward.ToJson() as SimpleJSON.JSONClass;
            data.Add("target", target);
            return data;
        }
    }

    [Serializable]
    public class DragonBuild {
        public string dragon;
        public uint level;
        public List<string> pets;

        public DragonBuild() {
            dragon = "";
            level = 0;

            pets = new List<string>();
        }

        public void FromJson(SimpleJSON.JSONNode _data) {

        }
    }

    namespace Leaderboard {
        [Serializable]
        public class Record {
            //---[Attributes]-------------------------------------------------------
            public uint rank;

            public string name;
            public ulong score;

            public DragonBuild build;


            //---[Methods]------------------------------------------------------
            public Record() {
                rank = 0;

                name = "";
                score = 0;

                build = new DragonBuild();
            }

            public void FromJson(SimpleJSON.JSONNode _data) {

            }
        }
    }
}
