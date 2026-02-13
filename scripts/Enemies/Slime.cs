using Godot;

/// <summary>
/// Inimigo tipo Slime.
/// Implementação concreta que herda da classe Enemy.
/// Possui animações para caminhar em 4 direções e velocidade baixa.
/// </summary>
public partial class Slime : Enemy
{
	/// <summary>Nome único do inimigo</summary>
	protected override string EnemyName => "Slime";

	/// <summary>Vida máxima do Slime</summary>
	protected override int MaxHealth => 10;

	/// <summary>Velocidade de movimento do Slime em pixels por segundo</summary>
	protected override float MoveSpeed => 160.0f;

	private AnimatedSprite2D _anim;

	public override void _Ready()
	{
		_anim = GetNodeOrNull<AnimatedSprite2D>("SlimeWalkAnimation");
		base._Ready();
	}

	/// <summary>Atualiza a animação de caminhada baseado na direção do movimento</summary>
	protected override void UpdateWalkAnimation(Vector2 movement)
	{
		if (_anim == null)
			return;
		if (movement.LengthSquared() < 0.0001f)
			return;

		Vector2 dir = movement.Normalized();

		// Escolhe animação baseada na direção principal (horizontal ou vertical)
		if (Mathf.Abs(dir.X) >= Mathf.Abs(dir.Y))
		{
			_anim.Play(dir.X > 0 ? "R_Walk" : "L_Walk");
		}
		else
		{
			_anim.Play(dir.Y > 0 ? "D_Walk" : "U_Walk");
		}
	}
}
