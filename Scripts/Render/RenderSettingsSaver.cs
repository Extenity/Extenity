using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class RenderSettingsSaver : MonoBehaviour
{
	public Color ambientEquatorColor;
	public Color ambientGroundColor;
	public float ambientIntensity;
	public Color ambientLight;
	public AmbientMode ambientMode;
	public SphericalHarmonicsL2 ambientProbe;
	public Color ambientSkyColor;
	public Cubemap customReflection;
	public DefaultReflectionMode defaultReflectionMode;
	public int defaultReflectionResolution;
	public float flareFadeSpeed;
	public float flareStrength;
	public bool fog;
	public Color fogColor;
	public float fogDensity;
	public float fogEndDistance;
	public FogMode fogMode;
	public float fogStartDistance;
	public float haloStrength;
	public int reflectionBounces;
	public float reflectionIntensity;
	public Material skybox;

	private void Awake()
	{
		SetConfigurationToRenderSettings();
	}

	public void GetConfigurationFromRenderSettings()
	{
		ambientEquatorColor = RenderSettings.ambientEquatorColor;
		ambientGroundColor = RenderSettings.ambientGroundColor;
		ambientIntensity = RenderSettings.ambientIntensity;
		ambientLight = RenderSettings.ambientLight;
		ambientMode = RenderSettings.ambientMode;
		ambientProbe = RenderSettings.ambientProbe;
		ambientSkyColor = RenderSettings.ambientSkyColor;
		customReflection = RenderSettings.customReflection;
		defaultReflectionMode = RenderSettings.defaultReflectionMode;
		defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
		flareFadeSpeed = RenderSettings.flareFadeSpeed;
		flareStrength = RenderSettings.flareStrength;
		fog = RenderSettings.fog;
		fogColor = RenderSettings.fogColor;
		fogDensity = RenderSettings.fogDensity;
		fogEndDistance = RenderSettings.fogEndDistance;
		fogMode = RenderSettings.fogMode;
		fogStartDistance = RenderSettings.fogStartDistance;
		haloStrength = RenderSettings.haloStrength;
		reflectionBounces = RenderSettings.reflectionBounces;
		reflectionIntensity = RenderSettings.reflectionIntensity;
		skybox = RenderSettings.skybox;
	}

	public void SetConfigurationToRenderSettings()
	{
		RenderSettings.ambientEquatorColor = ambientEquatorColor;
		RenderSettings.ambientGroundColor = ambientGroundColor;
		RenderSettings.ambientIntensity = ambientIntensity;
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.ambientMode = ambientMode;
		RenderSettings.ambientProbe = ambientProbe;
		RenderSettings.ambientSkyColor = ambientSkyColor;
		RenderSettings.customReflection = customReflection;
		RenderSettings.defaultReflectionMode = defaultReflectionMode;
		RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
		RenderSettings.flareFadeSpeed = flareFadeSpeed;
		RenderSettings.flareStrength = flareStrength;
		RenderSettings.fog = fog;
		RenderSettings.fogColor = fogColor;
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogEndDistance = fogEndDistance;
		RenderSettings.fogMode = fogMode;
		RenderSettings.fogStartDistance = fogStartDistance;
		RenderSettings.haloStrength = haloStrength;
		RenderSettings.reflectionBounces = reflectionBounces;
		RenderSettings.reflectionIntensity = reflectionIntensity;
		RenderSettings.skybox = skybox;
	}
}
