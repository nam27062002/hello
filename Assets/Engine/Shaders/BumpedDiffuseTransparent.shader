Shader "Hungry Dragon/Bumped Diffuse Transparent (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Specular( "Specular", float ) = 1
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Back
		ColorMask RGBA

		Pass
		{
			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;

				float3 vLight : TEXCOORD2;

				float3 halfDir : VECTOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _Specular;
			uniform float _BumpStrength;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				fixed3 worldPos = mul(_Object2World, v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.vLight = ShadeSH9(float4(o.normal, 1.0));

				// Half View - See: Blinn-Phong
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				o.halfDir = normalize(lightDirection + viewDirection);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

     			float3 normalDirection = i.normal;

     			fixed4 diffuse = max(0,dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;

     			fixed4 specular = fixed4(0,0,0,0);
     			if (_Specular > 0)
     				specular = pow(max(dot( normalDirection, i.halfDir), 0), _Specular);

     			// col = (diffuse + fixed4(UNITY_LIGHTMODEL_AMBIENT.rgb,1)) * col + specular * _LightColor0;
     			col = (diffuse + fixed4(i.vLight,1)) * col + specular * _LightColor0;
				return col;
			}
			ENDCG
		}
	}
}
