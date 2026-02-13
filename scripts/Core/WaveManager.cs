using Godot;
using System.Collections.Generic;

/// <summary>
/// Gerenciador de ondas de inimigos.
/// Controla o spawn de inimigos, fluxo de ondas e dificuldade progressiva.
/// </summary>
public partial class WaveManager : Node
{
	/// <summary>
	/// Definição de uma onda de inimigos
	/// </summary>
	[System.Serializable]
	public class Wave
	{
		/// <summary>Caminho da cena do inimigo a ser spawned (ex: "res://scenes/enemies/Slime.tscn")</summary>
		public string EnemyScenePath;

		/// <summary>Quantidade de inimigos a spawnar nesta onda</summary>
		public int EnemyCount;

		/// <summary>Intervalo entre spawns em segundos</summary>
		public float SpawnInterval;

		/// <summary>Tempo de espera antes de começar a spawnar (em segundos)</summary>
		public float StartDelay;

		public Wave(string scenePath, int count, float interval, float delay = 0f)
		{
			EnemyScenePath = scenePath;
			EnemyCount = count;
			SpawnInterval = interval;
			StartDelay = delay;
		}
	}

	/// <summary>Lista de ondas pré-configuradas</summary>
	[Export] private PackedScene[] _waveDefinitions;

	/// <summary>Local onde os inimigos serão spawned (node parent dos inimigos)</summary>
	[Export] public NodePath EnemyContainerPath { get; set; } = "EnemyContainer";

	/// <summary>Se verdadeiro, ondas infinitas (dificuldade aumenta continuamente)</summary>
	[Export] public bool InfiniteWaves { get; set; } = false;

	/// <summary>Multiplicador de dificuldade a cada onda (1.1 = 10% mais inimigos)</summary>
	[Export] public float DifficultyMultiplier { get; set; } = 1.1f;

	/// <summary>Intervalo mínimo de spawn (limite inferior)</summary>
	[Export] public float MinSpawnInterval { get; set; } = 0.5f;

	// ---- ESTADO RUNTIME ----

	/// <summary>Número da onda atual (começando em 1)</summary>
	private int _currentWaveNumber = 0;

	/// <summary>Se verdadeiro, o gerenciador de ondas está ativo</summary>
	private bool _isActive = false;

	/// <summary>Número de inimigos ainda vivos da onda atual</summary>
	private int _enemiesAliveInWave = 0;

	/// <summary>Se verdadeiro, está aguardando antes de spawnar</summary>
	private bool _isWaitingForWave = false;

	/// <summary>Tempo restante para começar a onda</summary>
	private float _waveStartCountdown = 0f;

	/// <summary>Tempo restante para o próximo spawn</summary>
	private float _spawnCountdown = 0f;

	/// <summary>Quantos inimigos ainda precisam ser spawned nesta onda</summary>
	private int _enemiesToSpawnInWave = 0;

	/// <summary>Node que contém todos os inimigos</summary>
	private Node _enemyContainer;

	/// <summary>Lista das ondas salvas (se usar definições personalizadas)</summary>
	private List<Wave> _waves = new();

	public override void _Ready()
	{
		_enemyContainer = GetNodeOrNull(EnemyContainerPath);
		if (_enemyContainer == null)
		{
			GD.PushWarning("WaveManager: EnemyContainer não encontrado em " + EnemyContainerPath);
			SetProcess(false);
			return;
		}

		// Inscrever em eventos de morte de inimigos
		GameEvents.OnEnemyDied += HandleEnemyDied;

		// Configurar ondas padrão se não foram definidas
		if (_waveDefinitions == null || _waveDefinitions.Length == 0)
		{
			SetupDefaultWaves();
		}

		GD.Print("WaveManager inicializado!");
	}

	public override void _ExitTree()
	{
		GameEvents.OnEnemyDied -= HandleEnemyDied;
	}

	public override void _Process(double delta)
	{
		if (!_isActive)
			return;

		// Aguardando o tempo de início da onda
		if (_isWaitingForWave)
		{
			_waveStartCountdown -= (float)delta;
			if (_waveStartCountdown <= 0f)
			{
				_isWaitingForWave = false;
				GD.Print($"Iniciando onda {_currentWaveNumber}...");
			}
			return;
		}

		// Spawning de inimigos
		if (_enemiesToSpawnInWave > 0)
		{
			_spawnCountdown -= (float)delta;
			if (_spawnCountdown <= 0f)
			{
				SpawnEnemy();
				_enemiesToSpawnInWave--;
				_enemiesToSpawnInWave = Mathf.Max(0, _enemiesToSpawnInWave);
				_spawnCountdown = GetCurrentWave().SpawnInterval;
			}
		}
		// Onda terminou (todos inimigos spawned e derrotados)
		else if (_enemiesAliveInWave == 0 && _enemiesToSpawnInWave == 0)
		{
			FinishCurrentWave();
		}
	}

	/// <summary>Inicia o gerenciamento de ondas</summary>
	public void StartWaves()
	{
		_isActive = true;
		_currentWaveNumber = 0;
		StartNextWave();
	}

	/// <summary>Para o gerenciamento de ondas</summary>
	public void StopWaves()
	{
		_isActive = false;
		_currentWaveNumber = 0;
		_enemiesAliveInWave = 0;
		_enemiesToSpawnInWave = 0;
		GD.Print("Ondas interrompidas!");
	}

