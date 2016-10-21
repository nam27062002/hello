// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Hungry Dragon/BurnToAshes (Transparent)" {
Properties {
	_BurnMask ("Burn Mask", 2D) = "white" {}
//	_AlphaMask ("Alpha Mask", 2D) = "white" {}
	_ColorRamp("Color Ramp", 2D) = "white" {}
	_AshLevel("Ash Level", Range(0, 1)) = 0
	_AshWidth("Ash Width", Range(0, 0.5)) = 0.01
	_BurnMaskScale("Burn Mask Scale", Range(1.0, 8.0)) = 2.0

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
			 
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _BurnMask;
//			sampler2D _AlphaMask;
			sampler2D _ColorRamp;
			float4 _BurnMask_ST;
			float _AshLevel;
			float _AshWidth;
			float _BurnMaskScale;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _BurnMask);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed fragAlpha = tex2D(_BurnMask, i.texcoord * _BurnMaskScale).r - _AshLevel;
				clip(fragAlpha);

				fixed ashIdx = clamp(fragAlpha / _AshWidth, 0.0, 1.0);
//				fixed alpha = tex2D( _AlphaMask, i.texcoord ).a;
				fixed3 ashColor = tex2D(_ColorRamp, fixed2(ashIdx, 0.0));
				fixed4 col = fixed4(ashColor, 1.0);
				UNITY_APPLY_FOG(i.fogCoord, col);
//				col.a = alpha;
				return col;
			}
		ENDCG
		}
	}

}
