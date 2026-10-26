// port: shader funcional (shadertest)
Shader "psx/railgun" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_EmissiveColor("Emissive Color (RGB)", Color) = (1,1,1,1)
		_EmissiveIntensity("Emissive Strength", Range(0,8)) = 1
		_EmissivePosition("Emissive Position", Range(0,10)) = 0
		_Outline("Outline", Range(0,1)) = 0
		_VertexWarpScale("Vertex Warp Scale", Range(0,10)) = 1
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 200

		Pass {
			ZWrite On
			Cull Back
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
			float4 _EmissiveColor;
			float _EmissiveIntensity;

			float4 frag(v2f IN) : COLOR
			{
				float2 uv = _TextureWarping > 0.001 ? IN.uv / IN.normal.r : IN.uv;
				half4 albedo = tex2D(_MainTex, uv) * _Color;
				half4 c = albedo * IN.color;
				// El railgun lleva el brillo en la propia textura: el emisivo la
				// realza sin depender de la luz.
				c.rgb += albedo.rgb * _EmissiveColor.rgb * _EmissiveIntensity;

				half4 color = c * IN.colorFog.a;
				color.rgb += IN.colorFog.rgb * (1 - IN.colorFog.a);
				float resY = _ResY > 0.5 ? _ResY : 2048.0;
				float prec = _ColorPrecision > 1.5 ? (_ColorPrecision - 1.0) : (resY - 1.0);
				color.rgb = floor(color.rgb * prec) / prec;


				return color;
			}
			ENDCG

		}
	}
}
