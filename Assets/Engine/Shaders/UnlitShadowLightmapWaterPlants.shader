Shader "Hungry Dragon/Scenary/Diffuse + Lightmap + Animated Vertex (Water plants)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpeedWave ("Speed Wave", float) = 1.0
		_Amplitude("Amplitude", float) = 0.08
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="true" }
		LOD 100
		
		Pass {		
			cull off
			ZWrite On


			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma shader_feature __ NIGHT

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

				#define FOG
				#define CUSTOM_VERTEXPOSITION
				#define MAINCOLOR_TEXTURE
				#define OPAQUEALPHA

				#include "scenary.cginc"
			ENDCG
		}
	}
}
