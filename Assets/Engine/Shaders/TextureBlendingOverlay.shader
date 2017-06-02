// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Scenary/Texture Blending + Lightmap + Vertex Color Overlay" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture("Second Texture (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100
		
		Pass {  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest

				#define	HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"
								
				#define FOG
				#define BLEND_TEXTURE
				#define VERTEXCOLOR_OVERLAY
				#define OPAQUEALPHA

//				#define DEBUG								
				#include "scenary.cginc"


			ENDCG
		}
	}
}
