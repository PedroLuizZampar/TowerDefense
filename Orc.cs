using Godot;

public partial class Orc : Enemy
{
	// ---- CONFIGURAÇÃO DO ORC ----
	[Export] public string OrcName { get; set; } = "Orc";
	[Export] public int OrcMaxHealth { get; set; } = 20;
	[Export] public float OrcMoveSpeed { get; set; } = 120.0f;

	protected override string EnemyName => OrcName;
	protected override int MaxHealth => OrcMaxHealth;
	protected override float MoveSpeed => OrcMoveSpeed;

	private AnimatedSprite2D _anim;

	public override void _Ready()
	{
		// Obtém animação
		_anim = GetNodeOrNull<AnimatedSprite2D>("OrcWalkAnimation");
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
