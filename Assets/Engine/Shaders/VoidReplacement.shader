Shader "Hidden/VoidReplacement"
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Fog{ Mode Off }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1.0, 1.0, 0.0, 1.0);
			}
			ENDCG
		}
	}
}
