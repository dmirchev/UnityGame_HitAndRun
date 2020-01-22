Shader "Unlit/Gradient"
{
    Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorLow ("Color Low", COLOR) = (1,1,1,1)
		_yPosHigh ("Y Pos High", Float) = -5
		_yPosLow ("Y Pos Low", Float) = -20
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			fixed4 _ColorLow;

			half _yPosLow;
			half _yPosHigh;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col;
				fixed4 tex = tex2D(_MainTex, i.uv);

				fixed3 gradient = lerp(_ColorLow, tex, smoothstep(_yPosLow, _yPosHigh, i.worldPos.y)).rgb;
				fixed4 col = fixed4(gradient, 1);
				
				return col;
			}
			ENDCG
		}
	}
}
