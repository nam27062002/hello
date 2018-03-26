Shader "Hungry Dragon/Scenary/Diffuse + Lightmap (Chests)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_StencilMask("Stencil Mask", int) = 10
	}

	SubShader {
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100
		
		Pass {		

			Stencil
			{
				Ref [_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

//				#define FOG
				#define MAINCOLOR_TEXTURE
				#define OPAQUEALPHA

				#include "scenary.cginc"

			ENDCG
		}
	}
}
