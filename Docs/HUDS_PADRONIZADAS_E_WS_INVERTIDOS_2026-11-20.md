# HUDs padronizadas e movimentação W/S invertida

**Data:** 20/11/2026

## Projeto editado
`C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main`

## HUD das instruções

Novo módulo reutilizável `Assets/TCCAudio/Runtime/StageControlPanel.cs`.

- Fundo escuro, traço e barra vertical na cor do país, título **CONTROLES** centralizado.
- Coluna de teclas com alinhamento à direita e coluna de funções com alinhamento à esquerda; separador vertical e linha abaixo do cabeçalho.
- Altura adaptada ao número de instruções: 4 linhas para Brasil, Japão e Itália; 5 para Egito (com espaço/pular).
- O módulo respeita a matriz de escala de 1280x720 (Egito/Brasil/Japão) e 1000x600 (Itália), que as HUDs já configuram.
- O painel fica no canto inferior esquerdo e não cruza o aviso de interação no inferior central.
- Removido o `controls.Replace("     ", "\n")` que quebrava linhas incorretamente.
- Cores: Egito areia/dourado, Brasil verde, Japão vermelho/rosa, Itália terracota.
- A caixa de interação no Brasil também recebeu o verde do país.
- A HUD legada `BrazilExplorationHUD` não desenha por cima da HUD oficial quando estiver na cena de um país.
- **Esc · Menu da fase** permanece dentro do painel de controles; o botão Voltar ao Menu Principal permanece exclusivamente no painel de pausa, sem ficar solto na tela.

## Teclas W / S

| Fase | W | S | Setas |
|---|---|---|---|
| Egito externo/interno | frente/baixo | fundo/cima | ↑ cima, ↓ baixo |
| Brasil inteiro | frente/baixo | fundo/cima | ↑ cima, ↓ baixo |
| Japão externo/interno | frente/baixo | fundo/cima | ↑ cima, ↓ baixo |
| Itália restaurante/interior | frente/baixo | fundo/cima | ↑ cima, ↓ baixo |
| Itália rua/exterior | fundo/cima | frente/baixo | ↑ cima, ↓ baixo |

A/D não mudaram. E/Enter continuam interagindo. A inversão também vale no interior do Japão porque Kenji utiliza o mesmo `BrazilPlayer25D` no exterior e no interior.

### Implementação técnica
- `Assets/TCCEgito/Runtime/EgyptInput.cs`: nova propriedade `BrazilDepth` com o sentido de `EgyptDepth`; a propriedade `Depth` convencional foi mantida para compatibilidade.
- `Assets/TCCBrasil/Runtime/BrazilPlayer25D.cs`: usa `EgyptInput.BrazilDepth`, atendendo Lucas no Brasil e Kenji nas duas cenas do Japão.
- `Assets/TCCItalia/Scripts/ItaliaFase.cs`: inverte a leitura de W/S apenas quando `interior == true`, tanto para Input System moderno quanto para o Input Manager legado, mantendo as setas inalteradas.
- `Assets/TCCEgito/Runtime/EgyptPlayer25D.cs`: Egito segue usando `EgyptDepth`, que já havia sido invertido.

## Arquivos modificados

- `Assets/TCCAudio/Runtime/StageControlPanel.cs` (criado)
- `Assets/TCCAudio/Runtime/StageControlPanel.cs.meta` (criado com GUID próprio)
- `Assets/TCCEgito/Runtime/EgyptStageUI.cs`
- `Assets/TCCEgito/Runtime/EgyptInput.cs`
- `Assets/TCCBrasil/Runtime/BrazilStageUI.cs`
- `Assets/TCCBrasil/Runtime/BrazilExplorationHUD.cs`
- `Assets/TCCBrasil/Runtime/BrazilPlayer25D.cs`
- `Assets/TCCJapao/Runtime/JapanQuest.cs`
- `Assets/TCCItalia/Scripts/ItaliaFase.cs`

**Nenhuma cena, modelo 3D, missão, conquista ou porta foi removida.**

## Validação
- 29/29 verificações estáticas da UI, cores, controles, vínculos entre scripts e referências das duas cenas japonesas.
- Compilação C# runtime: **0 erros, 0 avisos**.
- Compilação C# Editor: **0 erros** e 51 avisos de APIs obsoletas já existentes.
- O Editor da Unity detectou mudanças e concluiu um ciclo de refresh dos assets sem novas mensagens de erro C# no trecho recente do log.
- **Pendente:** percorrer as quatro fases em Play Mode e confirmar visualmente o alinhamento e o comportamento dos controles. A verificação estática e a compilação não substituem esse teste.

## Backup
`C:\Users\User\Documents\TCC_BACKUP_HUD_WS_2026-11-20`

## Relatório de auditoria
`Docs/AUDITORIA_HUD_PADRONIZADA_WS_2026-11-20.txt`

## Teste rápido
1. Pare o Play na Unity para recompilar os scripts. Se a cena tiver alterações locais ainda não salvas, não as salve por cima dos arquivos atualizados sem revisar.
2. Abra `TCCJapao/Scenes/JAPAO_EXTERNO.unity`, depois `JAPAO_INTERIOR.unity`. Confirme o painel inferior com `S / W · Cima / baixo` e que **S** anda para o fundo, **W** para a frente.
3. Repita no Brasil, inclusive após voltar do minigame de embaixadinhas.
4. Na Itália, confira W/S invertidos dentro da pizzaria, mas normais na rua.
5. No Egito, confira cinco linhas de controles e o botão Esc abrindo o menu, sem outros botões soltos de voltar ao menu.
