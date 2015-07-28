Shader "Hidden/CubeBlurOdd" {
	Properties {
		_MainTex ("Main", CUBE) = "" {}
		_Texel ("Texel", Float) = 0.0078125
		_Level ("Level", Float) = 0.
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Pass {
			ZTest Always
			Blend Off
			AlphaTest off
			Cull Off
			ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			struct v2f {
				half4 pos : SV_POSITION;
				half4 uvw : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uvw = v.texcoord;
				return o;
			}

#ifdef SHADER_API_D3D11
			TextureCube _MainTex;
			SamplerState _MainTexSampler;
			#define sampleCUBE(tex, dir, lod) tex.SampleLevel(tex##Sampler, dir, lod)
#elif SHADER_API_SM2
			samplerCUBE _MainTex;
			#define sampleCUBE(tex, dir,  lod) texCUBEbias(tex, half4(dir, lod))
#else
			samplerCUBE _MainTex;
			#define sampleCUBE(tex, dir,  lod) texCUBElod(tex, half4(dir, lod))
#endif
			half _Texel;
			half _Level;

			#define zero	half3(0., 0., 0.)
			#define one		half3(1., 1., 1.)
			#define two		half3(2., 2., 2.)

			half3 fold(half3 st, half3 face)
			{
				half3 c = min(max(st, -one), one);
				half3 f = abs(st - c);
				half m = max(max(f.x, f.y), f.z);
				return c - m*face;
			}

			half3 gauss(half d)
			{
				// compute coefficients for positions 0., 1.*d/.5 and 2.*d/.5
				// this assumes a sigma of .5 for a density of 1.
				half3 v = half3(0., 1.*d, 2.*d);
				return exp(-v*v);
			}

			half4 frag(v2f i) : SV_Target
			{
			#if SHADER_API_SM2
				return texCUBEbias(_MainTex, half4(i.uvw.xyz, _Level));
			#else

				half3 st;

				half3 face = abs(i.uvw.xyz)==one ? i.uvw.xyz : zero;
				half3 u = face.zxy*_Texel;
				half3 v = face.yzx*_Texel;
				half4 s = float4(i.uvw.xyz*(one - abs(face)), 0.);

				// modulate coefficients based on position (texel density on projected sphere)
				half w = 1. / sqrt(1. + dot(s.xyz, s.xyz));
				half3 C = gauss(w*w*w);

				half4 s1, s2, s3;
				half3 c;

				s = 0.;
				w = 0.;

				// first row

				c = C.xyz*C.zzz;

				st = i.uvw.xyz - 2.*u - 2.*v;
				st = fold(st, face);
				s3 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 1.*u - 2.*v;
				st = fold(st, face);
				s2 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 2.*v;
				st = fold(st, face);
				s1 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 1.*u - 2.*v;
				st = fold(st, face);
				s2 += sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 2.*u - 2.*v;
				st = fold(st, face);
				s3 += sampleCUBE(_MainTex, st, _Level);

				w += c.x + dot(c.yz, two.yz);
				s1 = c.x*s1 + c.y*s2;
				s += c.z*s3;
				s += s1;

				// second row

				c = C.xyz*C.yyy;

				st = i.uvw.xyz + 2.*u - 1.*v;
				st = fold(st, face);
				s3 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 1.*u - 1.*v;
				st = fold(st, face);
				s2 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 1.*v;
				st = fold(st, face);
				s1 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 1.*u - 1.*v;
				st = fold(st, face);
				s2 += sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 2.*u - 1.*v;
				st = fold(st, face);
				s3 += sampleCUBE(_MainTex, st, _Level);

				w += c.x + dot(c.yz, two.yz);
				s1 = c.x*s1 + c.y*s2;
				s += c.z*s3;
				s += s1;

				// third row

				c = C.xyz*C.xxx;

				st = i.uvw.xyz - 2.*u;
				st = fold(st, face);
				s3 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 1.*u;
				st = fold(st, face);
				s2 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz;
				s1 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 1.*u;
				s2 += sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 2.*u;
				st = fold(st, face);
				s3 += sampleCUBE(_MainTex, st, _Level);

				w += c.x + dot(c.yz, two.yz);
				s1 = c.x*s1 + c.y*s2;
				s += c.z*s3;
				s += s1;

				// fourth row

				c = C.xyz*C.yyy;

				st = i.uvw.xyz + 2.*u + 1.*v;
				st = fold(st, face);
				s3 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 1.*u + 1.*v;
				st = fold(st, face);
				s2 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 1.*v;
				st = fold(st, face);
				s1 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 1.*u + 1.*v;
				st = fold(st, face);
				s2 += sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 2.*u + 1.*v;
				st = fold(st, face);
				s3 += sampleCUBE(_MainTex, st, _Level);

				w += c.x + dot(c.yz, two.yz);
				s1 = c.x*s1 + c.y*s2;
				s += c.z*s3;
				s += s1;

				// fifth row

				c = C.xyz*C.zzz;

				st = i.uvw.xyz - 2.*u + 2.*v;
				st = fold(st, face);
				s3 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz - 1.*u + 2.*v;
				st = fold(st, face);
				s2 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 2.*v;
				st = fold(st, face);
				s1 = sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 1.*u + 2.*v;
				st = fold(st, face);
				s2 += sampleCUBE(_MainTex, st, _Level);

				st = i.uvw.xyz + 2.*u + 2.*v;
				st = fold(st, face);
				s3 += sampleCUBE(_MainTex, st, _Level);

				w += c.x + dot(c.yz, two.yz);
				s1 = c.x*s1 + c.y*s2;
				s += c.z*s3;
				s += s1;

//				return half4(C.zzz, 1.);
				return s/w;
			#endif
			}
			ENDCG
		}
	}
}
