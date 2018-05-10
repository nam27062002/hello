// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:34295,y:32851,varname:node_4795,prsc:2|alpha-9603-OUT,refract-782-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:8341,x:33065,y:33109,ptovrint:False,ptlb:RefractionMap,ptin:_RefractionMap,varname:node_8341,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5854deeb8c52a224396890147f99839f,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2dAsset,id:4041,x:33065,y:33292,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:node_4041,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:82eda6dffbc50ca4f882a7ee806b60ce,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2310,x:33340,y:33227,varname:node_2310,prsc:2,tex:82eda6dffbc50ca4f882a7ee806b60ce,ntxv:0,isnm:False|TEX-4041-TEX;n:type:ShaderForge.SFN_Tex2d,id:7044,x:33340,y:33098,varname:node_7044,prsc:2,tex:5854deeb8c52a224396890147f99839f,ntxv:0,isnm:False|TEX-8341-TEX;n:type:ShaderForge.SFN_ComponentMask,id:6431,x:33567,y:33075,varname:node_6431,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-7044-RGB;n:type:ShaderForge.SFN_Multiply,id:782,x:34025,y:33075,varname:node_782,prsc:2|A-1708-OUT,B-1593-OUT,C-9603-OUT;n:type:ShaderForge.SFN_Slider,id:1925,x:33199,y:32786,ptovrint:False,ptlb:Power,ptin:_Power,varname:node_1925,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-5,cur:1,max:5;n:type:ShaderForge.SFN_Multiply,id:9603,x:33567,y:33380,varname:node_9603,prsc:2|A-2310-R,B-6555-OUT,C-7745-A;n:type:ShaderForge.SFN_Slider,id:6555,x:33183,y:33376,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_6555,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_RemapRange,id:1593,x:33788,y:33075,varname:node_1593,prsc:2,frmn:0,frmx:1,tomn:-1,tomx:1|IN-6431-OUT;n:type:ShaderForge.SFN_TexCoord,id:6032,x:33567,y:32932,varname:node_6032,prsc:2,uv:0,uaff:True;n:type:ShaderForge.SFN_SwitchProperty,id:1708,x:34025,y:32930,ptovrint:False,ptlb:UseCustomDataWithRefraction,ptin:_UseCustomDataWithRefraction,varname:node_1708,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7190-OUT,B-8864-OUT;n:type:ShaderForge.SFN_Multiply,id:8864,x:33788,y:32942,varname:node_8864,prsc:2|A-7190-OUT,B-6032-Z;n:type:ShaderForge.SFN_VertexColor,id:7745,x:33340,y:33446,varname:node_7745,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3225,x:33567,y:33251,varname:node_3225,prsc:2|A-2310-R,B-6555-OUT;n:type:ShaderForge.SFN_OneMinus,id:7190,x:33567,y:32786,varname:node_7190,prsc:2|IN-1925-OUT;proporder:8341-1925-4041-6555-1708;pass:END;sub:END;*/

Shader "SrtoRubfish/DistorsionWave" {
    Properties {
        _RefractionMap ("RefractionMap", 2D) = "white" {}
        _Power ("Power", Range(-5, 5)) = 1
        _Noise ("Noise", 2D) = "white" {}
        _Opacity ("Opacity", Range(0, 1)) = 0
        [MaterialToggle] _UseCustomDataWithRefraction ("UseCustomDataWithRefraction", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _RefractionMap; uniform float4 _RefractionMap_ST;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _Power;
            uniform float _Opacity;
            uniform fixed _UseCustomDataWithRefraction;
            struct VertexInput {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float node_7190 = (1.0 - _Power);
                float4 node_7044 = tex2D(_RefractionMap,TRANSFORM_TEX(i.uv0, _RefractionMap));
                float4 node_2310 = tex2D(_Noise,TRANSFORM_TEX(i.uv0, _Noise));
                float node_9603 = (node_2310.r*_Opacity*i.vertexColor.a);
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (lerp( node_7190, (node_7190*i.uv0.b), _UseCustomDataWithRefraction )*(node_7044.rgb.rg*2.0+-1.0)*node_9603);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
////// Lighting:
                float3 finalColor = 0;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,node_9603),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
