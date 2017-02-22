// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Unlit/TextureColorShadow (Cast Shadows)" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
	_ColorAdd ("Color Add", Color) = (0,0,0,0)
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
						
			#include "UnityCG.cginc"
			#include "HungryDragon.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ColorMultiply;
			float4 _ColorAdd;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _ColorMultiply + _ColorAdd;
				UNITY_APPLY_FOG(i.fogCoord, col);
//				UNITY_OPAQUE_ALPHA(col.a);
				HG_DEPTH_ALPHA(i, col)
				return col;
			}
		ENDCG
	}
}
	Fallback "Hungry Dragon/VertexLit"
}
