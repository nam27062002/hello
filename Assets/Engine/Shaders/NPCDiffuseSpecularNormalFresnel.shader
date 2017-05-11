// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/Diffuse + NormalMap + Specular + Fresnel + Rim (Glow)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_GlowTex("Emissive (RGBA)", 2D) = "white" {}
		_ReflectionMap("Reflection Map", Cube) = "white" {}
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0
		_LightColor("Light Color", Color) = (1, 1, 1, 1)
		_NormalStrength("Normal Strength", float) = 3
		_SpecularPower( "Specular Power", float ) = 1
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
		_FresnelFactor("Fresnel factor", Range(0.0, 5.0)) = 0.27
		_FresnelInitialColor("Fresnel initial (RGB)", Color) = (0, 0, 0, 0)
		_FresnelFinalColor("Fresnel final (RGB)", Color) = (0, 0, 0, 0)
		_RimFactor("Rim factor", Range(0.0, 8.0)) = 0.27
		_RimColor("Rim Color (RGB)", Color) = (1.0, 1.0, 1.0, 1.0)
		_EmissiveColor("Emissive color (RGB)", Color) = (0, 0, 0, 0)
		_GlowColor("Glow (RGB, Alpha is intensity)", Color) = (1, 1, 1, 1)

	}


	SubShader
	{
		Tags{ "RenderType" = "Glow"  "Queue" = "Geometry" "LightMode" = "ForwardBase" }
		Pass
		{
			Cull Back
			ColorMask RGBA
/*
			Stencil
			{
				Ref 5
				Comp always
				Pass Replace
				ZFail keep
			}
*/
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest

			#if LOW_DETAIL_ON
			#endif

			#if MEDIUM_DETAIL_ON
			#define RIM
			#define BUMP
			#endif

			#if HI_DETAIL_ON
			#define RIM
			#define BUMP
			#define SPEC
			#define REFL
			#endif

//			#define BUMP
//			#define REFL

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
				float2 uv2 : TEXCOORD3;
				float4 vertex : SV_POSITION;

				float3 viewDir : TEXTCOORD1;
				float3 halfDir : TEXTCOORD2;
				float3 normalWorld : TEXCOORD4;
				#ifdef BUMP
				float3 tangentWorld : TANGENT;
		        float3 binormalWorld : TEXCOORD5;
				#endif
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform sampler2D _NormalTex;
			uniform float4 _NormalTex_ST;
			uniform sampler2D _GlowTex;
			#ifdef REFL
			uniform samplerCUBE _ReflectionMap;
			uniform float _ReflectionAmount;
			#endif
			uniform float _SpecularPower;
			uniform fixed4 _SpecularDir;
			uniform float4 _LightColor;
			uniform float _NormalStrength;
			uniform float _FresnelFactor;
			uniform float4 _FresnelInitialColor;
			uniform float4 _FresnelFinalColor;
			uniform float4 _EmissiveColor;
			uniform float _RimFactor;
			uniform float4 _RimColor;
			uniform float4 _GlowColor;

//			#if GLOWEFFECT_MULTIPLY_COLOR
//			uniform float4 _GlowColorMult;
//			#endif

//			uniform half4 _GlowColor;
//			uniform half4 _GlowColorMult;


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _NormalTex);
				fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);

//				float3 normal = UnityObjectToWorldNormal(v.normal);
//				o.vLight = ShadeSH9(float4(normal, 1.0));

				// Half View - See: Blinn-Phong
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
				// float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				// o.halfDir = normalize(lightDirection + viewDirection);
				o.viewDir = viewDirection;
				float3 lightDirection = normalize(_SpecularDir.xyz);
				o.halfDir = normalize(lightDirection + viewDirection);

				// To calculate tangent world
																								  // To calculate tangent world
				#ifdef BUMP
				o.tangentWorld = UnityObjectToWorldNormal(v.tangent);
				o.normalWorld = UnityObjectToWorldNormal(v.normal);
				o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
				#else
				o.normalWorld = UnityObjectToWorldNormal(v.normal);
				#endif


				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				float specMask = col.w;

				// Aux vars
				#ifdef BUMP
           		float3 encodedNormal = tex2D(_NormalTex, i.uv2);
				float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
				float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
   				float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
				#else
				float3 normalDirection = i.normalWorld;
				#endif

   				float3 lightDirection = normalize(_SpecularDir.xyz);

   				// Compute diffuse and specular
				fixed4 diffuse = max(0, dot(normalDirection, lightDirection)) * _LightColor;		// Custom light color
				fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower);
				//fixed specular = pow(max(0, dot(normalDirection, lightDirection)), _SpecularPower);

	     		// [AOC] We use light color alpha as specular intensity
	     		specular *= _LightColor.a;

				// fixed fresnel = pow(max(dot(normalDirection, i.viewDir), 0), _FresnelFactor);
				float nf = max(dot(i.viewDir, normalDirection), 0.0);
				float fresnel = clamp(pow(nf, _FresnelFactor), 0.0, 1.0);
				float rim = clamp(pow(1.0 - nf, _RimFactor), 0.0, 1.0) * _RimColor.a;	// Use rim color alpha as intensity

				// col = diffuse * col + (specular * _LightColor0) + (fresnel * _FresnelColor);
				// col = diffuse * col + (specular * _LightColor0) + lerp(_FresnelInitialColor, _FresnelFinalColor, fresnel);	// World light color
				#ifdef REFL
				float4 reflection = texCUBE(_ReflectionMap, normalDirection);
				col = (1.0 - _ReflectionAmount) * col + _ReflectionAmount * reflection;
				#endif
				col = diffuse * col + (specular * _LightColor) + lerp(_FresnelInitialColor, _FresnelFinalColor, fresnel) + (rim * _RimColor);	// Custom light color

				float3 emissive = tex2D(_GlowTex, i.uv2);
				col = lerp(col, _EmissiveColor, (emissive.r + emissive.g + emissive.b) * _EmissiveColor.a);	// Multiplicative, emissive color alpha controls intensity
				// col = lerp(col, _EmissiveColor, emissive.r + emissive.g + emissive.b);			// Multiplicative, no intensity control
				// col += _EmissiveColor * (emissive.r + emissive.g + emissive.b);				// Additive, no intesity control

				//fixed4 one = fixed4(1, 1, 1, 1);
				// col = one- (one-col) * (1-(i.color-fixed4(0.5,0.5,0.5,0.5)));	// Soft Light
				//col = one - 2.0 * (one - reflection) * (one - col);	// Overlay

				UNITY_OPAQUE_ALPHA(col.a);	// Opaque
				return col;
			}
			ENDCG
		}
	}

	CustomEditor "GlowMaterialInspector"

}
