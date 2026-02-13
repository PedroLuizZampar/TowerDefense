# Guia de Uso: GameManager e WaveManager

## GameManager

### O que é?

`GameManager` é um Singleton que controla o fluxo geral do jogo. É o ponto central responsável por:

- Começar, pausar e retomar o jogo
- Gerenciar vida do jogador
- Gerenciar dinheiro do jogador
- Gerenciar pontuação
- Controlar velocidade do jogo (time scale)
- Responder a eventos de inimigos

### Como usar?

#### Acessar a instância

```csharp
GameManager manager = GameManager.Instance;

// Verificar estado do jogo
if (manager.CurrentGameState == GameState.Playing)
{
    // Jogo está rodando
}
```

#### Controlar o fluxo do jogo

```csharp
// Iniciar o jogo
GameManager.Instance.StartGame();

// Pausar o jogo
GameManager.Instance.PauseGame();

// Retomar o jogo
GameManager.Instance.ResumeGame();

// Alternar pausa (útil para menu de pausa)
GameManager.Instance.TogglePause();

// Terminar o jogo (vitória ou derrota)
GameManager.Instance.EndGame(victory: true);  // Vitória
GameManager.Instance.EndGame(victory: false); // Derrota
```

#### Gerenciar recursos do jogador

```csharp
// Adicionar dinheiro
GameManager.Instance.AddMoney(100); // +100 moedas

// Tentar gastar dinheiro (retorna true/false se bem-sucedido)
if (GameManager.Instance.TrySpendMoney(500))
{
    // Construiu uma torre
}

// Ver dinheiro do jogador
int currentMoney = GameManager.Instance.PlayerMoney;

// Adicionar pontuação
GameManager.Instance.AddScore(250);

// Ver pontuação
int score = GameManager.Instance.PlayerScore;
```

#### Gerenciar saúde do jogador

```csharp
// O jogador recebe dano
GameManager.Instance.TakeDamage(10);

// Ver vida do jogador
int health = GameManager.Instance.PlayerHealth;
```

#### Controlar velocidade do tempo

```csharp
// Velocidade normal
GameManager.Instance.TimeScale = 1.0f;

// Meia velocidade (câmera lenta)
GameManager.Instance.TimeScale = 0.5f;

// Dobro da velocidade
GameManager.Instance.TimeScale = 2.0f;

// Congelado
GameManager.Instance.TimeScale = 0f;
```

### Exemplo Completo: Menu de Pausa

```csharp
public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        // Quando o menu abre, pausa o jogo
        GameManager.Instance.PauseGame();
    }

    public void OnResumeButtonPressed()
    {
        // Quando o botão "Continuar" é pressionado
        GameManager.Instance.ResumeGame();
        QueueFree(); // Fechar menu
    }

    public override void _Input(InputEvent @event)
    {
        // Pressionar ESC para pausar/retomar
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            GameManager.Instance.TogglePause();
            GetTree().Root.SetInputAsHandled();
        }
    }
}
```

---

## WaveManager

### O que é?

`WaveManager` gerencia as ondas de inimigos. É responsável por:

- Controlar o timing de spawn de inimigos
- Definir quantos inimigos existem em cada onda
- Aumentar a dificuldade progressivamente
- Suportar ondas finitas ou infinitas
- Emitir eventos quando as ondas terminam

### Setup na Cena

1. Crie um Node chamado `WaveManager` (script `WaveManager.cs`)
2. Como filho do WaveManager, crie um Node chamado `EnemyContainer`
3. O `EnemyContainer` será o pai de todos os inimigos spawned

```
Cena
├── GameManager
│   └── WaveManager
│       └── EnemyContainer
└── (suas outras nodes)
```

### Como usar?

#### Estrutura de uma onda

Uma onda é definida por 4 parâmetros:

```csharp
new WaveManager.Wave(
    scenePath: "res://scenes/enemies/Slime.tscn",  // Caminho para a cena do inimigo
    count: 10,                                       // 10 inimigos nesta onda
    interval: 0.5f,                                  // 0.5 segundos entre spawns
    delay: 2.0f                                      // Esperar 2 segundos antes de começar
);
```

#### Usar ondas padrão (para testes)

```csharp
WaveManager waveManager = GetNode<WaveManager>("WaveManager");
waveManager.StartWaves(); // Começa com ondas padrão pré-configuradas
```

As ondas padrão são:
- Onda 1: 5 Slimes
- Onda 2: 8 Slimes
- Onda 3: 10 Slimes
- Onda 4: 15 Slimes
- Onda 5: 20 Slimes

#### Definir ondas customizadas

```csharp
var waves = new List<WaveManager.Wave>
{
    new("res://scenes/enemies/Slime.tscn", 5, 1.0f, 2.0f),
    new("res://scenes/enemies/Slime.tscn", 8, 0.8f, 2.0f),
    new("res://scenes/enemies/Orc.tscn", 10, 1.0f, 2.0f),
    new("res://scenes/enemies/Slime.tscn", 15, 0.7f, 2.0f),
};

WaveManager waveManager = GetNode<WaveManager>("WaveManager");
waveManager.SetCustomWaves(waves);
waveManager.StartWaves();
```

