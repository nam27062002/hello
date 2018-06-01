// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/Dragon/Eggs"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_ColorRamp("Color Ramp (RGB)", 2D) = "white" {}
		[Toggle(EMISSIVE)]_EnableEmissive("Enable emissive", Float) = 0.0
		[Toggle(COLORRAMP)]_EnableColorRamp("Enable color ramp", Float) = 0.0
		_GlowTex("Emissive (RGBA)", 2D) = "white" {}
		_EmissiveColor("Emissive color (Color)", Color) = (0, 0, 0, 0)


		[Toggle(REFLECTION)]_EnableReflection("Enable reflection", Float) = 0.0
		_ReflectionMap("Reflection Map", Cube) = "white" {}
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0

		_LightColor("Light Color", Color) = (1, 1, 1, 1)
		_NormalStrength("Normal Strength", Float) = 3.0
		_SpecularPower( "Specular Power", Float ) = 30.0
		[Rotation] _SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
		_FresnelFactor("Fresnel factor", Range(0.0, 5.0)) = 0.27
		_FresnelInitialColor("Fresnel initial (RGB)", Color) = (0, 0, 0, 0)
		_FresnelFinalColor("Fresnel final (RGB)", Color) = (0, 0, 0, 0)
		_RimFactor("Rim factor", Range(0.0, 8.0)) = 0.27
		_RimColor("Rim Color (RGB)", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry" "LightMode" = "ForwardBase" }
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
			#pragma shader_feature REFLECTION
			#pragma shader_feature EMISSIVE
			#pragma shader_feature NORMALMAP
			#pragma shader_feature COLORRAMP

			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#if LOW_DETAIL_ON
			#endif

			#if MEDIUM_DETAIL_ON
			#define NORMALMAP
			#endif

			#if HI_DETAIL_ON
			#define NORMALMAP
			#endif

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			#include "../HungryDragon.cginc"

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
				#ifdef NORMALMAP
				float3 tangentWorld : TANGENT;
		        float3 binormalWorld : TEXCOORD5;
				#endif
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform sampler2D _NormalTex;
			uniform float4 _NormalTex_ST;

			#ifdef COLORRAMP
			uniform sampler2D _ColorRamp;			 
			#endif

			uniform sampler2D _GlowTex;
			#ifdef EMISSIVE
			uniform float4 _EmissiveColor;
			#endif

			#ifdef REFLECTION
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
			uniform float _RimFactor;
			uniform float4 _RimColor;


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _NormalTex);
				fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);

				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
				o.viewDir = viewDirection;
				float3 lightDirection = normalize(_SpecularDir.xyz);
				o.halfDir = normalize(lightDirection + viewDirection);

				// To calculate tangent world
																								  // To calculate tangent world
				#ifdef NORMALMAP
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

				float3 maskTex = tex2D(_GlowTex, i.uv);

				#ifdef COLORRAMP
				col = lerp(col, tex2D(_ColorRamp, float2(col.x, 0.0)), maskTex.g);
				#endif


				// Aux vars
				#ifdef NORMALMAP
           		float3 encodedNormal = tex2D(_NormalTex, i.uv2);
				float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
				float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
   				float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
				#else
				float3 normalDirection = i.normalWorld;
				#endif

   				float3 lightDirection = normalize(_SpecularDir.xyz);

   				// Compute diffuse and specular
				fixed4 diffuse = (0.5 + max(0, dot(normalDirection, lightDirection))) * _LightColor;		// Custom light color
				fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower) * specMask;

	     		// [AOC] We use light color alpha as specular intensity
	     		specular *= _LightColor.a;

				float nf = max(dot(i.viewDir, normalDirection), 0.0);
				float fresnel = clamp(pow(nf, _FresnelFactor), 0.0, 1.0);
				float rim = clamp(pow(1.0 - nf, _RimFactor), 0.0, 1.0) * _RimColor.a;	// Use rim color alpha as intensity

				#ifdef REFLECTION
				float4 reflection = texCUBE(_ReflectionMap, normalDirection);
				float amount = _ReflectionAmount * maskTex.g;
				col = (1.0 - amount) * col + amount * reflection;
				#endif

				col = diffuse * col + (specular * _LightColor);	//Diffuse + specular
				col += lerp(_FresnelInitialColor, _FresnelFinalColor, fresnel); //Fresnel
				col += (rim * _RimColor); // Rim light

				#ifdef EMISSIVE
				col = lerp(col, _EmissiveColor, maskTex.r * _EmissiveColor.a);	// Multiplicative, emissive color alpha controls intensity
				#endif

//				col += UNITY_LIGHTMODEL_AMBIENT;
				UNITY_OPAQUE_ALPHA(col.a);	// Opaque
				return col;
			}
			ENDCG
		}
	}
}
