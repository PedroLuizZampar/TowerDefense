# Arquitetura do Tower Defense

## 📐 Diagrama de Camadas

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│                  (UI, HUD, Menu, Câmera)                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                         │
│                    (GameManager)                            │
│  • Controla fluxo do jogo                                   │
│  • Gerencia recursos do jogador                             │
│  • Responde a eventos                                       │
└─────────────────────────────────────────────────────────────┘
              ↓                            ↓
┌──────────────────────────┐  ┌──────────────────────────┐
│   WAVE MANAGEMENT        │  │   EVENT SYSTEM           │
│   (WaveManager)          │  │   (GameEvents)           │
│                          │  │                          │
│ • Spawn de inimigos     │  │ • OnEnemyDied           │
│ • Timing de ondas       │  │ • OnEnemyReachedEnd     │
│ • Dificuldade progressiva│  │ • OnEnemyDamageTaken    │
│ • Gerencia inimigos      │  │ • OnTowerShoot          │
└──────────────────────────┘  │ • OnGameStarted         │
              ↓               │ • OnGamePaused          │
┌─────────────────────────────────────────────────────────────┐
│                    ENTITY LAYER                              │
│                                                              │
│  ┌──────────────┐          ┌──────────────┐               │
│  │  Enemy       │          │  Tower       │               │
│  │  (Base)      │          │  (Base)      │               │
│  │              │          │              │               │
│  │ • Movimento  │          │ • Detecção   │               │
│  │ • Vida       │          │ • Targeting  │               │
│  │ • Dano       │          │ • Rotação    │               │
│  └──────────────┘          └──────────────┘               │
│       ↓                            ↓                        │
│  ┌──────────────┐          ┌──────────────┐               │
│  │  Slime       │          │BasicTower    │               │
│  │  (Concreto)  │          │  (Concreto)  │               │
│  └──────────────┘          └──────────────┘               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Dados

### Início do Jogo

```
UI Button (Iniciar)
        ↓
GameManager.StartGame()
        ↓
Emite: OnGameStarted
        ↓
WaveManager.StartWaves()
        ↓
WaveManager spawna inimigos
```

### Morte de Inimigo

```
Inimigo morre (vida <= 0)
        ↓
Enemy.Die()
        ↓
GameEvents.InvokeEnemyDied(enemy)
        ↓ (evento disparado para 2 listeners)
        ├─→ GameManager.HandleEnemyDied()
        │   ├─ AddMoney(50)
        │   └─ AddScore(100)
        │
        └─→ WaveManager.HandleEnemyDied()
            └─ Decrementa inimigos vivos
```

---

## 🏗️ Padrões de Arquitetura Utilizados

### 1. Singleton Pattern (GameManager)
- Garante única instância
- Acessível globalmente: `GameManager.Instance`

### 2. Observer Pattern (Event System)
- Desacoplamento entre sistemas
- `GameEvents` gerencia todos os subscribers

### 3. Template Method Pattern (Enemy/Tower)
- Lógica comum nas classes base
- Subclasses implementam métodos abstratos

### 4. Factory Pattern (WaveManager)
- Criação dinâmica de inimigos via caminhos

---

## 📊 Responsabilidades por Sistema

| Sistema | Responsabilidade | Dependências |
|---------|-----------------|--------------|
| **GameManager** | Fluxo do jogo, recursos | WaveManager, Events |
| **WaveManager** | Spawn, ondas, dificuldade | GameManager, Enemy |
| **Enemy** | Movimento, vida, comportamento | Events |
| **Tower** | Detecção, targeting, rotação | Enemy |
| **GameEvents** | Comunicação desacoplada | Nenhuma |

---

## 🌳 Hierarquia de Classes

```
Node (Godot)
├── GameManager (Singleton)
│
├── WaveManager (Gerenciador de ondas)
│
├── CharacterBody2D
│   └── Enemy (Base abstrata)
│       ├── Slime
│       ├── Orc (potencial)
│       └── Goblin (potencial)
│
└── StaticBody2D
    └── Tower (Base abstrata)
        ├── BasicTower
        ├── SnipperTower (potencial)
        └── FlameTower (potencial)
```

---

## 🔌 Pontos de Extensão

### Adicionar Novo Inimigo
Herde de `Enemy`, implemente:
- `EnemyName`, `MaxHealth`, `MoveSpeed`
- `UpdateWalkAnimation()`

### Adicionar Nova Torre
Herde de `Tower`, implemente:
- `DefaultRangeRadius`
- Opcionalmente: `AimAt()`, `OnEnemyEnteredRange()`

### Adicionar Novo Evento
Adicione em `GameEvents.cs`:
```csharp
public static event Action<ParamType> OnNovoEvento;
public static void InvokeNovoEvento(ParamType param)
{
    OnNovoEvento?.Invoke(param);
}
```

---

## ⚡ Performance

### Otimizações Implementadas
✅ Inimigos instanciados uma vez
✅ Eventos usam delegates (rápido)
✅ WaveManager usa delta time (não loops)
✅ Singleton evita lookups repetidos

### Possíveis Otimizações Futuras
- Object pooling para inimigos (reciclar objetos)
- Spatial hashing para detecção
- Culling de inimigos fora da câmera
- Cache de cálculos de distância

---

## 🔐 Segurança

### Validações Implementadas
✅ Verificação de valores negativos
✅ Proteção contra null references
✅ Warnings para configurações faltantes
✅ Health nunca fica negativo

---

**Última atualização:** 13 de fevereiro de 2026
