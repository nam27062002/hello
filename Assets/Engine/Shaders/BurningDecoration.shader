// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Hungry Dragon/Burning Decoration" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

	_BurnLevel( "Burn Level", Range (0, 3)) = 0
	_BurnMask("Burn Mask", 2D) = "white" {}

	// FOG
	// _FogColor ("Fog Color", Color) = (0,0,0,0)
	// _FogStart( "Fog Start", float ) = 0
	// _FogEnd( "Fog End", float ) = 100

}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGBA
	
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
				// HG_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _BurnMask;
			uniform float _BurnLevel;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				// HG_TRANSFER_FOG(o, mul(_Object2World, v.vertex), _FogStart, _FogEnd);	// Fog
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				clip(tex2D(_BurnMask, i.texcoord).b - (_BurnLevel-2));	// Remove ashes pixels

				fixed4 col = tex2D(_MainTex, i.texcoord);

				fixed burnedFactor = tex2D(_BurnMask, i.texcoord).r - _BurnLevel;
				if (burnedFactor <= -0.7)
				{
					fixed delta = 1 - (burnedFactor+0.7) / -0.3f;
					col = col * fixed4( delta, 0, 0, 1 );	
				}
				else if ( burnedFactor < 0 )
				{
					fixed delta = 1 - burnedFactor / -0.7f;
					col = col * fixed4( 1, delta, delta, 1 );
				}

				// HG_APPLY_FOG(i, col, _FogColor);	// Fog

				return col;
			}
		ENDCG
	}
}

}
