using UnityEngine;

[CreateAssetMenu(fileName = "PreloadedShaders.asset", menuName = "Subnautica/Create PreloadedShaders")]
public class PreloadedShaders : ScriptableObject
{
	private const int spaceValue = 15;

	[UseShaderSelector]
	public Shader voxelandBlockTypeTopLayer;

	[UseShaderSelector]
	public Shader voxelandBlockTypeOpaque;

	[UseShaderSelector]
	public Shader voxelandOpaque;

	[UseShaderSelector]
	public Shader voxelandHighlight;

	[Space(15f)]
	[UseShaderSelector]
	public Shader marmosetUBER;

	[Space(15f)]
	[UseShaderSelector]
	public Shader scannerToolScanning;

	[UseShaderSelector]
	public Shader VFXWeatherManagerZDepth;

	[UseShaderSelector]
	public Shader uSkyManagerStar;

	[Space(15f)]
	[UseShaderSelector]
	public Shader UICircularBar;

	[UseShaderSelector]
	public Shader UIIcon;

	[UseShaderSelector]
	public Shader UIIconBar;

	[Space(15f)]
	[UseShaderSelector]
	public Shader HighlighterCoreOpaque;

	[UseShaderSelector]
	public Shader HighlighterCoreTransparent;

	[UseShaderSelector]
	public Shader[] HighlightingBaseShaders;

	[Space(15f)]
	[UseShaderSelector]
	public Shader[] PAParticleFieldShaders;

	[UseShaderSelector]
	public Shader PAParticleFieldDefault;

	[Space(15f)]
	[UseShaderSelector]
	public Shader GammaCorrection;

	[Space(15f)]
	[UseShaderSelector]
	public Shader DebugDisplaySolid;
}
