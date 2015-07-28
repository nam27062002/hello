Shader "Custom/diffuseAlphaTest" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert finalcolor:mycolor vertex:myvert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			half fog;
		};

		void myvert (inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input,data);
			float pos = length(mul (UNITY_MATRIX_MV, v.vertex).xyz);

		}
		void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
		{
			if (color.a < 0.9)
			  discard;
			
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