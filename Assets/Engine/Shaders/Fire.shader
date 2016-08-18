Shader "Hungry Dragon/Fire Transparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha 
		// Blend One One
		Cull Off
		ZWrite Off
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
				float2 noiseUV : TEXCOORD1;
				float4 vertex : SV_POSITION;
			}; 

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.noiseUV = TRANSFORM_TEX(  v.uv + float2( 0, -_Time.y ) , _NoiseTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 noise = tex2D( _NoiseTex, i.noiseUV ) * 0.65;
				noise.g = noise.g * i.uv.y;
				fixed4 col = tex2D(_MainTex, i.uv - noise.rg);
				return col;
			}
			ENDCG
		}
	}
Fallback "Mobile/VertexLit"
}
