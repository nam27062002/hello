Shader "Hungry Dragon/PostEffect/LockEffectReplacement"
{
	SubShader{
		Tags{ "RenderType" = "Opaque" "Lock" = "On" }
		Pass{
			Fog{ Mode Off }
			Color(1,1,1,1) 
		}
	}

	SubShader{
		Tags{ "RenderType" = "TransparentCutout" "Lock" = "On" }
		Pass{
			Fog{ Mode Off }
			Color(1,1,1,1) 
		}
	}
}
