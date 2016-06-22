Shader "Hungry Dragon/Bird"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  "LightMode" = "ForwardBase" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(2)
				float4 vertex : SV_POSITION;
				fixed4 light : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 normalDirection = normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);
            	float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz); 
            	float3 diffuseReflection = max(0.0, dot(normalDirection, lightDirection)) * _LightColor0;
            	o.light.rgb = ShadeSH9(float4(normalDirection, 1.0)) + diffuseReflection;
            	o.light.a = 1;
            	// o.light = fixed4(diffuseReflection , 1) + (UNITY_LIGHTMODEL_AMBIENT * 2);

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 paintedCol = col * _Color;

				col = lerp(paintedCol, col, col.a);
				col.a = 1;
				// apply light
				col *= i.light;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
Fallback "Mobile/VertexLit"
}
