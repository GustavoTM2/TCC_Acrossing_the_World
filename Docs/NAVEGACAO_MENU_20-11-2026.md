# Navegação do menu e retorno das fases — 20/11/2026

## Menu principal
- **JOGAR**: abre a seleção de EGITO, BRASIL, ITÁLIA e JAPÃO; cada card abre a cena externa jogável correspondente. Os 8 callbacks existentes (4 opções + 4 países) foram preservados.
- **OPÇÕES**: permite ajustar o volume do jogo, alternar a tela cheia e mudar o nível de qualidade. As preferências são salvas no PlayerPrefs.
- **CRÉDITOS**: mostra a apresentação e créditos gerais do projeto com botão de retorno.
- **SAIR**: mostra confirmação. No Editor interrompe o Play Mode; no executável chama Application.Quit().
- **Voltar**, na escolha de países: retorna ao menu principal.
- **Teclado**: W/S e setas verticais navegam entre opções; A/D e setas horizontais navegam pelos países; Enter confirma; Esc retorna ou abre a confirmação de saída.

## Botões e ações para voltar
- **Egito**: botão "Voltar ao menu" disponível no exterior e no interior, inclusive durante a entrega do artefato. Transição com tela preta e retorno à cena do menu.
- **Brasil**: botão de menu no HUD e na tela do prêmio. Esc durante a exploração também retorna ao menu.
- **Itália**: botão de menu na rua e restaurante, durante conversa e resultado. Um botão separado agora fica visível durante contagem, preparo da pizza, forno, montagem da caixa, cortes e revisão. Esc também retorna ao menu.
- **Japão**: botão "Voltar ao menu" na cena externa, interior, diálogos e tela da conquista; Esc também retorna ao menu.
- **Progresso**: permanece na sessão ao retornar ao menu, permitindo que o cartão do país mostre "CONCLUÍDO".

## Arquivos alterados
- Assets/TCCMenu/Runtime/TravelMenu.cs
- Assets/TCCMenu/Runtime/TravelMenu.Overlay.cs (novo)
- Assets/TCCItalia/Scripts/ItaliaFase.cs
- Assets/TCCEgito/Runtime/EgyptStage.cs
- Assets/TCCEgito/Runtime/EgyptStageUI.cs

## Verificação técnica
- 4 destinos existem e estão habilitados no EditorBuildSettings.
- 8 botões de seleção e botão "Voltar" identificados na cena serializada.
- 15/15 verificações estáticas de navegação aprovadas (Docs/VALIDACAO_NAVEGACAO_20-11-2026.txt).
- Compilação runtime: 0 erros / 0 avisos.
- Compilação Editor: 0 erros (47 avisos de compatibilidade de APIs).
- **Pendente**: testes manuais de clique, retorno e aparência em Play Mode / executável. A compilação e os testes estáticos não garantem ausência de erros visuais em tempo de execução.

## Backup anterior
C:\Users\User\Documents\TCC_backup_menu_navegacao_2026-11-20
