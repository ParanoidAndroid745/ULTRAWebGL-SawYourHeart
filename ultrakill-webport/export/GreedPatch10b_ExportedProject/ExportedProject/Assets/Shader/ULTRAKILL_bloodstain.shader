// port: shader funcional (shadertest)
Shader "ULTRAKILL/bloodstain" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_OpacScale("Opacity", Range(0,2)) = 1
		_VertexWarpScale("Vertex Warp Scale", Range(0,10)) = 1
	}
	SubShader {
		Tags { "Queue" = "Transparent-1" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 200

		Pass {
			Blend SrcAlpha SrcColor
			ZWrite Off
			Cull Back
			Offset -1, -1
			Lighting On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			uniform float _ResY;
			uniform float _TextureWarping;
			uniform float _ColorPrecision;

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				half4 color : COLOR0;
				half4 colorFog : COLOR1;
				float2 uv : TEXCOORD0;
				half3 normal : TEXCOORD1;
			};

			float4 _MainTex_ST;
			uniform half4 unity_FogStart;
			uniform half4 unity_FogEnd;


			v2f vert(appdata_full v)
			{
				v2f o;


				// Vertex snapping + pixelizacion (con _ResY = 0 no hace nada).
				float4 snapToPixel = UnityObjectToClipPos(v.vertex);
				float4 vertex = snapToPixel;
				vertex.xyz = snapToPixel.xyz / snapToPixel.w;
				if (_ResY > 0.5)
				{
					float gridX = _ResY * 1.7777778;
					vertex.x = floor(gridX * vertex.x) / gridX;
					vertex.y = floor(_ResY * vertex.y) / _ResY;
				}
				vertex.xyz *= snapToPixel.w;
				o.pos = vertex;

				o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);

				float distance = length(mul(UNITY_MATRIX_MV, v.vertex));

				// Affine texture mapping: con _TextureWarping = 0 es correcto.
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				if (_TextureWarping > 0.001) { o.uv *= distance + (vertex.w * (UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2; }
				o.normal = distance + (vertex.w * (UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;
				o.normal.g = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
				o.normal.b = 1;

				o.colorFog = unity_FogColor;
				o.colorFog.a = clamp((unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart), 0, 1);

				// Recorte de poligonos lejanos, como en el resto del port.
				if (distance > unity_FogStart.z + unity_FogColor.a * 255) { o.pos.w = 0; }

				return o;
			}


			sampler2D _MainTex;
			float4 _Color;
			float _OpacScale;

			float4 frag(v2f IN) : COLOR
			{
				float2 uv = _TextureWarping > 0.001 ? IN.uv / IN.normal.r : IN.uv;
				half4 tex = tex2D(_MainTex, uv);
				float a = saturate(tex.a * _OpacScale * _Color.a);
				// Multiplicativo: donde no hay sangre el factor es 1 y el fondo no
				// se toca; donde la hay, el fondo se tine de rojo (translucido).
				half3 tint = lerp(half3(1, 1, 1), tex.rgb * _Color.rgb, a);
				half4 color = half4(tint, 0);


				return color;
			}
			ENDCG

		}
	}
}
