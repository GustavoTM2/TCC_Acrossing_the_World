# Japão — câmera externa 2,5D e HUD revisadas
Data: 20/11/2026

Projeto final:
`C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main`

## Câmera do exterior

A cena `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity` agora tem o componente **JapanExteriorCamera25D** associado diretamente ao objeto `Camera_Exploracao`, no Inspector.

- O controlador antigo `EgyptCamera25D` foi **desativado somente na câmera do Japão externo**. Outras fases não foram alteradas.
- A câmera segue Kenji de forma suave com `Vector3.SmoothDamp`, amortecendo movimentos rápidos e evitando saltos bruscos.
- A câmera acompanha os eixos X e Z, inclusive quando Kenji avança em direção ao templo; usa pequena antecipação da direção de movimento.
- Os quatro `BoxCollider` já existentes em `Paredes_Invisiveis` delimitam a área de enquadramento. A câmera calcula os limites na inicialização e não acompanha o alvo para fora desse espaço visual.
- O ângulo fica fixo (para não girar e mostrar o que está fora do mapa) e a câmera usa um FOV mais fechado, com ajuste em telas ultrawide.
- A câmera tem verificação de obstrução por `SphereCastAll` e se aproxima do personagem caso haja telhados ou paredes no caminho.
- A transformação e o campo de visão inicial do objeto no Editor também foram corrigidos para corresponder ao enquadramento configurado para o jogo.

### Configuração no Inspector (Camera_Exploracao)
| Campo | Valor |
|---|---|
| Target | Kenji_Jogador |
| Boundary Root | Paredes_Invisiveis |
| Downward Angle | 68 graus |
| Vertical Field Of View | 44 graus |
| Height Above Player | 6.4 |
| Distance Behind Player | 3.2 |
| Follow Smooth Time | 0.23 |
| Movement Look Ahead | 0.16 |
| Boundary Safety | 0.45 |
| Avoid Walls | marcado |
| Collision Radius | 0.22 |

A câmera está otimizada para o percurso do Japão e para evitar mostrar além dos limites físicos. A correspondência exata com uma imagem de referência depende da resolução e de verificação visual na Game View.

## HUD externa

Padronização visual mantida no tema vermelho japonês e creme, com painéis retangulares e texto em alto contraste:

- **Superior esquerdo:** painel `JAPÃO / O SANTUÁRIO DOS ANTIGOS` e objetivo, agora com largura menor de 610 unidades (antes 640).
- **Superior central:** aviso `ESC · MENU DA FASE`, deslocado para não ficar por cima da recompensa.
- **Superior direito:** katana 3D giratória depois da conquista, sem sobreposição do aviso de pausa.
- **Inferior centro:** aviso `E / Enter` quando Kenji está próximo de um personagem ou porta. Agora tem espaço reservado acima dos controles.
- **Inferior esquerdo:** controles de movimento/interação/pausa em painel próprio, sem cruzar com o aviso de interação.
- Os tamanhos respeitam escala uniforme em diferentes resoluções para não distorcer a interface.
- Durante o menu de pausa aberto, a HUD de exploração fica oculta.

## O que foi preservado

Guia turístico idêntico ao da Itália; a conversa de entrada; Kenji e animações; entrada e saída do templo; monge; katana e tela de conquista; música; menu de pausa pelo Esc; outras fases.

## Arquivos alterados

- `Assets/TCCJapao/Runtime/JapanExteriorCamera25D.cs` e `.meta` — novo controlador exclusivo
- `Assets/TCCJapao/Runtime/JapanQuest.cs` — inicialização da câmera e ajustes de GUI
- `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity` — controlador anexado, controlador antigo desabilitado, enquadramento inicial ajustado
- `Assets/TCCAudio/Runtime/StageExperience.cs` — posição da dica Esc alinhada à nova HUD
- `Assets/TCCJapao/Editor/JapanQuestVerification.cs` — novas verificações da câmera

## Verificações

- 24/24 verificações estáticas de bindings, controlador, limites, obstrução e HUD passaram.
- Compilação dos scripts do jogo: **0 erros e 0 avisos**.
- Compilação das ferramentas Editor: **0 erros**, 50 avisos de compatibilidade de APIs antigas.
- **Pendente:** verificar visualmente com Kenji andando em todos os extremos do mapa, na Game View em várias resoluções. Uma inspeção estática não prova que nenhum fragmento do terreno aparecerá em todas as câmeras, resoluções ou ângulos.

## Teste rápido

1. Abra `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity` na Unity e pressione **Play**.
2. Verifique o seguimento da câmera usando A/D e W/S; caminhe até as quatro paredes invisíveis, especialmente a entrada do templo.
3. Confira se o cenário fica enquadrado e se Kenji continua visível nas laterais.
4. Chegue perto do guia e da porta; verifique a caixa de interação sem sobreposição com os controles.
5. Abra o menu Esc e retorne à fase; verifique o som e a HUD.
6. Volte do interior do templo e verifique o enquadramento ao reaparecer perto da entrada.
7. Se quiser ajustar mais, os campos do componente `JapanExteriorCamera25D` estão expostos no Inspector.

## Backup

`C:\Users\User\Documents\TCC_BACKUP_CAMERA_HUD_JAPAO_2026-11-20`

## Auditoria detalhada

`Docs/AUDITORIA_CAMERA_HUD_JAPAO_2026-11-20.txt`
