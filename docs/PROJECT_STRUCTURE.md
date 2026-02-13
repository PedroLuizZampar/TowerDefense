# Estrutura do Projeto Tower Defense

## Visão Geral

Este projeto está organizado de forma modular e escalável para facilitar o desenvolvimento de um jogo Tower Defense em Godot com C#.

## 📁 Estrutura de Pastas

```
TowerDefense/
├── assets/                          # Todos os recursos do jogo
│   ├── sprites/                     # Gráficos 2D
│   │   ├── towers/                  # Sprites de torres
│   │   ├── enemies/                 # Sprites de inimigos
│   │   ├── ui/                      # Elementos de interface
│   │   └── effects/                 # Efeitos visuais (explosões, etc)
│   ├── sounds/                      # Áudio
│   │   ├── sfx/                     # Efeitos sonoros
│   │   └── music/                   # Músicas de fundo
│   ├── tilesets/                    # Tilesets e mapas
│   └── fonts/                       # Fontes customizadas
│
├── scenes/                          # Cenas Godot (.tscn)
│   ├── levels/                      # Cenas de níveis
│   ├── towers/                      # Cenas de torres
│   ├── enemies/                     # Cenas de inimigos
│   ├── ui/                          # Cenas de interface
│   └── effects/                     # Cenas de efeitos
│
├── scripts/                         # Código C#
│   ├── Core/                        # Lógica principal do jogo
│   │   ├── GameManager.cs           # Gerenciador geral do jogo
│   │   ├── WaveManager.cs           # Gerenciador de ondas de inimigos
│   │   └── GameState.cs             # Estado do jogo
│   │
│   ├── Towers/                      # Lógica de torres
│   │   ├── Tower.cs                 # Classe base para torres
│   │   └── BasicTower.cs            # Implementação de torre básica
│   │
│   ├── Enemies/                     # Lógica de inimigos
│   │   ├── Enemy.cs                 # Classe base para inimigos
│   │   └── Slime.cs                 # Implementação de Slime
│   │
│   ├── AI/                          # Inteligência artificial
│   │   └── PathManager.cs           # Gerenciador de paths dos inimigos
│   │
│   ├── UI/                          # Interface do usuário
│   │   ├── HUD.cs                   # Interface in-game
│   │   └── PauseMenu.cs             # Menu de pausa
│   │
│   └── Utilities/                   # Utilitários reutilizáveis
│       ├── Constants.cs             # Constantes globais
│       └── EventSystem.cs           # Sistema centralizado de eventos
│
└── project.godot                    # Configuração do projeto Godot
```

## 🎯 Módulos Principais

### Scripts/Core
Contém a lógica principal de gerenciamento do jogo.

**Arquivos esperados:**
- `GameManager.cs` - Controla o fluxo geral (início, pausa, fim)
- `WaveManager.cs` - Gerencia ondas de inimigos
- `GameState.cs` - Estado global do jogo (dinheiro, vida, pontuação)

### Scripts/Towers
Contém toda a lógica de torres.

**Estrutura:**
- `Tower.cs` - classe base abstrata com:
  - Detecção de inimigos (Area2D)
  - Sistema de targeting
  - Rotação do canhão

- Criar novas torres herdando de `Tower.cs`:
  ```csharp
  public partial class SnipperTower : Tower
  {
      protected override float DefaultRangeRadius => 400f;
  }
  ```

### Scripts/Enemies
Contém toda a lógica de inimigos.

**Estrutura:**
- `Enemy.cs` - classe base abstrata com:
  - Movimento em paths
  - Sistema de vida/dano
  - Animações

- Para criar novos inimigos, herde de `Enemy.cs`:
  ```csharp
  public partial class Orc : Enemy
  {
      protected override string EnemyName => "Orc";
      protected override int MaxHealth => 20;
      protected override float MoveSpeed => 100f;

      protected override void UpdateWalkAnimation(Vector2 movement)
      {
          // Implementar animações específicas do Orc
      }
  }
  ```

### Scripts/Utilities
Utilitários compartilhados entre todos os módulos.

**GameConstants.cs** - Define valores usados no jogo:
- Mascaras de colisão
- Velocidades min/max
- Valores de vida
- Raios de torres
- Nomes de paths padrão

**EventSystem.cs** - Sistema centralizado de eventos:
```csharp
// Inscrever em eventos
GameEvents.OnEnemyDied += HandleEnemyDeath;

// Disparar eventos
GameEvents.InvokeEnemyDied(enemy);
```

