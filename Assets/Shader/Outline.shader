Shader "Custom/Outline"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TestColor("Test color", Color) = (1, 1, 1, 1) // For Testing, Mask the background color
	}

		CGINCLUDE
#include"UnityCG.cginc"

			uniform sampler2D _MainTex;
		uniform sampler2D _MainTex_ST;
		uniform float4 _MainTex_TexelSize;
		sampler2D_float _CameraDepthTexture;
		sampler2D _CameraDepthNormalsTexture;

		uniform float4 _EdgeColor;
		uniform float _Exponent;
		uniform float _SampleDistance; // To control the edge width
		uniform float _FilterPower;
		uniform float _Threshold;
		uniform float4 _TestColor;
		uniform float _BgFade;
		uniform float3 _LightDir; // For toon shading
		uniform int _bToonShader;

		struct appdata
		{
			float4 vertex: POSITION;
			float4 normal: NORMAL;
			float2 uv: TEXCOORD0;
		};

		struct v2f
		{
			float2 uv: TEXCOORD0;
			float4 vertex: SV_POSITION;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		ENDCG
}
