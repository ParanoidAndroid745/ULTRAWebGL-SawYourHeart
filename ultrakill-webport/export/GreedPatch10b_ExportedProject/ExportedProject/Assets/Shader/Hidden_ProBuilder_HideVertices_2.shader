// Original: ProBuilder editor helper. In ULTRAKILL the InvisibleFace*
// materials point at this shader and must not draw anything.
// The AssetRipper stub drew an opaque surface, making invisible blockers
// (e.g. the long PipesHolder boxes in 0-2) visible in game.
Shader "Hidden/ProBuilder/HideVertices" {
	SubShader {
		Tags { "IgnoreProjector"="True" "RenderType"="Opaque" }
		Pass {
			ColorMask 0
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				return fixed4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
	Fallback Off
}
