# Implementação de GameManager e WaveManager

## 📋 Resumo

Foram criados dois novos módulos cruciais para o jogo:

### 1. **GameManager.cs** (scripts/Core/)
- **Tipo**: Singleton Node
- **Responsabilidade**: Controlar fluxo do jogo (início, pausa, fim)
- **Recurso**: Gerenciar vida, dinheiro e pontuação do jogador
- **Eventos**: Responde a mortes de inimigos e inimigos chegando ao final

**Características principais:**
```csharp
// Acesso global
GameManager.Instance.StartGame();
GameManager.Instance.PauseGame();
GameManager.Instance.AddMoney(100);
GameManager.Instance.TakeDamage(10);
GameManager.Instance.TimeScale = 2.0f; // Câmera lenta
```

**Estados possíveis:**
- `MainMenu` - Menu principal
- `Playing` - Jogo em andamento
- `Paused` - Jogo pausado
- `GameOver` - Jogo terminado

---

### 2. **WaveManager.cs** (scripts/Core/)
- **Tipo**: Node especializado
- **Responsabilidade**: Gerenciar ondas de inimigos
- **Recurso**: Spawn automático com timing, dificuldade progressiva
- **Suporte**: Ondas finitas ou infinitas

**Características principais:**
```csharp
// Usar ondas padrão
waveManager.StartWaves();

// Ou definir customizadas
var waves = new List<WaveManager.Wave>
{
    new("res://scenes/enemies/Slime.tscn", 5, 1.0f, 2.0f),
    new("res://scenes/enemies/Orc.tscn", 8, 0.8f, 2.0f),
};
waveManager.SetCustomWaves(waves);
waveManager.StartWaves();

// Ondas infinitas
waveManager.InfiniteWaves = true;
waveManager.DifficultyMultiplier = 1.15f; // 15% mais inimigos
```

---

## 🔧 Como Implementar na Cena

### Passo 1: Estrutura da Cena

Na sua cena principal (ex: `Screen1.tscn`), adicione:

```
Screen1 (Node2D)
├── GameManager (Node) ← Adicionar script GameManager.cs
│   └── WaveManager (Node) ← Adicionar script WaveManager.cs
│       └── EnemyContainer (Node) ← Será usado para conter inimigos
├── TileMap (seu mapa)
├── PathUp (Path2D) ← Caminho para inimigos
├── PathDown (Path2D) ← Mais um caminho opcional
└── UI (seu HUD)
```

### Passo 2: Configurar GameManager

1. Na cena, selecione o nó `GameManager`
2. No inspetor, atribua o script `scripts/Core/GameManager.cs`
3. Não há configurações necessárias (usa padrões)

### Passo 3: Configurar WaveManager

1. Na cena, selecione o nó `WaveManager` (filho de GameManager)
2. No inspetor, atribua o script `scripts/Core/WaveManager.cs`
3. Configure as exportações:
   - **Enemy Container Path**: `EnemyContainer` (filho do WaveManager)
   - **Infinite Waves**: `false` (para uso normal)
   - **Difficulty Multiplier**: `1.1` (10% a mais a cada onda)

### Passo 4: Adicionar Botão para Iniciar

```csharp
public partial class MainMenuButton : Button
{
    public override void _Pressed()
    {
        GameManager.Instance.StartGame();
    }
}
```

---

## 🎮 Fluxo de Funcionamento

```mermaid
[Início do Jogo]
        ↓
[GameManager.StartGame()]
        ↓
[Emite OnGameStarted]
        ↓
[WaveManager.StartWaves()]
        ↓
[Aguarda StartDelay]
        ↓
[Spawna inimigos com SpawnInterval]
        ↓
[Inimigos se movem, jogador defende]
        ↓
[Inimigos morrem → OnEnemyDied disparado]
        ↓
[GameManager recebe evento, adiciona dinheiro/pontos]
        ↓
[Próxima onda quando todos inimigos morrem]
        ↓
[Todas as ondas completas → Vitória!]
        ↓
[GameManager.EndGame(true)]
```

---

## 📊 Fluxo de Eventos

### Quando inimigo morre:
```
Enemy.Die()
  ↓
GameEvents.InvokeEnemyDied(enemy)
  ↓
GameManager.HandleEnemyDied()
  ├─ AddMoney(50)
  ├─ AddScore(100)
  └─ Emite log
  ↓
WaveManager.HandleEnemyDied()
  └─ Decrementa _enemiesAliveInWave
```

