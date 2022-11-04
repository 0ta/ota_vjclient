Shader "otavj/CreateRT4Sender"
{
    Properties
    {
        _MainTex("", 2D) = "black" {}
        _SourceTex("", 2D) = "black" {}
        _HumanStencil("", 2D) = "black" {}
        _EnvironmentDepth("", 2D) = "black" {}
     }

     SubShader
     {
        Pass
        {
            Cull Off ZTest Always ZWrite Off
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex Vertex
            #pragma fragment Fragment
            //#pragma target 3.0

            sampler2D _SourceTex;
            sampler2D _HumanStencil;
            sampler2D _EnvironmentDepth;
            float2 _DepthRange;

            float3 Hue2RGB(float hue)
            {
                float h = hue * 6 - 2;
                float r = abs(h - 1) - 1;
                float g = 2 - abs(h);
                float b = 2 - abs(h - 2);
                return saturate(float3(r, g, b));
            }

            void Vertex(float4 vertex : POSITION,
                        float2 texCoord : TEXCOORD,
                        out float4 outVertex : SV_Position,
                        out float2 outTexCoord : TEXCOORD)
            {
                outVertex = UnityObjectToClipPos(vertex);
                outTexCoord = texCoord;
            }

            float4 Fragment(float4 vertex : SV_Position,
                            float2 texCoord : TEXCOORD) : SV_Target
            {
                float4 tc = frac(texCoord.xyxy * float4(1, 1, 2, 2));

                float3 main = tex2D(_SourceTex, tc.zy);
                float3 mask = tex2D(_HumanStencil, tc.zw);
                float3 depth = tex2D(_EnvironmentDepth, tc.zw);

                depth = (depth - _DepthRange.x) / (_DepthRange.y - _DepthRange.x);
                float3 findepth = Hue2RGB(clamp(depth, 0, 0.8));

                //float3 srgb = texCoord.x < 0.5 ? main : (texCoord.y < 0.5 ? Hue2RGB(depth.x) : mask);
                float3 srgb = texCoord.x < 0.5 ? main : (texCoord.y < 0.5 ? findepth : mask);

                //return float4(GammaToLinearSpace(srgb), 1);
                return float4(srgb, 1);
            }
            ENDCG
        }
    }
}
