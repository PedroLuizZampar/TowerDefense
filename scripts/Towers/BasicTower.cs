using Godot;

/// <summary>
/// Torre básica com alcance padrão de 250 pixels.
/// Implementação simples que herda toda a lógica de detecção e targeting de Tower.
/// </summary>
public partial class BasicTower : Tower
{
	/// <summary>Alcance padrão dessa torre em pixels</summary>
	protected override float DefaultRangeRadius => 250f;
}
