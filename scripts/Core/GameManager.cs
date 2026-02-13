using Godot;

/// <summary>
/// Gerenciador central do jogo.
/// Controla o fluxo geral (início, pausa, retomada, fim).
/// Funciona como um Singleton acessível globalmente.
/// </summary>
public partial class GameManager : Node
{
	/// <summary>Instância singleton do GameManager</summary>
	public static GameManager Instance { get; private set; }

	/// <summary>Estado atual do jogo</summary>
	public GameState CurrentGameState { get; private set; }

	/// <summary>Vida atual do jogador</summary>
	public int PlayerHealth { get; private set; } = 100;

	/// <summary>Dinheiro do jogador</summary>
	public int PlayerMoney { get; private set; } = 500;

	/// <summary>Pontuação total do jogador</summary>
	public int PlayerScore { get; private set; } = 0;

	/// <summary>Número da onda atual</summary>
	public int CurrentWaveNumber { get; private set; } = 0;

	/// <summary>Velocidade do tempo do jogo (1.0 = normal, 0.5 = metade, 2.0 = dobro)</summary>
	private float _timeScale = 1.0f;

	public float TimeScale
	{
		get => _timeScale;
		set
		{
			_timeScale = Mathf.Max(0f, value);
			Engine.TimeScale = _timeScale;
		}
	}

	private WaveManager _waveManager;

	public override void _Ready()
	{
		// Implementar Singleton
		if (Instance != null && Instance != this)
		{
			GD.PushWarning("GameManager: Outra instância já existe. Removendo duplicada.");
			QueueFree();
			return;
		}

		Instance = this;

		// Inicializar estado
		CurrentGameState = GameState.MainMenu;

		// Obter referência ao WaveManager
		_waveManager = GetNodeOrNull<WaveManager>("WaveManager");
		if (_waveManager == null)
		{
			GD.PushWarning("GameManager: WaveManager não encontrado como filho.");
		}

		// Inscrever em eventos de inimigos
		GameEvents.OnEnemyDied += HandleEnemyDied;
		GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;
		GameEvents.OnEnemyDamageTaken += HandleEnemyDamageTaken;

		GD.Print("GameManager inicializado com sucesso!");
	}

	public override void _ExitTree()
	{
		// Desinscrever de eventos
		GameEvents.OnEnemyDied -= HandleEnemyDied;
		GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
		GameEvents.OnEnemyDamageTaken -= HandleEnemyDamageTaken;

		if (Instance == this)
			Instance = null;
	}

	/// <summary>Inicia o jogo</summary>
	public void StartGame()
	{
		if (CurrentGameState != GameState.MainMenu && CurrentGameState != GameState.GameOver)
		{
			GD.PushWarning("GameManager: Não é possível iniciar um jogo que já está em progresso.");
			return;
		}

		PlayerHealth = 100;
		PlayerMoney = 500;
		PlayerScore = 0;
		CurrentWaveNumber = 0;

		CurrentGameState = GameState.Playing;
		Engine.TimeScale = 1.0f;

		GameEvents.InvokeGameStarted();
		GD.Print("Jogo iniciado!");

		if (_waveManager != null)
			_waveManager.StartWaves();
	}

	/// <summary>Pausa o jogo</summary>
	public void PauseGame()
	{
		if (CurrentGameState != GameState.Playing)
		{
			GD.PushWarning("GameManager: Só é possível pausar um jogo em progresso.");
			return;
		}

		CurrentGameState = GameState.Paused;
		Engine.TimeScale = 0f;
		GameEvents.InvokeGamePaused();
		GD.Print("Jogo pausado!");
	}

	/// <summary>Retoma o jogo após pausa</summary>
	public void ResumeGame()
	{
		if (CurrentGameState != GameState.Paused)
		{
			GD.PushWarning("GameManager: Só é possível retomar um jogo pausado.");
			return;
		}

		CurrentGameState = GameState.Playing;
		Engine.TimeScale = 1.0f;
		GameEvents.InvokeGameResumed();
		GD.Print("Jogo retomado!");
	}

