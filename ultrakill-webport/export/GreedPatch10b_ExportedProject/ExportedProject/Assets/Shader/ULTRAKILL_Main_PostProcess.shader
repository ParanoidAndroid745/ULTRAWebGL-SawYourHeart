// port: shader funcional (shadertest)
Shader "ULTRAKILL/Main_PostProcess" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_UnderWaterScale("Underwater Scale", Range(0,10)) = 0
		_UnderWaterStrength("Underwater Strength", Range(0,1)) = 0
		_UnderWaterSpeed("Underwater Speed", Range(0,10)) = 0
		_ClampFinalOpacity("Clamp Final Opacity", Range(0,1)) = 1
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		Pass {
			Blend One OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ClampFinalOpacity;
			struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
			struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
			v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = TRANSFORM_TEX(v.uv, _MainTex); return o; }
			float4 frag(v2f i) : COLOR
			{
				half4 c = tex2D(_MainTex, i.uv);
				c.a = saturate(c.a * _ClampFinalOpacity);
				return c;
			}
			ENDCG
		}
	}
}
