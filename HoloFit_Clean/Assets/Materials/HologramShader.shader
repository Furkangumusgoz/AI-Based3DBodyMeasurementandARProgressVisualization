Shader "Custom/Hologram" {
    Properties {
        _Color ("Main Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _EmissionColor ("Emission Color", Color) = (0.0, 0.8, 1.0, 1.0)
        _RimPower ("Hologram Parlakligi (Rim Power)", Range(0.5, 8.0)) = 2.5
        _Alpha ("Iç Seffaflik (Alpha)", Range(0, 1)) = 0.3
    }
    SubShader {
        // Þeffaflýk ayarlarý
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        
        // Arka planla karýþma (Additive/Alpha)
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        struct Input {
            float3 viewDir;
        };

        fixed4 _Color;
        fixed4 _EmissionColor;
        float _RimPower;
        float _Alpha;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Ana Renk
            o.Albedo = _Color.rgb;
            
            // Fresnel (Kenar Parlamasý) hesaplamasý
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            
            // Kenarlarý parlat
            o.Emission = _EmissionColor.rgb * pow (rim, _RimPower);
            
            // Kenarlar daha opak, ortalar daha þeffaf olsun
            o.Alpha = _Alpha + (pow(rim, _RimPower) * 0.7);
        }
        ENDCG
    }
    FallBack "Diffuse"
}