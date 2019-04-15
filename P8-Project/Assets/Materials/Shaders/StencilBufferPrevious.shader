﻿Shader "Stencils/Materials/StencilBufferPrevious" {
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+300" }
			Stencil
			{
				Ref 1
				Comp equal
				Pass keep
				Fail keep
			}

		CGPROGRAM
		#pragma surface surf Lambert noshadow

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
		}
	Fallback Off
}
