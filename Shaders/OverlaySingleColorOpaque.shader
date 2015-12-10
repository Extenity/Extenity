Shader "Primitive/Overlay Single Color Opaque"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}
   
	SubShader
	{
		Tags { "Queue" = "Transparent+1" "RenderType" = "Opaque" "LightMode" = "Always" }
 
		Pass
		{          
			Lighting Off
			ZWrite Off
			ZTest Always
 
			Color[_Color]
		}
	} 

	Fallback "Unlit/Texture"
}