## 🔗 Sistema de Eventos

O projeto usa um sistema centralizado de eventos para desacoplamento:

**Eventos de Inimigos:**
- `OnEnemyDied` - Inimigo foi eliminado
- `OnEnemyReachedEnd` - Inimigo alcançou o final do caminho
- `OnEnemyDamageTaken` - Inimigo recebeu dano

**Eventos de Torres:**
- `OnTowerShoot` - Torre atirou em um inimigo
- `OnTowerTargetChanged` - Torre mudou de alvo

**Eventos de Jogo:**
- `OnGameStarted` - Jogo iniciou
- `OnGamePaused` - Jogo foi pausado
- `OnGameResumed` - Jogo foi retomado

## 📝 Como Adicionar Novos Elementos

### Adicionar Nova Torre

1. Crie um novo arquivo em `scripts/Towers/MeuTower.cs`:
```csharp
public partial class MeuTower : Tower
{
    protected override float DefaultRangeRadius => 300f;

    // Opcionalmente, sobrescreva métodos como:
    // - AimAt() para lógica de mira customizada
    // - OnEnemyEnteredRange() para comportamento customizado
}
```

2. Crie a cena em `scenes/towers/MeuTower.tscn`

3. Configure os nós filhos:
   - `RangeArea` (Area2D) com `CollisionShape2D`
   - `Cannon` (Node2D) - elemento que rotaciona

### Adicionar Novo Inimigo

1. Crie um novo arquivo em `scripts/Enemies/MeuInimigo.cs`:
```csharp
public partial class MeuInimigo : Enemy
{
    protected override string EnemyName => "Meu Inimigo";
    protected override int MaxHealth => 15;
    protected override float MoveSpeed => 150f;

    protected override void UpdateWalkAnimation(Vector2 movement)
    {
        // Implementar animações
    }

    // Opcionalmente:
    protected override void OnSpawned()
    {
        // Lógica quando spawn
    }

    protected override void OnDied()
    {
        // Efeitos especiais ao morrer
    }
}
```

2. Crie a cena em `scenes/enemies/MeuInimigo.tscn`

3. Configure os nós filhos:
   - Sprite2D ou AnimatedSprite2D
   - CollisionShape2D para física

### Adicionar Nova Torre com Comportamento Especial

Se precisar de comportamento diferente (ex: torres que atiram múltiplos disparos):

1. Sobrescreva `_Process`:
```csharp
public override void _Process(double delta)
{
    base._Process(delta);

    if (CurrentTarget != null)
    {
        Shoot(CurrentTarget);
    }
}

private void Shoot(Enemy target)
{
    // Implementar lógica de disparo
}
```

## 🔧 Usando o Sistema de Eventos

**Exemplo - Ganhar dinheiro quando inimigo morre:**

```csharp
public partial class GameState : Node
{
    public override void _Ready()
    {
        GameEvents.OnEnemyDied += OnEnemyDied;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        playerMoney += 100; // +100 moedas por inimigo morto
    }

    public override void _ExitTree()
    {
        GameEvents.OnEnemyDied -= OnEnemyDied;
    }
}
```

**Exemplo - Criar efeito quando torre atira:**

```csharp
public partial class EffectManager : Node
{
    public override void _Ready()
    {
        GameEvents.OnTowerShoot += OnTowerShoot;
    }

    private void OnTowerShoot(Tower tower, Enemy target)
    {
        SpawnMuzzleFlash(tower.GlobalPosition);
    }
}
```

## ✅ Checklist para Novo Conteúdo

- [ ] Script criado em pasta apropriada
- [ ] Cena criada em `scenes/`
- [ ] Classe herda da classe base correta
- [ ] Nós filhos configurados (sprites, colliders, etc)
- [ ] Constantes adicionadas a `GameConstants.cs` se necessário
- [ ] Eventos disparados em `GameEvents` se necessário
- [ ] Documentação em comentários /// (XML docs)

## 📖 Padrões Utilizados

### Padrão Template Method (Classes Base)
As classes `Enemy` e `Tower` definem o fluxo geral, permitindo que subclasses implementem partes específicas.

### Padrão Observer (Event System)
Desacoplamento entre sistemas usando eventos centralizados em `GameEvents`.

### Padrão Singleton (Constants)
`GameConstants` fornece um ponto único de acesso a constantes globais.

---

**Última atualização:** 13 de fevereiro de 2026
