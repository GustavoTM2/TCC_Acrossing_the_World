# HUDs das fases e duas câmeras do Japão — revisão final

Data: 20/11/2026

## Projeto alterado
`C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main`

## Voltar ao Menu
- Confirmado que não há mais botões `Voltar ao Menu` desenhados individualmente pelas HUDs do Egito, Brasil, Itália e Japão. A busca por chamadas dos botões de retorno localizou apenas o botão `VOLTAR AO MENU PRINCIPAL` dentro de `Assets/TCCAudio/Runtime/StageExperience.cs`.
- A tecla `Esc` abre o menu de pausa nas quatro fases, mantendo opções para **Continuar Jogando, Volume, Luminosidade e Voltar ao Menu Principal**.
- Os botões `Voltar à praia`, `Tentar novamente` e `Continuar explorando` não foram removidos, porque são controles internos das missões e não levam ao menu principal.

## HUDs reorganizadas
- **Egito**: instruções verticais em cinco linhas num quadro compacto inferior esquerdo; prompt de E/Enter isolado na parte inferior central.
- **Brasil**: controles verticais com mesma organização; o painel do berimbau fica oculto enquanto o placar das embaixadinhas é mostrado, removendo a sobreposição no canto superior direito.
- **Itália**: a indicação global `Esc · Menu da Fase` é ocultada apenas durante as telas da pizzaria, pois interferia no cronômetro e no pedido, mas Esc continua abrindo o menu normalmente. A HUD da cidade e restaurante mantém os mesmos blocos de título, controles, diálogo e prêmio.
- **Japão**: a caixa de controles foi transformada em painel compacto (x24, y585, 336x116), igual ao Egito e Brasil. O aviso de interação ficou em (x385, y608, 620x65), sem cruzar com controles. A caixa do prêmio Katana Okoizume foi para o canto superior direito (x944, y24, 312x124), no alinhamento dos artefatos das outras fases. As caixas de diálogo e conquista permanecem centralizadas.

## Japão externo — enquadramento
Script: `Assets/TCCJapao/Runtime/JapanExteriorCamera25D.cs`

- Campo de visão vertical ampliado de 44 para **49 graus**.
- Ângulo da câmera ajustado de 68 para **64 graus**, mostrando mais a frente do personagem e o cenário.
- Altura 6,5 unidades e distância atrás de Kenji 4 unidades.
- Seguimento suavizado com `SmoothDamp` em aproximadamente 0,20 s; segue os movimentos em X/Z.
- Proteção contra paredes e limitação às quatro barreiras invisíveis mantidas para minimizar vazamentos fora do mapa.
- Posição inicial da câmera na cena atualizada, de acordo com a nova configuração.
- Referência ao alvo Kenji e aos limites serializados preservada.

## Japão interno — nova câmera
Arquivo novo: `Assets/TCCJapao/Runtime/JapanInteriorCamera25D.cs`

- Componente anexado diretamente ao objeto `Camera_Japao_Interior` da cena `Assets/TCCJapao/Scenes/JAPAO_INTERIOR.unity`.
- O componente antigo `EgyptCamera25D` foi **desativado somente no Japão interno**; continua nos outros países que o utilizam.
- Novo FOV de **58 graus** para mostrar Kenji e mais da arquitetura do salão.
- Enquadramento com posição relativa aproximada `(+0,45m, +3,8m, +4,8m)` e ponto de interesse à frente de Kenji, olhando para o monge e altar.
- Segue Kenji também na profundidade ao caminhar pelo templo; suavização de 0,19 s.
- Limites no salão em X e Z e detecção de obstáculos por `SphereCastAll`.
- A câmera inicial serializada foi atualizada para corresponder ao novo enquadramento, inclusive fora do Play.
- O sistema `JapanQuest` encontra e configura a câmera quando Kenji nasce perto da porta interna.

## Preservado
- Guia turístico da Itália no Japão externo, monge, katana 3D, conquista e recompensa giratória, portas, HUD de diálogo, progresso, sons e menu Esc.
- Cena, movimentos e recompensas das fases do Egito, Brasil e Itália, além das correções de interface descritas.
- O retorno ao menu não foi removido do jogo — apenas o acesso fora do painel Esc.

## Verificações
- Auditoria estática: **30/30 testes** sobre botões, referências dos componentes, arquivos, alinhamento das HUDs e vínculos de câmera.
- Compilação C# do jogo (Assembly-CSharp): **0 erros e 0 avisos**.
- Compilação das ferramentas do Editor: **0 erros**, 51 avisos de APIs antigas ou obsoletas.
- A Unity atualizou os assets e o log recente não mostrou novos erros de compilação.
- **Pendente:** teste visual em Play Mode, particularmente movimentos até as bordas do mapa externo, enquadramento de Kenji nas portas internas e possíveis obstruções da câmera nos pilares.

## Arquivos modificados
- Assets/TCCJapao/Runtime/JapanQuest.cs
- Assets/TCCJapao/Runtime/JapanExteriorCamera25D.cs
- Assets/TCCJapao/Runtime/JapanInteriorCamera25D.cs (+ meta)
- Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity
- Assets/TCCJapao/Scenes/JAPAO_INTERIOR.unity
- Assets/TCCJapao/Editor/JapanQuestVerification.cs
- Assets/TCCEgito/Runtime/EgyptStageUI.cs
- Assets/TCCBrasil/Runtime/BrazilStageUI.cs
- Assets/TCCAudio/Runtime/StageExperience.cs

## Backup
`C:\Users\User\Documents\TCC_BACKUP_HUD_DUAS_CAMERAS_2026-11-20`

## Relatório técnico
`Docs/AUDITORIA_HUD_E_CAMERAS_2026-11-20.txt`

## Como testar
1. Abra `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity` e pressione Play; caminhe em todas as direções e até as quatro barreiras.
2. Confira Kenji inteiro no enquadramento, a visibilidade do guia italiano e a entrada do templo.
3. Entre em `JAPAO_INTERIOR` e caminhe da porta até o monge; agora a câmera deve acompanhar o personagem na profundidade.
4. Pressione Esc em ambas as cenas; somente dentro dessa pausa estará a opção para voltar ao menu principal.
5. No Brasil, repita o desafio das embaixadinhas depois de obter o berimbau para conferir placar sem artefato sobreposto.
6. Na Itália, inicie o preparo da pizza e confira que o aviso Esc não cobre o cronômetro nem o pedido.
7. No Egito, confira instruções no inferior esquerdo e interação no inferior central sem cruzamento de painéis.
