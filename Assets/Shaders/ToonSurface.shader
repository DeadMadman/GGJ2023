Shader "Custom/ToonSurface"
{
    Properties
    {
        MainColor ("Color", Color) = (1,1,1,1)
        MainTexture ("MainTexture", 2D) = "white" {}
        RampTexture ("RampTexture", 2D) = "white" {}
        
        OutlineColor ("OutlineColor", Color) = (1,1,1,1)
        OutlineWidth ("OutlineWidth", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        Cull off
        ZWrite off
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        struct Input
        {
            float2 uvMainTexture;
        };

        fixed4 OutlineColor;
        float OutlineWidth;
        
        void vert (inout appdata_full v)
        {
            v.vertex.xyz += normalize(v.normal) * OutlineWidth;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Emission = OutlineColor.rgb;
        }
        
        ENDCG

        ZWrite on

        CGPROGRAM
        #pragma surface surf ToonRamp

        fixed4 MainColor;
        sampler2D MainTexture;
        sampler2D RampTexture;

        half4 LightingToonRamp (SurfaceOutput o, half3 lightDir, fixed atten)
        {
            half diff = dot(o.Normal, lightDir);
            float h = diff * 0.5 + 0.5;
            float2 rh = h;
            float3 ramp = tex2D(RampTexture, rh).rgb;
  
            half4 c;
            c.rgb = o.Albedo * _LightColor0.rgb * ramp;
            c.a = o.Alpha;
            return c;
        }
        
        struct Input
        {
            float2 uvMainTexture;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (MainTexture, IN.uvMainTexture);
            o.Albedo = c * MainColor.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
