// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:34296,y:32917,varname:node_3138,prsc:2|emission-8104-OUT,alpha-4866-OUT,clip-8309-OUT;n:type:ShaderForge.SFN_Tex2d,id:8397,x:33299,y:32969,ptovrint:False,ptlb:flame mask,ptin:_flamemask,varname:node_8397,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e66f78fe86eeeb64fa7066212a402113,ntxv:0,isnm:False|UVIN-4439-OUT;n:type:ShaderForge.SFN_Tex2d,id:8673,x:31723,y:32749,varname:node_8673,prsc:2,tex:f9e9ed2b10ceb39408d44a558175c09b,ntxv:0,isnm:False|UVIN-641-UVOUT,TEX-7978-TEX;n:type:ShaderForge.SFN_Tex2d,id:3095,x:31718,y:32938,varname:_node_8673_copy,prsc:2,tex:f9e9ed2b10ceb39408d44a558175c09b,ntxv:0,isnm:False|UVIN-9928-UVOUT,TEX-7978-TEX;n:type:ShaderForge.SFN_Tex2d,id:3127,x:31701,y:33118,varname:_node_8673_copy_copy,prsc:2,tex:f9e9ed2b10ceb39408d44a558175c09b,ntxv:0,isnm:False|UVIN-9314-UVOUT,TEX-7978-TEX;n:type:ShaderForge.SFN_Add,id:374,x:31937,y:32843,varname:node_374,prsc:2|A-8673-R,B-3095-G;n:type:ShaderForge.SFN_Add,id:8293,x:32124,y:32979,varname:node_8293,prsc:2|A-374-OUT,B-3127-B;n:type:ShaderForge.SFN_Multiply,id:3179,x:32554,y:33005,varname:node_3179,prsc:2|A-7921-OUT,B-8674-OUT;n:type:ShaderForge.SFN_Slider,id:8674,x:32229,y:32899,ptovrint:False,ptlb:dist Amount,ptin:_distAmount,varname:node_8674,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.211346,max:3;n:type:ShaderForge.SFN_TexCoord,id:5526,x:31980,y:33352,varname:node_5526,prsc:2,uv:0;n:type:ShaderForge.SFN_Append,id:4439,x:33065,y:33017,varname:node_4439,prsc:2|A-5526-U,B-6596-OUT;n:type:ShaderForge.SFN_Tex2d,id:4167,x:32124,y:33675,ptovrint:False,ptlb:alpha,ptin:_alpha,varname:node_4167,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:3427,x:32738,y:33289,varname:node_3427,prsc:2|A-3179-OUT,B-7482-OUT;n:type:ShaderForge.SFN_Add,id:6596,x:32918,y:33389,varname:node_6596,prsc:2|A-3427-OUT,B-7093-OUT;n:type:ShaderForge.SFN_Panner,id:641,x:31348,y:32704,varname:node_641,prsc:2,spu:-1,spv:-0.6|UVIN-5965-OUT,DIST-8040-OUT;n:type:ShaderForge.SFN_Panner,id:9928,x:31359,y:32870,varname:node_9928,prsc:2,spu:0.85,spv:-1|UVIN-106-OUT,DIST-8040-OUT;n:type:ShaderForge.SFN_Panner,id:9314,x:31350,y:33013,varname:node_9314,prsc:2,spu:-0.2,spv:-0.92|UVIN-106-OUT,DIST-8040-OUT;n:type:ShaderForge.SFN_TexCoord,id:8023,x:30219,y:32727,varname:node_8023,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:106,x:30655,y:32829,varname:node_106,prsc:2|A-8023-UVOUT,B-3052-OUT;n:type:ShaderForge.SFN_Slider,id:3052,x:30121,y:33019,ptovrint:False,ptlb:dist Scale,ptin:_distScale,varname:node_3052,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8353261,max:1;n:type:ShaderForge.SFN_Time,id:1615,x:30720,y:33016,varname:node_1615,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8040,x:30921,y:33131,varname:node_8040,prsc:2|A-1615-T,B-301-OUT;n:type:ShaderForge.SFN_Slider,id:9234,x:30385,y:33424,ptovrint:False,ptlb:dist Speed,ptin:_distSpeed,varname:_distScale_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3376412,max:1;n:type:ShaderForge.SFN_Multiply,id:7482,x:32525,y:33570,varname:node_7482,prsc:2|A-5526-V,B-1841-OUT;n:type:ShaderForge.SFN_Multiply,id:3330,x:33620,y:33065,varname:node_3330,prsc:2|A-8397-R,B-756-RGB;n:type:ShaderForge.SFN_Color,id:756,x:33468,y:33321,ptovrint:False,ptlb:OUT colour,ptin:_OUTcolour,varname:node_756,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.3517241,c3:0,c4:1;n:type:ShaderForge.SFN_Add,id:8104,x:33871,y:33076,varname:node_8104,prsc:2|A-3330-OUT,B-8295-OUT;n:type:ShaderForge.SFN_Color,id:836,x:33502,y:33524,ptovrint:False,ptlb:IN colour,ptin:_INcolour,varname:_node_756_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.7796323,c3:0.007352948,c4:1;n:type:ShaderForge.SFN_Multiply,id:8295,x:33742,y:33209,varname:node_8295,prsc:2|A-8397-G,B-836-RGB;n:type:ShaderForge.SFN_Multiply,id:301,x:30745,y:33244,varname:node_301,prsc:2|A-3052-OUT,B-9234-OUT;n:type:ShaderForge.SFN_Power,id:7921,x:32383,y:33066,varname:node_7921,prsc:2|VAL-8293-OUT,EXP-4940-OUT;n:type:ShaderForge.SFN_Slider,id:4940,x:32020,y:33225,ptovrint:False,ptlb:dist Contrast,ptin:_distContrast,varname:node_4940,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2.750238,max:10;n:type:ShaderForge.SFN_OneMinus,id:1841,x:32292,y:33589,varname:node_1841,prsc:2|IN-5526-V;n:type:ShaderForge.SFN_Power,id:7093,x:32699,y:33427,varname:node_7093,prsc:2|VAL-5526-V,EXP-4005-OUT;n:type:ShaderForge.SFN_Slider,id:4005,x:32844,y:33644,ptovrint:False,ptlb:V push,ptin:_Vpush,varname:node_4005,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:6.375284,max:10;n:type:ShaderForge.SFN_Multiply,id:4565,x:33313,y:33179,varname:node_4565,prsc:2|A-515-OUT,B-4167-G;n:type:ShaderForge.SFN_OneMinus,id:515,x:33150,y:33145,varname:node_515,prsc:2|IN-8397-B;n:type:ShaderForge.SFN_Multiply,id:5965,x:31138,y:32631,varname:node_5965,prsc:2|A-106-OUT,B-2693-OUT;n:type:ShaderForge.SFN_Vector1,id:2693,x:30900,y:32602,varname:node_2693,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Tex2dAsset,id:7978,x:31425,y:33245,ptovrint:False,ptlb:dist TEX,ptin:_distTEX,varname:node_7978,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f9e9ed2b10ceb39408d44a558175c09b,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:4866,x:33889,y:32752,ptovrint:False,ptlb:Master Opacity,ptin:_MasterOpacity,varname:node_4866,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Power,id:8309,x:34088,y:33175,varname:node_8309,prsc:2|VAL-4565-OUT,EXP-4769-OUT;n:type:ShaderForge.SFN_Vector1,id:4769,x:33813,y:33512,varname:node_4769,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:5805,x:30623,y:32507,varname:node_5805,prsc:2|A-6941-OUT,B-9772-OUT;n:type:ShaderForge.SFN_Vector1,id:9772,x:30389,y:32641,varname:node_9772,prsc:2,v1:0.0025;n:type:ShaderForge.SFN_FragmentPosition,id:2269,x:30132,y:32449,varname:node_2269,prsc:2;n:type:ShaderForge.SFN_ComponentMask,id:6941,x:30373,y:32448,varname:node_6941,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-2269-XYZ;proporder:4167-8397-8674-3052-9234-756-836-4940-4005-7978-4866;pass:END;sub:END;*/