	/// <summary>Inicia a próxima onda</summary>
	private void StartNextWave()
	{
		_currentWaveNumber++;

		if (!InfiniteWaves && _currentWaveNumber > _waves.Count)
		{
			GD.Print("Todas as ondas foram completadas! Vitória!");
			GameManager.Instance?.EndGame(true);
			return;
		}

		GameManager.Instance?.SetCurrentWave(_currentWaveNumber);

		Wave currentWave = GetCurrentWave();
		_enemiesToSpawnInWave = currentWave.EnemyCount;
		_enemiesAliveInWave = currentWave.EnemyCount;

		// Aplicar multiplicador de dificuldade em ondas infinitas
		if (InfiniteWaves && _currentWaveNumber > _waves.Count)
		{
			_enemiesAliveInWave = Mathf.RoundToInt(_enemiesAliveInWave * Mathf.Pow(DifficultyMultiplier, _currentWaveNumber - _waves.Count));
			_enemiesToSpawnInWave = _enemiesAliveInWave;
		}

		_isWaitingForWave = true;
		_waveStartCountdown = currentWave.StartDelay;

		GD.Print($"Próxima onda em {currentWave.StartDelay} segundos. Inimigos: {_enemiesToSpawnInWave}");
	}

	/// <summary>Termina a onda atual</summary>
	private void FinishCurrentWave()
	{
		GD.Print($"Onda {_currentWaveNumber} concluída!");

		if (!InfiniteWaves && _currentWaveNumber >= _waves.Count)
		{
			GameManager.Instance?.EndGame(true);
			return;
		}

		StartNextWave();
	}

	/// <summary>Spawna um inimigo da onda atual</summary>
	private void SpawnEnemy()
	{
		Wave currentWave = GetCurrentWave();

		if (string.IsNullOrWhiteSpace(currentWave.EnemyScenePath))
		{
			GD.PushWarning($"WaveManager: Caminho de cena vazio para onda {_currentWaveNumber}");
			return;
		}

		var enemyScene = GD.Load<PackedScene>(currentWave.EnemyScenePath);
		if (enemyScene == null)
		{
			GD.PushWarning($"WaveManager: Não foi possível carregar {currentWave.EnemyScenePath}");
			return;
		}

		var enemyInstance = enemyScene.Instantiate<Node>();
		_enemyContainer.AddChild(enemyInstance);

		GD.Print($"Inimigo spawned! ({_enemiesToSpawnInWave} ainda para spawnar)");
	}

	/// <summary>Retorna a definição da onda atual</summary>
	private Wave GetCurrentWave()
	{
		int waveIndex = _currentWaveNumber - 1;

		// Se está em ondas infinitas, reutiliza a última onda
		if (InfiniteWaves && waveIndex >= _waves.Count)
		{
			return _waves[_waves.Count - 1];
		}

		if (waveIndex < 0 || waveIndex >= _waves.Count)
		{
			GD.PushWarning($"WaveManager: Onda {_currentWaveNumber} não existe!");
			return new Wave("res://scenes/enemies/Slime.tscn", 5, 1.0f);
		}

		return _waves[waveIndex];
	}

	/// <summary>Configura ondas padrão para testes</summary>
	private void SetupDefaultWaves()
	{
		_waves.Clear();

		// Onda 1: 5 Slimes, 1 segundo entre spawns
		_waves.Add(new Wave("res://scenes/enemies/Slime.tscn", 5, 1.0f, 2.0f));

		// Onda 2: 8 Slimes, 0.8 segundos entre spawns
		_waves.Add(new Wave("res://scenes/enemies/Slime.tscn", 8, 0.8f, 2.0f));

		// Onda 3: 10 Slimes, 0.7 segundos entre spawns
		_waves.Add(new Wave("res://scenes/enemies/Slime.tscn", 10, 0.7f, 2.0f));

		// Onda 4: 15 Slimes, 0.6 segundos entre spawns
		_waves.Add(new Wave("res://scenes/enemies/Slime.tscn", 15, 0.6f, 2.0f));

		// Onda 5: 20 Slimes, 0.5 segundos entre spawns
		_waves.Add(new Wave("res://scenes/enemies/Slime.tscn", 20, 0.5f, 2.0f));

		GD.Print("WaveManager: Usando ondas padrão para testes!");
	}

	/// <summary>Define ondas customizadas</summary>
	public void SetCustomWaves(List<Wave> waves)
	{
		_waves = new List<Wave>(waves);
		GD.Print($"WaveManager: {_waves.Count} ondas customizadas definidas!");
	}

	// ---- MANIPULADORES DE EVENTOS ----

	private void HandleEnemyDied(Enemy enemy)
	{
		if (_isActive && _enemiesAliveInWave > 0)
		{
			_enemiesAliveInWave--;
			GD.Print($"Inimigos restantes: {_enemiesAliveInWave}");
		}
	}

	// ---- MÉTODOS DE DEBUG ----

	/// <summary>Pula para a próxima onda (para testes)</summary>
	public void SkipToNextWave()
	{
		_enemiesToSpawnInWave = 0;
		_enemiesAliveInWave = 0;
		GD.Print("Pulando para próxima onda...");
	}

	/// <summary>Retorna informações sobre a onda atual</summary>
	public string GetWaveInfo()
	{
		if (_currentWaveNumber == 0)
			return "Nenhuma onda iniciada";

		Wave current = GetCurrentWave();
		return $"Onda {_currentWaveNumber}: {_enemiesAliveInWave} vivos, {_enemiesToSpawnInWave} para spawnar";
	}
}
