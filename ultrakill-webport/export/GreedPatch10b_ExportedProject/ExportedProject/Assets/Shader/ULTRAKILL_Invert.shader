// port: shader funcional (shadertest)
Shader "ULTRAKILL/Invert" {
	Properties {
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		Pass {
			Blend OneMinusDstColor Zero
			ZWrite Off
			ZTest Always
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata { float4 vertex : POSITION; };
			struct v2f { float4 pos : SV_POSITION; };
			v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
			float4 frag(v2f i) : COLOR { return float4(1,1,1,1); }
			ENDCG
		}
	}
}
