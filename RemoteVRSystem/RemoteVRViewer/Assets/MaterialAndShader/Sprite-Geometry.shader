Shader "Unlit/Sprite-Geometry"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _Offset("Offset",Range(0.0001,0.1)) = 0.0016
    }
    SubShader
    {
        Tags { "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True" }
        Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma geometry geom
            #pragma fragment SpriteFrag
            // make fog work
            #pragma multi_compile_fog
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            struct g2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Offset;

            [maxvertexcount(4)]
            void geom(point v2f input[1], inout PointStream<g2f> outStream)
            {
                g2f o;
                o.vertex = input[0].vertex;
                o.color = input[0].color;
                o.texcoord = input[0].texcoord;
                outStream.Append(o);
                
                o.vertex = input[0].vertex; o.vertex += float4(_Offset, 0, 0, 0); outStream.Append(o);
                o.vertex = input[0].vertex; o.vertex += float4(-_Offset, 0, 0, 0); outStream.Append(o);
                o.vertex = input[0].vertex; o.vertex += float4(0, _Offset, 0, 0); outStream.Append(o);
                o.vertex = input[0].vertex; o.vertex += float4(0, -_Offset, 0, 0); outStream.Append(o);

                //outStream.RestartStrip();
            }
            ENDCG
        }
    }
}
