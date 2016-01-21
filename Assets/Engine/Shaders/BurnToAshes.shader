Shader "Unlit/BurnToAshes"
{
	Properties
	{
    	_DissolveMask ("Dissolve Mask (RGB)", 2D) = "white" {}
		_AshLevel( "Ash Level", Range (0, 1)) = 0
	}
	SubShader
	{
    	Tags { "RenderType" = "Opaque" }
		LOD 100

		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _DissolveMask_ST;
    		sampler2D _DissolveMask;
			uniform float _AshLevel;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _DissolveMask);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				clip(tex2D(_DissolveMask, i.uv).rgb - _AshLevel);
				fixed4 col = float4(0,0,0,1);
				return col;
			}
			ENDCG
		}
	}
}
