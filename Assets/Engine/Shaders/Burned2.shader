// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Burned 2" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Speed("Fire Speed", float) = 0
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
			float _SpeedX;
			float _SpeedY;
			 
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed Overlay( fixed i )
			{
				if ( i < 0.5 )
				{
					return i * i;
				}
				return i+i;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);

				fixed4 col2 = col;
				col2.r = Overlay( col.r );
				col2.g = Overlay( col.g );
				col2.b = Overlay( col.b );
				col = col + col2 * ((sin( _Time.y * 2 ) + 1.0) / 2.0);

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
		ENDCG
	}
}
	// Pass to render object as a shadow caster
	Fallback "VertexLit"
}