	/// <summary>Alterna entre pausa e jogo ativo</summary>
	public void TogglePause()
	{
		if (CurrentGameState == GameState.Playing)
			PauseGame();
		else if (CurrentGameState == GameState.Paused)
			ResumeGame();
	}

	/// <summary>Termina o jogo (vitória/derrota)</summary>
	public void EndGame(bool victory)
	{
		if (CurrentGameState == GameState.GameOver)
			return;

		CurrentGameState = GameState.GameOver;
		Engine.TimeScale = 0f;

		if (victory)
		{
			GD.Print("Vitória! Todos os inimigos foram derrotados!");
		}
		else
		{
			GD.Print("Derrota! Vida chegou a 0!");
		}
	}

	/// <summary>Adiciona dinheiro ao jogador</summary>
	public void AddMoney(int amount)
	{
		if (amount < 0)
		{
			GD.PushWarning($"GameManager: Tentativa de adicionar dinheiro negativo ({amount}).");
			return;
		}

		PlayerMoney += amount;
		GD.Print($"Dinheiro adicionado: +{amount}. Total: {PlayerMoney}");
	}

	/// <summary>Remove dinheiro do jogador (ex: compra de torre)</summary>
	public bool TrySpendMoney(int amount)
	{
		if (amount <= 0)
		{
			GD.PushWarning($"GameManager: Tentativa de gastar dinheiro inválido ({amount}).");
			return false;
		}

		if (PlayerMoney < amount)
		{
			GD.Print($"Dinheiro insuficiente! Necessário: {amount}, Possui: {PlayerMoney}");
			return false;
		}

		PlayerMoney -= amount;
		GD.Print($"Dinheiro gasto: -{amount}. Total: {PlayerMoney}");
		return true;
	}

	/// <summary>Adiciona pontuação</summary>
	public void AddScore(int points)
	{
		if (points < 0)
		{
			GD.PushWarning($"GameManager: Tentativa de adicionar pontuação negativa ({points}).");
			return;
		}

		PlayerScore += points;
		GD.Print($"Pontuação adicionada: +{points}. Total: {PlayerScore}");
	}

	/// <summary>Inflige dano ao jogador</summary>
	public void TakeDamage(int damage)
	{
		if (damage <= 0)
		{
			GD.PushWarning($"GameManager: Tentativa de infligir dano inválido ({damage}).");
			return;
		}

		PlayerHealth = Mathf.Max(0, PlayerHealth - damage);
		GD.Print($"Dano sofrido: -{damage}. Saúde restante: {PlayerHealth}");

		if (PlayerHealth <= 0)
		{
			EndGame(false);
		}
	}

	/// <summary>Define o número da onda atual</summary>
	public void SetCurrentWave(int waveNumber)
	{
		CurrentWaveNumber = waveNumber;
		GD.Print($"Onda {waveNumber} iniciada!");
	}

	// ---- MANIPULADORES DE EVENTOS ----

	private void HandleEnemyDied(Enemy enemy)
	{
		AddMoney(50); // +50 moedas por inimigo morto
		AddScore(100); // +100 pontos por inimigo morto
		GD.Print($"Inimigo {enemy.DisplayName} morreu!");
	}

	private void HandleEnemyReachedEnd(Enemy enemy)
	{
		TakeDamage(10); // -10 vida se inimigo chegar no final
		GD.Print($"Inimigo {enemy.DisplayName} chegou ao final do caminho!");
	}

	private void HandleEnemyDamageTaken(Enemy enemy, int damage)
	{
		// Pode ser usado para efeitos visuais, som, etc
		// Por enquanto apenas log
		GD.Print($"Inimigo {enemy.DisplayName} recebeu {damage} de dano. Vida: {enemy.CurrentHealth}");
	}
}

/// <summary>Estados possíveis do jogo</summary>
public enum GameState
{
	MainMenu,
	Playing,
	Paused,
	GameOver
}
