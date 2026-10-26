// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "psx/trasparent/vertexlit" {
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
		Lighting On
		CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
// Uniform SUELTO (no en Properties): en el backend GLES de Unity/HLSLcc los
// uniforms del cbuffer UnityPerMaterial no se declaran en la etapa de vertex
// (undeclared identifier).
uniform float _ResY;
uniform float _TextureWarping;
uniform float _ColorPrecision;

	struct v2f
	{
		fixed4 pos : SV_POSITION;
		half4 color : COLOR0;
		half4 colorFog : COLOR1;
		float2 uv_MainTex : TEXCOORD0;
		half3 normal: TEXCOORD1;
	};

	float4 _MainTex_ST;
	uniform half4 unity_FogStart;
	uniform half4 unity_FogEnd;

	v2f vert(appdata_full v)
	{
		v2f o;

		//Vertex snapping
		float4 snapToPixel = UnityObjectToClipPos(v.vertex);
		float4 vertex = snapToPixel;
		vertex.xyz = snapToPixel.xyz / snapToPixel.w;
		// Pixelizacion: la rejilla la manda _ResY (0 = sin pixelizar).
		// Con el preset PC (_ResY = 0) el juego deja de verse PSX.
		if (_ResY > 0.5)
		{
			float gridX = _ResY * 1.7777778;
			vertex.x = floor(gridX * vertex.x) / gridX;
			vertex.y = floor(_ResY * vertex.y) / _ResY;
		}
		vertex.xyz *= snapToPixel.w;
		o.pos = vertex;


		//o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		//Vertex lighting 
		//o.color = float4(ShadeVertexLights(v.vertex, v.normal), 1.0);
		o.color = float4(ShadeVertexLightsFull(v.vertex, v.normal, 4, true), 1.0);
		o.color *= v.color;

		float distance = length(mul(UNITY_MATRIX_MV,v.vertex));

		//Affine Texture Mapping
		float4 affinePos = vertex;//vertex;				
		o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		if (_TextureWarping > 0.001) { o.uv_MainTex *= distance + (vertex.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2; }
		o.normal = distance + (vertex.w*(UNITY_LIGHTMODEL_AMBIENT.a * 8)) / distance / 2;

		//Fog
		float4 fogColor = unity_FogColor;

		float fogDensity = (unity_FogEnd - distance) / (unity_FogEnd - unity_FogStart);
		o.normal.g = fogDensity;
		o.normal.b = 1;

		o.colorFog = fogColor;
		o.colorFog.a = clamp(fogDensity,0,1);

		//Cut out polygons
		if (distance > unity_FogStart.z + unity_FogColor.a * 255)
		{
			o.pos.w = 0;
		}


		return o;
	}

	sampler2D _MainTex;

	float4 frag(v2f IN) : COLOR
	{
		half4 c = tex2D(_MainTex, (_TextureWarping > 0.001 ? IN.uv_MainTex / IN.normal.r : IN.uv_MainTex))*IN.color;
		half4 color = c*(IN.colorFog.a);
		color.rgb += IN.colorFog.rgb*(1 - IN.colorFog.a);
		color.a = c.a;
		// Compresion de color (_ColorPrecision) y pixelizacion (_ResY).
		// OJO: _ResY se usa aqui a proposito (ademas de en el vertex). En el
		// backend GLSL de Unity una property que solo se usa en la etapa de
		// vertex no llega a declararse ('undeclared identifier'), asi que tiene
		// que usarse en las dos etapas.
		float resY = _ResY > 0.5 ? _ResY : 2048.0;
		float prec = _ColorPrecision > 1.5 ? (_ColorPrecision - 1.0) : (resY - 1.0);
		color.rgb = floor(color.rgb * prec) / prec;
		return color;
	}
		ENDCG
	}
	}
}