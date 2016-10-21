Shader "Hungry Dragon/Collision"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_DebugColor ("Debug Color", Color) = (0.5, 1, 0, 0.5)	// Green-ish, used by the replacement shader
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "ReplacementShaderID"="Collision"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform fixed4 _Color;

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
				return _Color;
			}
			ENDCG
		}
	}
}