Shader "Shader Forge/sh_fire_procedural" {
    Properties {
        _alpha ("alpha", 2D) = "white" {}
        _flamemask ("flame mask", 2D) = "white" {}
        _distAmount ("dist Amount", Range(0, 3)) = 0.211346
        _distScale ("dist Scale", Range(0, 1)) = 0.8353261
        _distSpeed ("dist Speed", Range(0, 1)) = 0.3376412
        _OUTcolour ("OUT colour", Color) = (1,0.3517241,0,1)
        _INcolour ("IN colour", Color) = (1,0.7796323,0.007352948,1)
        _distContrast ("dist Contrast", Range(0, 10)) = 2.750238
        _Vpush ("V push", Range(0, 10)) = 6.375284
        _distTEX ("dist TEX", 2D) = "white" {}
        _MasterOpacity ("Master Opacity", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
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
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _flamemask; uniform float4 _flamemask_ST;
            uniform float _distAmount;
            uniform sampler2D _alpha; uniform float4 _alpha_ST;
            uniform float _distScale;
            uniform float _distSpeed;
            uniform float4 _OUTcolour;
            uniform float4 _INcolour;
            uniform float _distContrast;
            uniform float _Vpush;
            uniform sampler2D _distTEX; uniform float4 _distTEX_ST;
            uniform float _MasterOpacity;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 node_1615 = _Time + _TimeEditor;
                float node_8040 = (node_1615.g*(_distScale*_distSpeed));
                float2 node_106 = (i.uv0*_distScale);
                float2 node_641 = ((node_106*0.5)+node_8040*float2(-1,-0.6));
                float4 node_8673 = tex2D(_distTEX,node_641);
                float2 node_9928 = (node_106+node_8040*float2(0.85,-1));
                float4 _node_8673_copy = tex2D(_distTEX,node_9928);
                float2 node_9314 = (node_106+node_8040*float2(-0.2,-0.92));
                float4 _node_8673_copy_copy = tex2D(_distTEX,node_9314);
                float2 node_4439 = float2(i.uv0.r,(((pow(((node_8673.r+_node_8673_copy.g)+_node_8673_copy_copy.b),_distContrast)*_distAmount)*(i.uv0.g*(1.0 - i.uv0.g)))+pow(i.uv0.g,_Vpush)));
                float4 _flamemask_var = tex2D(_flamemask,TRANSFORM_TEX(node_4439, _flamemask));
                float4 _alpha_var = tex2D(_alpha,TRANSFORM_TEX(i.uv0, _alpha));
                clip(pow(((1.0 - _flamemask_var.b)*_alpha_var.g),2.0) - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = ((_flamemask_var.r*_OUTcolour.rgb)+(_flamemask_var.g*_INcolour.rgb));
                float3 finalColor = emissive;
                return fixed4(finalColor,_MasterOpacity);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _flamemask; uniform float4 _flamemask_ST;
            uniform float _distAmount;
            uniform sampler2D _alpha; uniform float4 _alpha_ST;
            uniform float _distScale;
            uniform float _distSpeed;
            uniform float _distContrast;
            uniform float _Vpush;
            uniform sampler2D _distTEX; uniform float4 _distTEX_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 node_1615 = _Time + _TimeEditor;
                float node_8040 = (node_1615.g*(_distScale*_distSpeed));
                float2 node_106 = (i.uv0*_distScale);
                float2 node_641 = ((node_106*0.5)+node_8040*float2(-1,-0.6));
                float4 node_8673 = tex2D(_distTEX,node_641);
                float2 node_9928 = (node_106+node_8040*float2(0.85,-1));
                float4 _node_8673_copy = tex2D(_distTEX,node_9928);
                float2 node_9314 = (node_106+node_8040*float2(-0.2,-0.92));
                float4 _node_8673_copy_copy = tex2D(_distTEX,node_9314);
                float2 node_4439 = float2(i.uv0.r,(((pow(((node_8673.r+_node_8673_copy.g)+_node_8673_copy_copy.b),_distContrast)*_distAmount)*(i.uv0.g*(1.0 - i.uv0.g)))+pow(i.uv0.g,_Vpush)));
                float4 _flamemask_var = tex2D(_flamemask,TRANSFORM_TEX(node_4439, _flamemask));
                float4 _alpha_var = tex2D(_alpha,TRANSFORM_TEX(i.uv0, _alpha));
                clip(pow(((1.0 - _flamemask_var.b)*_alpha_var.g),2.0) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
