// port: shader funcional (shadertest)
Shader "Hidden/ProBuilder/VertexPicker" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest Always
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
			struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
			v2f vert(appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.color = v.color; o.uv = v.uv; return o; }
			float4 _Color;

			float4 frag(v2f i) : COLOR
			{
				float4 c = _Color * i.color;
				return c;
			}

			ENDCG
		}
	}
}
