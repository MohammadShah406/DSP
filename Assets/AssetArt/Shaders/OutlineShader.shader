Shader "Unlit/OutlineShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth("Outline Width", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            // Render outline first (slightly scaled)
            Name "OUTLINE"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front // Invert normals to render outline
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float _OutlineWidth;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                posWS += normalWS * _OutlineWidth; // Push vertices along normal
                OUT.positionCS = TransformWorldToHClip(posWS);
                return OUT;
            }

            float4 _OutlineColor;

            float4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // Base mesh pass
        Pass
        {
            Name "BASE"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vertBase(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float4 _BaseColor;

            float4 fragBase(Varyings IN) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
