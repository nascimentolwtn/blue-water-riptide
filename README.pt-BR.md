# Blue Water Riptide

[🇧🇷 Português](README.pt-BR.md) · [🇺🇸 English](README.md)

Um brawler de arena em equipe, com visão top-down, no estilo *Brawl Stars* — combate em equipe, ataque básico + Super carregável por personagem, condição de vitória por nocaute. A temática naval/marinha e de vôlei de praia é **apenas estética** (nomes de personagens, nomes de ataques, arte, ambientação da arena) — não há bola, não há zonas de pontuação, não há regras de esporte. As sementes de elenco e lore estão em [`.claude/plans/sailors.md`](.claude/plans/sailors.md); veja [`.claude/plans/00-game-design-overview.md`](.claude/plans/00-game-design-overview.md) para o design autoritativo.

Todo ano as frotas se reúnem em Tideline Cove para a **Tideline Cup** — um torneio amistoso e ferozmente competitivo em que três esquadrões (o disciplinado Blue Water Squad, o imprudente Riptide Rush e o inabalável Anchor Guard) comparecem para vencer. Veja [`History.md`](History.md) para o histórico completo do mundo, dos esquadrões e dos personagens.

## Engine e Plataformas-Alvo

- **Engine:** Unity (C#), Unity 2022 LTS ou mais recente.
- **Plataforma principal:** Android.
- **Plataforma futura:** iOS (as configurações de build e o input devem permanecer agnósticos de plataforma desde o início, para manter essa portabilidade barata mais adiante).

## Modos de Jogo (escopo inicial)

1. **Single Player** — escolha um time e jogue contra um time adversário controlado por IA, montado de forma aleatória.
2. **Local Network (LAN)** — co-op ou PvP no mesmo time com dispositivos próximos via Wi-Fi local. Alvo de rede: Unity Netcode for GameObjects + Unity Transport, com broadcast UDP para descoberta de host na LAN (sem necessidade de internet).
3. **Online (futuro)** — sistema de lobby para entrar no time de um amigo ou entrar sozinho na fila contra outros jogadores pela internet.

## Estrutura do Repositório

```
Assets/
  Scripts/
    Core/         # loop de jogo, estado de partida/sessão, pontuação e regras
    Gameplay/      # resolução de combate (projéteis, corpo a corpo/AOE, knockback), quadra/arena
    Characters/    # stats, habilidades e controllers dos Marinheiros
    Networking/    # setup do NGO, descoberta LAN, integração de sessão/lobby
    UI/            # menus, HUD, seleção de time
    AI/            # IA do oponente no modo single player
  Scenes/
  Prefabs/
    Characters/
    Projectiles/
    Environment/
  Art/
    Characters/
    Environment/
    UI/
  Audio/
    SFX/
    Music/
  Resources/
Packages/          # manifest do Unity Package Manager
ProjectSettings/    # populado na primeira abertura no Unity Hub/Editor
docs/
.claude/
  plans/            # planos de design e implementação (veja abaixo)
```

> Nota: `ProjectSettings/` e `Packages/packages-lock.json` normalmente são gerados/gerenciados pelo Unity Editor. Abra esta pasta como um projeto no Unity Hub para concluir a inicialização deles.

## Planos de Design e Implementação

Veja [`.claude/plans/`](.claude/plans/) para os planos da versão inicial:

- `00-game-design-overview.md` — conceito central, tradução do elenco a partir de Brawl Stars, regras de partida, sistemas de meta.
- `01-single-player-mode.md`
- `02-local-network-mode.md`
- `03-online-mode-future.md`
- `04-menu-navigation-lobby.md`
- `05-gamification-trophies-ranking.md`
- `06-splash-screen-and-icon.md`
