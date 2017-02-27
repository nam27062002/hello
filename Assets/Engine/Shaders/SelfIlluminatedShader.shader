// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - no Lightmap support
// - supports ONLY 1 directional light. Other lights are completely ignored.
// Based on -> Bumped Specular (1 Directional Light)
Shader "Hungry Dragon/Self Illuminated" {
Properties 
{
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
	_AmbientColor("Ambient Color", Color) = (1,1,1,1)
	[LightDirection] _LightDir("Light Dir", Vector) = (1,1,1,1)
	_LightColor("Light Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.03, 10)) = 0.078125
	_SpecIntensity ("Spec Intensity", Range (0, 1)) = 0.5
}

SubShader 
{ 
	Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase" }
	
	LOD 100
	Lighting Off
	ZWrite On
	
	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		// #pragma fragmentoption ARB_precision_hint_fastest exclude_path:prepass nolightmap NoLighting halfasview novertexlights noshadow
		#pragma multi_compile_fwdbase
		#pragma multi_compile_fog
		#include "UnityCG.cginc"

		
		sampler2D _MainTex;
		float4 _MainTex_ST;
			
		half _Shininess;
		half _SpecIntensity;
		fixed4 _Color;
		fixed4 _AmbientColor;
		fixed4 _LightDir;
		fixed4 _LightColor;
		
		struct v2f 
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
			float3 halfDir : VECTOR;
			UNITY_FOG_COORDS(1)
		};
		
		
		v2f vert (appdata_base v)
		{ 
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); 
			o.uv = v.texcoord.xy;

			// Normal
			o.normal = normalize(mul(v.normal, (float3x3)unity_WorldToObject).xyz);

			// Half View - See: Blinn-Phong
			float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
	       		float3 lightDirection = normalize(_LightDir.rgb);
	        	o.halfDir = normalize(lightDirection + viewDirection);
	        
	   		UNITY_TRANSFER_FOG(o, o.vertex);
	    	return o;
			
		}

		fixed4 frag (v2f i) : COLOR
		{
			fixed4 c = tex2D(_MainTex, i.uv) * _Color; 
			
			float lightIntensity = _LightDir.w;
			float3 lightDirection = normalize( _LightDir.rgb );
			
			// Ambient
			float3 ambientLightning = _AmbientColor.rgb * c.rgb;
			
			// Diffuse
			float dotResult = max(dot( i.normal, lightDirection), 0);
			float3 diffuseLightning = c.rgb * _LightColor.rgb * dotResult * lightIntensity;

			// Blinn-Phong Specular
			float spec = pow(max(dot( i.normal, i.halfDir), 0), _Shininess * 128);
			float3 specLightning = _LightColor.rgb * spec * _SpecIntensity;
			fixed4 col = fixed4(ambientLightning + diffuseLightning + specLightning, 1);

			// Apply Fog
			UNITY_APPLY_FOG(i.fogCoord, col);

			return col;
		}
		ENDCG
	}
	
	
	
}
}
