// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hungry Dragon/Skybox/SkyboxCubeMap"
{
	Properties{
		_NoiseTex("Noise1", 2D) = "white" {}
		_NoiseTex2("Noise2", 2D) = "white" {}
		_CloudOffset("Cloud offset", Range(0.0, 2.0)) = 0.5
		_Color1("Color 1", Color) = (1.0, 1.0, 1.0, 1.0)
		_Color2("Color 1", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader{
		Tags{ "Queue" = "Background" }

		Pass{
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag


			#define SKY_SPEED 0.13

			// User-specified uniforms
			sampler2D _NoiseTex;
			sampler2D _NoiseTex2;
			float	  _CloudOffset;
			float4	  _Color1;
			float4	  _Color2;

			struct vertexInput {
				float4 vertex : POSITION;
			};

			struct vertexOutput {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD1;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4x4 modelMatrix = unity_ObjectToWorld;
				output.uv = normalize(mul(modelMatrix, input.vertex).xyz - _WorldSpaceCameraPos);
				output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
				return output;
			}

			float3x3 rotationMatrix(float3 axis, float angle)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;

				return float3x3(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
					oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
					oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
			}


			fixed4 frag(vertexOutput i) : COLOR
			{
				float persp = (0.0 + abs(i.uv.y) * 10.5);
				float2 uv = i.uv.xy + float2((_Time.y * 0.01), 0.0) * float2(persp, 1.0);
				fixed4 col = tex2D(_NoiseTex, uv * float2(1.0 / persp, 1.0));

				uv = i.uv.xy + float2((_Time.y * 0.05), 0.0) * float2(persp, 1.0);
				col += tex2D(_NoiseTex, (uv + col.x * _CloudOffset) * float2(1.0 / persp, 1.0));

				float dd = clamp(abs(uv.y * uv.x) * 0.5, 0.0, 1.0);


//				col *= dd;

				float4 skyCol = lerp(_Color1, _Color2, clamp(i.uv.y * 5.0, 0.0, 1.0));
//				float3x3 mat = rotationMatrix(float3(0.0, 1.0, 0.0), sin(_Time.x * 2.0 + input.viewDir.y * 2.0) * 3.141516);
//				fixed4 col = tex2D(_NoiseTex, mul(mat, input.viewDir).xy);
//				mat = rotationMatrix(float3(-1.0, 1.0, 0.0), sin(_Time.x * 1.0 + input.viewDir.y * 2.0) * 3.141516);
//				col -= tex2D(_NoiseTex2, mul(mat, input.viewDir).xy);
//				return texCUBE(_Cube, input.viewDir);
				return (col * 0.5 + skyCol) * dd;
			}
			ENDCG
		}
	}
}