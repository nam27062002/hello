Shader "Hungry Dragon/Bumped Diffuse (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Specular( "Specular", float ) = 1
		_BumpStrength("Bump Strength", float) = 3

		// FOG
		_FogColor ("Fog Color", Color) = (0,0,0,0)
		_FogStart( "Fog Start", float ) = 0
		_FogEnd( "Fog End", float ) = 100
	}
	SubShader
	{
		Pass
		{
			Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
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
				HG_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;

				float3 vLight : TEXCOORD2;

				float3 halfDir : VECTOR;
				float3 tangentWorld : TEXCOORD3;  
		        float3 normalWorld : TEXCOORD4;
		        float3 binormalWorld : TEXCOORD5;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _Specular;
			uniform float _BumpStrength;

			float4 _FogColor;
			float _FogStart;
			float _FogEnd;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				fixed3 worldPos = mul(_Object2World, v.vertex);
				HG_TRANSFER_FOG(o, worldPos, _FogStart, _FogEnd);	// Fog
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.vLight = ShadeSH9(float4(o.normal, 1.0));

				// Half View - See: Blinn-Phong
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				o.halfDir = normalize(lightDirection + viewDirection);

				// To calculate tangent world
	            float4x4 modelMatrix = _Object2World;
     			float4x4 modelMatrixInverse = _World2Object; 
	            o.tangentWorld = normalize( mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
     			o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
     			o.binormalWorld = normalize( cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				// Calc normal from detail texture normal and tangent world
				float heightSampleCenter = col.a;

	            float heightSampleRight = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0) ).a;

	            float heightSampleUp = tex2D (_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).a;
	     
	            float sampleDeltaRight = heightSampleRight - heightSampleCenter;
	            float sampleDeltaUp = heightSampleUp - heightSampleCenter;

	            float3 encodedNormal = cross(float3(1, 0, sampleDeltaRight * _BumpStrength ),float3(0, 1, sampleDeltaUp * _BumpStrength));
	            float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
     			float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));

     			fixed4 diffuse = max(0,dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;

     			fixed4 specular = fixed4(0,0,0,0);
     			if (_Specular > 0)
     				specular = pow(max(dot( normalDirection, i.halfDir), 0), _Specular);

     			// col = (diffuse + fixed4(UNITY_LIGHTMODEL_AMBIENT.rgb,1)) * col + specular * _LightColor0;
     			col = (diffuse + fixed4(i.vLight,1)) * col + specular * _LightColor0;

				// apply fog
				HG_APPLY_FOG(i, col, _FogColor);	// Fog
				UNITY_OPAQUE_ALPHA(col.a);	// Opaque
				return col;
			}
			ENDCG
		}
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off
			Offset 1, 1

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				struct v2f { 
					V2F_SHADOW_CASTER;
				};

				v2f vert (appdata_base v)
				{
					v2f o;
					TRANSFER_SHADOW_CASTER(o)
					return o;
				}

				float4 frag (v2f i) : COLOR
				{
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		} //Pass

	}
}
