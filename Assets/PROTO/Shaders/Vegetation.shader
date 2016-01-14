// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Vegetation" {
Properties 
{
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

	_WindDiplacement ("WindDisplacement", float) = 0
	_WavingDiplacement ("WavingDisplacement", float) = 0
	_WavingSpeed ("WavingSpeed", float) = 0

	_Height( "Height", float ) = 1
	_MovementYStart( "Movement Y Start", Range (0, 1)) = 0.3

}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
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

			uniform float _WindDiplacement;
			uniform float _WavingDiplacement;
			uniform float _WavingSpeed;
			uniform float _Height;
			uniform float _MovementYStart;


			v2f vert (appdata_t v)
			{
				v2f o;

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				if ( v.vertex.y > _MovementYStart )
				{ 
					float d = (v.vertex.y - _MovementYStart) / (_Height - _MovementYStart);
					d = sin( d * 3.14 / 2.0 );
					o.vertex.x += d * _WindDiplacement;
					o.vertex.x += sin( _Time.y * _WavingSpeed) * _WavingDiplacement * d;
				}

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
}
	Fallback "Mobile/VertexLit"
}
