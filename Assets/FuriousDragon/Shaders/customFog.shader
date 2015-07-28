Shader "Custom/MyCustomFog" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		m_FogColor ("Fog Color", Color) = (0,0,0,0)
		m_FogStart ("Fog Start", float) = 0
		m_FogEnd ("Fog End", float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert finalcolor:mycolor vertex:myvert

		sampler2D _MainTex;
		uniform half4 m_FogColor;
		uniform half m_FogStart;
		uniform half m_FogEnd;

		struct Input {
			float2 uv_MainTex;
			half fog;
		};

		void myvert (inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input,data);
			float pos = length(mul (UNITY_MATRIX_MV, v.vertex).xyz);

			float diff = m_FogEnd - m_FogStart;
			float invDiff = 1.0f / diff;
			data.fog = clamp ((m_FogEnd - pos) * invDiff, 0.0, 1.0);
		}
		void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
		{
			fixed3 fogColor = m_FogColor.rgb;
			#ifdef UNITY_PASS_FORWARDADD
			fogColor = 0;
			#endif
			color.rgb = lerp (fogColor, color.rgb, IN.fog);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}