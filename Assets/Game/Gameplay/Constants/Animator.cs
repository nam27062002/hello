using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameConstants
{
	public class Animator{

		// Eat
		public static readonly int EAT_HOLD = UnityEngine.Animator.StringToHash("eatHold");
		public static readonly int EAT = UnityEngine.Animator.StringToHash("eat");
		public static readonly int EAT_CRAZY = UnityEngine.Animator.StringToHash("eat crazy");
		public static readonly int EAT_SPEED = UnityEngine.Animator.StringToHash("eatingSpeed");

		//
		public static readonly int DRUNK = UnityEngine.Animator.StringToHash("drunk");
		public static readonly int BREATH = UnityEngine.Animator.StringToHash("breath");
        public static readonly int MEGA = UnityEngine.Animator.StringToHash("mega");
		public static readonly int STARVING = UnityEngine.Animator.StringToHash("starving");
		public static readonly int FLY_DOWN = UnityEngine.Animator.StringToHash("fly down");
		public static readonly int SWIM = UnityEngine.Animator.StringToHash("swim");
		public static readonly int BOOST = UnityEngine.Animator.StringToHash("boost");
		public static readonly int SPIN = UnityEngine.Animator.StringToHash("spin");
		public static readonly int MOVE = UnityEngine.Animator.StringToHash("move");
		public static readonly int DEAD = UnityEngine.Animator.StringToHash("dead");
		public static readonly int BASE_IDLE = UnityEngine.Animator.StringToHash("BaseLayer.Idle");
		public static readonly int IDLE = UnityEngine.Animator.StringToHash("idle");
		public static readonly int AGAINST_CURRENT = UnityEngine.Animator.StringToHash("against_current");
		public static readonly int GLIDE = UnityEngine.Animator.StringToHash("glide");
		public static readonly int DIR = UnityEngine.Animator.StringToHash("direction");
		public static readonly int DIR_X = UnityEngine.Animator.StringToHash("direction X");
		public static readonly int DIR_Y = UnityEngine.Animator.StringToHash("direction Y");
		public static readonly int BACK_DIR_X = UnityEngine.Animator.StringToHash("back direction X");
		public static readonly int BACK_DIR_Y = UnityEngine.Animator.StringToHash("back direction Y");
		public static readonly int NO_AIR = UnityEngine.Animator.StringToHash("no air");
		public static readonly int DAMAGE = UnityEngine.Animator.StringToHash("damage");
		public static readonly int IMPACT = UnityEngine.Animator.StringToHash("impact");
		public static readonly int HOLDED = UnityEngine.Animator.StringToHash("holded");
		public static readonly int BEND = UnityEngine.Animator.StringToHash("Bend");
		public static readonly int SONIC_FORM = UnityEngine.Animator.StringToHash("SonicForm");

		public static readonly int ROTATE_LEFT = UnityEngine.Animator.StringToHash("rotate left");
		public static readonly int ROTATE_RIGHT = UnityEngine.Animator.StringToHash("rotate right");
		public static readonly int AIM = UnityEngine.Animator.StringToHash("aim");
		public static readonly int HEIGHT = UnityEngine.Animator.StringToHash("height");
		public static readonly int SPEED = UnityEngine.Animator.StringToHash("speed");
		public static readonly int SCARED = UnityEngine.Animator.StringToHash("scared");
		public static readonly int UPSIDE_DOWN = UnityEngine.Animator.StringToHash("upside down");
		public static readonly int HIT = UnityEngine.Animator.StringToHash("hit");
		public static readonly int FALLING = UnityEngine.Animator.StringToHash("falling");
		public static readonly int JUMP = UnityEngine.Animator.StringToHash("jump");
		public static readonly int ATTACK = UnityEngine.Animator.StringToHash("attack");
		public static readonly int MELEE = UnityEngine.Animator.StringToHash("melee");
		public static readonly int RANGED = UnityEngine.Animator.StringToHash("ranged");
		public static readonly int BURN = UnityEngine.Animator.StringToHash("burn");
		public static readonly int EXPLODE = UnityEngine.Animator.StringToHash("explode");
		public static readonly int TOSS = UnityEngine.Animator.StringToHash("toss");

		public static readonly int ALT_ANIMATION = UnityEngine.Animator.StringToHash("AltAnimation");
		public static readonly int IN = UnityEngine.Animator.StringToHash("in");
		public static readonly int OUT = UnityEngine.Animator.StringToHash("out");
		public static readonly int OUT_AUTO = UnityEngine.Animator.StringToHash("out_auto");
		public static readonly int CHANGE = UnityEngine.Animator.StringToHash("change");

		public static readonly int DURATION_INVERTED = UnityEngine.Animator.StringToHash("durationInverted");
		public static readonly int LOOP = UnityEngine.Animator.StringToHash("loop");
		public static readonly int LOOP_DELAY = UnityEngine.Animator.StringToHash("loopDelay");
		public static readonly int DELAY_INVERTED = UnityEngine.Animator.StringToHash("delayInverted");
		public static readonly int ACTIVE = UnityEngine.Animator.StringToHash("active");
		public static readonly int RELOAD = UnityEngine.Animator.StringToHash("reload");

		public static readonly int SELECTED = UnityEngine.Animator.StringToHash("Selected");
		public static readonly int NORMAL = UnityEngine.Animator.StringToHash("Normal");
		public static readonly int HIGHLIGHTED = UnityEngine.Animator.StringToHash("Highlighted");

		public static readonly int OPEN = UnityEngine.Animator.StringToHash("open");
		public static readonly int OPEN_POSE = UnityEngine.Animator.StringToHash("open_pose");
		public static readonly int START = UnityEngine.Animator.StringToHash("start");
		public static readonly int CLOSE = UnityEngine.Animator.StringToHash("close");
		public static readonly int RESULTS_IN = UnityEngine.Animator.StringToHash("results_in");
		public static readonly int BOUNCE = UnityEngine.Animator.StringToHash("bounce");

		public static readonly int EGG_STATE = UnityEngine.Animator.StringToHash("egg_state");
		public static readonly int COLLECT_STEP = UnityEngine.Animator.StringToHash("collect_step");
		public static readonly int UNLOCK = UnityEngine.Animator.StringToHash("unlock");

		public static readonly int FOLD = UnityEngine.Animator.StringToHash("fold");
		public static readonly int UNFOLD = UnityEngine.Animator.StringToHash("unfold");
		public static readonly int RARITY = UnityEngine.Animator.StringToHash("rarity");
		public static readonly int INTENSITY = UnityEngine.Animator.StringToHash("intensity");

		public static readonly int SHOW = UnityEngine.Animator.StringToHash("show");
		public static readonly int INSTANT_SHOW = UnityEngine.Animator.StringToHash("instantShow");
		public static readonly int HIDE = UnityEngine.Animator.StringToHash("hide");
		public static readonly int INSTANT_HIDE = UnityEngine.Animator.StringToHash("instantHide");

		public static readonly int COUNTDOWN = UnityEngine.Animator.StringToHash("countdown");
		public static readonly int BEEP = UnityEngine.Animator.StringToHash("beep");

        public static readonly int ENABLED = UnityEngine.Animator.StringToHash("enabled");
        public static readonly int STATE = UnityEngine.Animator.StringToHash("state");
        
        
        // HELICOPTER DRAGON
        public static readonly int MISSILE = UnityEngine.Animator.StringToHash("missile");
        public static readonly int BOMB = UnityEngine.Animator.StringToHash("bomb");
        public static readonly int NECK_DISTANCE = UnityEngine.Animator.StringToHash("neckDistance");
	}
}