### Quando inimigo chega ao final:
```
Enemy.OnReachedEnd()
  ↓
GameEvents.InvokeEnemyReachedEnd(enemy)
  ↓
GameManager.HandleEnemyReachedEnd()
  ├─ TakeDamage(10)
  └─ Se vida <= 0, EndGame(false)
```

---

## 🧪 Testando

### Teste 1: Iniciar Jogo
```csharp
// No _Process ou ao pressionar um botão
if (Input.IsActionJustPressed("ui_accept"))
{
    GameManager.Instance.StartGame();
}
```

### Teste 2: Ver Ondas
```csharp
// Abra o console e veja os logs:
// "Próxima onda em 2 segundos. Inimigos: 5"
// "Iniciando onda 1..."
// "Inimigo spawned! (4 ainda para spawnar)"
```

### Teste 3: Pausar/Retomar
```csharp
override void _Input(InputEvent @event)
{
    if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Space)
    {
        GameManager.Instance.TogglePause();
    }
}
```

### Teste 4: Pular Onda (Debug)
```csharp
if (Input.IsActionJustPressed("ui_focus_next")) // Tab
{
    GetNode<WaveManager>("GameManager/WaveManager").SkipToNextWave();
}
```

---

## 🚨 Possíveis Problemas

### Problema: "WaveManager: EnemyContainer não encontrado"
**Solução:** Certifique-se que existe um Node nomeado "EnemyContainer" como filho direto de WaveManager.

### Problema: Inimigos não spawnam
**Solução:**
- Verifique se o caminho `EnemyContainerPath` está correto
- Verifique se as cenas de inimigos existem em `res://scenes/enemies/`
- Verifique se `Enemy.cs` está no script do inimigo

### Problema: GameManager é null
**Solução:**
- Certifique-se que GameManager está na cena e com o script atribuído
- GameManager é criado apenas uma vez, na primeira execução

---

## 📈 Próximos Passos

1. **Criar cenas de inimigos** - Salvar Slime como `res://scenes/enemies/Slime.tscn`

2. **Criar sistema HUD** - Mostrar vida, dinheiro, onda atual:
   ```csharp
   public partial class HUD : Control
   {
       public override void _Process(double delta)
       {
           GetNode<Label>("HealthLabel").Text = $"HP: {GameManager.Instance.PlayerHealth}";
           GetNode<Label>("MoneyLabel").Text = $"${GameManager.Instance.PlayerMoney}";
       }
   }
   ```

3. **Implementar construção de torres** - Usar `GameManager.TrySpendMoney()`

4. **Adicionar mais tipos de inimigos** - Herdar de `Enemy.cs`

5. **Implementar sistema de efeitos** - Usar `GameEvents` para disparar efeitos ao destruir inimigos

---

## 📝 Referência Rápida

### GameManager - Métodos Principais

| Método | Descrição |
|--------|-----------|
| `StartGame()` | Inicia o jogo |
| `PauseGame()` | Pausa o jogo |
| `ResumeGame()` | Retoma o jogo |
| `TogglePause()` | Alterna pausa/jogo |
| `EndGame(bool)` | Termina o jogo (vitória/derrota) |
| `AddMoney(int)` | Adiciona dinheiro |
| `TrySpendMoney(int)` | Tenta gastar dinheiro (retorna bool) |
| `AddScore(int)` | Adiciona pontuação |
| `TakeDamage(int)` | Remove vida do jogador |
| `SetCurrentWave(int)` | Define número da onda |

### WaveManager - Métodos Principais

| Método | Descrição |
|--------|-----------|
| `StartWaves()` | Começa o sistema de ondas |
| `StopWaves()` | Para o sistema de ondas |
| `SetCustomWaves(List)` | Define ondas customizadas |
| `SkipToNextWave()` | Pula para próxima onda |
| `GetWaveInfo()` | Retorna info da onda atual |

---

**Arquivos Criados:**
- ✅ `scripts/Core/GameManager.cs` - Gerenciador central
- ✅ `scripts/Core/WaveManager.cs` - Gerenciador de ondas
- ✅ Documentação completa com exemplos

**Status:** Pronto para uso!