#### Ondas infinitas

```csharp
WaveManager waveManager = GetNode<WaveManager>("WaveManager");
waveManager.InfiniteWaves = true;                    // Ativar ondas infinitas
waveManager.DifficultyMultiplier = 1.15f;            // 15% mais inimigos a cada onda
waveManager.StartWaves();
```

#### Controlar ondas

```csharp
WaveManager waveManager = GetNode<WaveManager>("WaveManager");

// Pular para próxima onda (para testes/debug)
waveManager.SkipToNextWave();

// Parar todas as ondas
waveManager.StopWaves();

// Ver informações da onda atual
string info = waveManager.GetWaveInfo();
GD.Print(info); // "Onda 3: 5 vivos, 3 para spawnar"
```

### Exemplo Completo: Gerenciador de Campanha

```csharp
public partial class CampaignManager : Node
{
    private WaveManager _waveManager;

    public override void _Ready()
    {
        _waveManager = GetNode<WaveManager>("WaveManager");

        // Configurar ondas da campanha
        var campaignWaves = new List<WaveManager.Wave>
        {
            new("res://scenes/enemies/Slime.tscn", 3, 1.5f, 2.0f),
            new("res://scenes/enemies/Slime.tscn", 5, 1.0f, 2.0f),
            new("res://scenes/enemies/Orc.tscn", 4, 1.0f, 2.0f),
            new("res://scenes/enemies/Slime.tscn", 8, 0.8f, 2.0f),
            new("res://scenes/enemies/Orc.tscn", 6, 0.9f, 2.0f),
        };

        _waveManager.SetCustomWaves(campaignWaves);

        // Inscrever em eventos
        GameEvents.OnGameStarted += StartCampaign;
    }

    private void StartCampaign()
    {
        GD.Print("Campanha iniciada com 5 ondas!");
        _waveManager.StartWaves();
    }

    public override void _Input(InputEvent @event)
    {
        // Tecla W para pular onda (debug)
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.W)
        {
            _waveManager.SkipToNextWave();
            GetTree().Root.SetInputAsHandled();
        }
    }
}
```

---

## Integração: GameManager + WaveManager

### Fluxo de Jogo Típico

```
1. Programa nota que algo quer iniciar o jogo
   └─> GameManager.Instance.StartGame()

2. GameManager inicia o WaveManager
   └─> WaveManager.StartWaves()

3. WaveManager inicia primeira onda
   └─> Spawna inimigos em intervalos

4. Inimigos se movem, jogador constrói torres
   └─> Eventos são disparados (dano, morte, etc)
   └─> GameManager responde aos eventos

5. Onda termina (todos inimigos derrotados)
   └─> WaveManager inicia próxima onda
   └─> Vai para passo 3

6. Última onda terminada
   └─> GameManager.Instance.EndGame(true)
   └─> Jogo entra em GameState.GameOver

7. Se vida do jogador chegar a 0
   └─> GameManager.Instance.EndGame(false)
   └─> Jogo terminado (derrota)
```

### Responsabilidades

| Sistema | Responsabilidade |
|---------|-----------------|
| **GameManager** | Estado geral, recursos do jogador, fluxo de jogo, responder eventos |
| **WaveManager** | Spawn de inimigos, timing de ondas, dificuldade progressiva |
| **Enemy/Tower** | Comportamento individual, disparar eventos |
| **GameEvents** | Comunicação desacoplada entre sistemas |

---

## Configuração Recomendada na Cena

```gdscript
# Na cena principal (ex: Level.tscn):

Level (Node)
├── GameManager (Node)
│   ├── WaveManager (Node)
│   │   └── EnemyContainer (Node)
│   └── (adicionar listeners de eventos se necessário)
├── TowerLayer (CanvasLayer)
│   └── (torres do jogador)
├── TileMap (TileMap)
│   └── (mapa do jogo)
├── PathUp (Path2D)
│   └── PathFollow2D
├── PathDown (Path2D)
│   └── PathFollow2D
└── HUD (CanvasLayer)
    └── (UI do jogo)
```

---

## Debugging

### Logs do GameManager

```
GameManager inicializado com sucesso!
Jogo iniciado!
Dinheiro adicionado: +50. Total: 550
Pontuação adicionada: +100. Total: 100
Inimigo Slime recebeu 5 de dano. Vida: 5
Inimigo Slime morreu!
Jogo pausado!
Jogo retomado!
```

### Logs do WaveManager

```
WaveManager inicializado!
WaveManager: Usando ondas padrão para testes!
Próxima onda em 2 segundos. Inimigos: 5
Iniciando onda 1...
Inimigo spawned! (4 ainda para spawnar)
Inimigos restantes: 4
Onda 1 concluída!
Próxima onda em 2 segundos. Inimigos: 8
```

---

**Última atualização:** 13 de fevereiro de 2026
