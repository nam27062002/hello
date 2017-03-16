// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Hungry Dragon/Burned" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_SpeedX("Fire Speed X", float) = 0
	_SpeedY("Fire Speed Y", float) = 0
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
			// #include "HungryDragon.cginc"

			struct appdata_t {
				float4 vertex : POSITION; 
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord2 : TEXCOORD2;
				// HG_FOG_COORDS(1)
			};

			sampler2D _MainTex; 
			float4 _MainTex_ST;
			float _SpeedX;
			float _SpeedY;
			 
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				half x = 0.5 + (_Time.y * _SpeedX);
				half y = 0.5 + (_Time.y * _SpeedY);
				o.texcoord2 = o.texcoord + frac( half2( x, y));
				// HG_TRANSFER_FOG(o, mul(_Object2World, v.vertex), _FogStart, _FogEnd);	// Fog
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				fixed4 col2 = tex2D(_MainTex, i.texcoord2);
				// HG_APPLY_FOG(i, col, _FogColor);	// Fog
				col = col + (col2 * col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
		ENDCG
	}
}
}
