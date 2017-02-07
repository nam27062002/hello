﻿Shader "Hidden/VoidReplacement"
{
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
//				float2 depth : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
//				UNITY_TRANSFER_DEPTH(o.depth);
				return o;
			}

			half4 frag(v2f i) : SV_Target{
				return half4(0.0, 0.5, 1.0, 1.0);
//				UNITY_OUTPUT_DEPTH(i.depth);
			}
			ENDCG
		}
	}
}
