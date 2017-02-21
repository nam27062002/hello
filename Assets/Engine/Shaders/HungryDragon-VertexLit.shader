// Simplified VertexLit shader. Differences from regular VertexLit one:
// - no per-material color
// - no specular
// - no emission

Shader "Hungry Dragon/VertexLit" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 80
/*	
	// Non-lightmapped
	Pass {
		Tags { "LightMode" = "Vertex" }
		
		Material {
			Diffuse (1,1,1,1)
			Ambient (1,1,1,1)
		} 
		Lighting On
		SetTexture [_MainTex] {
			constantColor (1,1,1,1)
			Combine texture * primary DOUBLE, constant // UNITY_OPAQUE_ALPHA_FFP
		} 
	}
	
	// Lightmapped, encoded as dLDR
	Pass {
		Tags { "LightMode" = "VertexLM" }
		
		BindChannels {
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture
		}
		SetTexture [_MainTex] {
			constantColor (1,1,1,1)
			combine texture * previous DOUBLE, constant // UNITY_OPAQUE_ALPHA_FFP
		}
	}
	// Lightmapped, encoded as RGBM
	Pass {
		Tags { "LightMode" = "VertexLMRGBM" }
		
		BindChannels {
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture * texture alpha DOUBLE
		}
		SetTexture [_MainTex] {
			constantColor (1,1,1,1)
			combine texture * previous QUAD, constant // UNITY_OPAQUE_ALPHA_FFP
		}
	}
*/

	// Pass to render object as a shadow caster
	Pass 
	{
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		
		ZWrite On ZTest LEqual Cull Off

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 2.0
		#pragma multi_compile_shadowcaster
		#include "UnityCG.cginc"

		struct v2fff { 
//			V2F_SHADOW_CASTER;
			float4 pos : SV_POSITION;
		};

		v2fff vert( appdata_base v )
		{
			v2fff o;
//			TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

			return o;
		}


		fixed4 frag( v2fff i ) : SV_Target
		{
//			SHADOW_CASTER_FRAGMENT(i)
			return i.pos.z;
//			return float4(i.pos.z);
		}
		ENDCG
	}

}
}
