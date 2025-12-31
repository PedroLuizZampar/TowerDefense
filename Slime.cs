using Godot;

public partial class Slime : Enemy
{
	// ---- CONFIGURAÇÃO DO SLIME ----
	protected override string EnemyName => "Slime";
	protected override int MaxHealth => 10;
	protected override float MoveSpeed => 160.0f;

	private AnimatedSprite2D _anim;

	public override void _Ready()
	{
		// Obtém animação
		_anim = GetNodeOrNull<AnimatedSprite2D>("SlimeWalkAnimation");
		base._Ready();
	}

	// Animação
	protected override void UpdateWalkAnimation(Vector2 movement)
	{
		if (_anim == null)
			return;
		if (movement.LengthSquared() < 0.0001f)
			return;

		Vector2 dir = movement.Normalized();

		// Direção principal horizontal vs vertical.
		if (Mathf.Abs(dir.X) >= Mathf.Abs(dir.Y))
		{
			if (dir.X > 0)
				_anim.Play("R_Walk");
			else
				_anim.Play("L_Walk");
		}
		else
		{
			if (dir.Y > 0)
				_anim.Play("D_Walk");
			else
				_anim.Play("U_Walk");
		}
	}
}
