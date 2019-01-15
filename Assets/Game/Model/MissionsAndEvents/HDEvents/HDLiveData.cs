﻿using System;
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
        public virtual void LoadData(SimpleJSON.JSONNode _data, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {
            reward = Metagame.Reward.CreateFromJson(_data, _economyGroup, _source);
            target = _data["target"].AsLong;
        }

        /// <summary>
        /// Serialize into json.
        /// </summary>
        /// <returns>The json.</returns>
        public virtual SimpleJSON.JSONClass SaveData() {
            SimpleJSON.JSONClass data = reward.ToJson() as SimpleJSON.JSONClass;
            data.Add("target", target);
            return data;
        }
    }

    [Serializable]
    public class DragonBuild {
        public string dragon;
        public string skin;
        public uint level;
        public uint health;
        public uint speed;
        public uint energy;
        public List<string> pets;


        public DragonBuild() {
            pets = new List<string>();
            Clean();
        }

        public void Clean() {
            dragon = "";
            skin = "";
            level = 0;
            health = 0;
            speed = 0;
            energy = 0;
            pets.Clear();
        }

        public void LoadData(SimpleJSON.JSONNode _data) {
            Clean();

            if (_data.ContainsKey("dragon")) dragon = _data["dragon"];
            if (_data.ContainsKey("skin")) skin = _data["skin"];
            if (_data.ContainsKey("level")) level = (uint)_data["level"].AsInt;

            if (_data.ContainsKey("stats")) {
                SimpleJSON.JSONNode stats = _data["stats"];
                health = (uint)stats["health"].AsInt;
                speed  = (uint)stats["speed"].AsInt;
                energy = (uint)stats["energy"].AsInt;
            }

            if (_data.ContainsKey("pets")) {
                SimpleJSON.JSONArray petsData = _data["pets"].AsArray;
                for (int i = 0; i < petsData.Count; i++) {
                    pets.Add(petsData[i]);
                }
            }
        }

        public SimpleJSON.JSONClass SaveData() {
            SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
            {
                if (!string.IsNullOrEmpty(dragon)) data.Add("dragon", dragon);
                if (!string.IsNullOrEmpty(skin)) data.Add("skin", skin);
                if (level > 0)
                    data.Add("level", level.ToString(GameServerManager.JSON_FORMAT));

                if (health > 0 || speed > 0 || energy > 0) {
                    SimpleJSON.JSONClass stats = new SimpleJSON.JSONClass();
                    {
                        stats.Add("health", health);
                        stats.Add("speed",  speed);
                        stats.Add("energy", energy);
                    }
                    data.Add("stats", stats);
                }

                int petsCount = pets.Count;
                if (petsCount > 0) {
                    SimpleJSON.JSONArray petsData = new SimpleJSON.JSONArray();
                    for (int i = 0; i < petsCount; i++) {
                        petsData.Add(pets[i]);
                    }
                    data.Add("pets", petsData);
                }
            }
            return data;
        }
    }

    namespace Leaderboard {
        [Serializable]
        public class Record {
            //---[Attributes]---------------------------------------------------

            public int rank;

            public string name;
            public long score;

            public DragonBuild build;



            //---[Methods]------------------------------------------------------

            public Record() {
                rank = 0;

                name = "";
                score = 0;

                build = new DragonBuild();
            }

            public void LoadData(SimpleJSON.JSONNode _data) {
                build.Clean();

                name = _data["name"];
                score = _data["score"].AsLong;
                if (_data.ContainsKey("build"))
                    build.LoadData(_data["build"]);
            }

            public SimpleJSON.JSONClass SaveData() {
                SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
                data.Add("name", name);
                data.Add("score", score.ToString(GameServerManager.JSON_FORMAT));
                data.Add("build", build.SaveData());
                return data;
            }
        }
    }
}
