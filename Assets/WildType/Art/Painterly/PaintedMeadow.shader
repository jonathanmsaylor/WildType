Shader "WildType/Painted Meadow"
{
    Properties
    {
        _BaseColor("Coat / tint", Color) = (1,1,1,1)
        _MarkColor("Inherited marking", Color) = (1,.94,.76,1)
        _Pattern("Coat bands", Float) = 0
        _BandWidth("Inherited band width", Float) = .18
        _BrushScale("Brush scale", Float) = 1
        _BaseMap("White", 2D) = "white" {}
        _Cutoff("Cutoff", Float) = .5
        _Cull("Cull", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "PaintedForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor, _MarkColor;
            float _Pattern, _BandWidth, _BrushScale;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; half4 color:COLOR; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; half4 color:COLOR; float2 uv:TEXCOORD2; float3 local:TEXCOORD3; half fog:TEXCOORD4; };
            float Hash(float3 p) { p=frac(p*.1031); p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z); }
            float Noise(float3 p) { float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(Hash(i),Hash(i+float3(1,0,0)),f.x),lerp(Hash(i+float3(0,1,0)),Hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(Hash(i+float3(0,0,1)),Hash(i+float3(1,0,1)),f.x),lerp(Hash(i+float3(0,1,1)),Hash(i+1),f.x),f.y),f.z); }
            V Vert(A v) { V o; VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);o.positionCS=p.positionCS;o.world=p.positionWS;o.local=v.positionOS.xyz;o.normal=TransformObjectToWorldNormal(v.normalOS);o.color=v.color;o.uv=v.uv;o.fog=ComputeFogFactor(p.positionCS.z);return o; }
            half4 Frag(V i, FRONT_FACE_TYPE facing:FRONT_FACE_SEMANTIC):SV_Target
            {
                float3 n=normalize(i.normal)*IS_FRONT_VFACE(facing,1,-1);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                float stroke=Noise(i.local*float3(13,30,5)*_BrushScale);
                float wash=Noise(i.local*3.4*_BrushScale);
                float band=abs(frac(i.uv.x*6+stroke*.045)-.5);
                float edge=max(fwidth(band)*1.5,.016);
                float marking=(1-smoothstep(_BandWidth-edge,_BandWidth+edge,band))*_Pattern*smoothstep(.08,.35,i.uv.y)*(1-smoothstep(.48,.60,i.uv.x));
                half3 base=lerp(_BaseColor.rgb*i.color.rgb,_MarkColor.rgb,marking);
                base*=.89+.14*wash+.09*stroke;
                float ndl=saturate(dot(n,sun.direction)*.65+.35);
                float light=smoothstep(.1,.85,ndl)*lerp(.38,1,sun.shadowAttenuation);
                half3 shade=lerp(half3(.38,.48,.57),half3(1.04,1.01,.82)*sun.color,light);
                half3 rgb=base*(shade*.72+SampleSH(n)*.42);
                return half4(MixFog(rgb,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
