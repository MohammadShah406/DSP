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
            // Render outline first (slightly offset in clip-space for consistent pixel width)
            Name "OUTLINE"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
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
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            // Computes a screen-space offset based on the view-space normal.
            // This produces a constant pixel-width outline and behaves better on sharp edges.
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Object -> World -> View
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                // View-space normal (direction only)
                float3 normalVS = TransformWorldToViewDir(normalWS);

                // Project the vertex
                float4 posCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Convert outline width to NDC units (approx):
                // Scale by posCS.w to keep thickness constant across depth (perspective)
                // Use only the XY components for screen offset
                float2 nxy = normalize(normalVS.xy);

                // Guard against degenerate normals (e.g., straight-on faces)
                // If normal projects poorly, fall back to view direction-based offset
                float eps = 1e-4;
                if (abs(nxy.x) + abs(nxy.y) < eps)
                {
                    // Use a small offset along a unit screen diagonal to avoid zero-length
                    nxy = normalize(float2(1.0, 1.0));
                }

                // Convert an approximate pixel-size to clip-space:
                // OutlineWidth is treated as a pixel-ish scale. URP doesn't expose screen size here,
                // so we approximate using posCS.w for perspective invariance.
                float2 offsetCS = nxy * _OutlineWidth * posCS.w;

                // Apply offset in clip space (XY only)
                posCS.xy += offsetCS;

                OUT.positionCS = posCS;
                return OUT;
            }

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