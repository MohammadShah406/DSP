Shader "Skybox/AllSky Blended Cubemap"
{
    Properties
    {
        _Tex1("Cubemap A", CUBE) = "" {}
        _Tex2("Cubemap B", CUBE) = "" {}
        _Rotation("Rotation", Range(0, 360)) = 0
        _Exposure("Exposure", Range(0, 8)) = 1
        _Blend("Blend", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            samplerCUBE _Tex1;
            samplerCUBE _Tex2;

            float _Rotation;
            float _Exposure;
            float _Blend;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            float3 RotateAroundY(float3 v, float degrees)
            {
                float a = radians(degrees);
                float s = sin(a);
                float c = cos(a);
                return float3(c * v.x + s * v.z, v.y, -s * v.x + c * v.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                dir = RotateAroundY(dir, _Rotation);

                half4 c1 = texCUBE(_Tex1, dir);
                half4 c2 = texCUBE(_Tex2, dir);

                half4 col = lerp(c1, c2, saturate(_Blend)) * _Exposure;
                return col;
            }
            ENDHLSL
        }
    }
    Fallback Off
}