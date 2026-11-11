# Menu principal — funcionamento e histórico

Unity usada: **6000.5.7f1**. Abra `Assets/TCCMenu/Scenes/Menu_Principal.unity` e aperte Play, ou use **TCC → Menu → Jogar pelo menu**. A cena é a primeira da lista de cenas do projeto. A cena antiga `Assets/Scenes/menu.unity` foi preservada.

## Funcionamento atual

- Fundo parado baseado na imagem enviada pela equipe, sem pulsação.
- Jogar, Opções, Créditos e Sair. A opção selecionada ganha texto maior, cor dourada e uma bússola ao lado.
- Setas para cima/baixo ou W/S selecionam; Enter confirma. O mouse também seleciona e clica.
- Jogar abre quatro janelas lado a lado. Setas para esquerda/direita ou A/D escolhem o país; Enter viaja; Esc volta.
- Egito e Brasil podem ser escolhidos em qualquer ordem. Itália e Japão abrem cenas de exploração, com indicação EXPLORAR.
- Opções, Créditos e Sair ainda não executam suas funções; apenas mostram um aviso breve.
- As telas de conquista das duas fases agora possuem Voltar para o menu funcional.

## Componentes da cena

O Canvas desenha a interface sobre a câmera. O CanvasScaler adapta o tamanho à janela, preservando uma área de referência de 1672 × 941. Em telas de outra proporção, sobra uma margem escura, sem esticar o mapa. Os objetos Image/RawImage mostram as artes; Text mostra as palavras; Button recebe os cliques. O EventSystem cuida do mouse. A navegação por teclado é feita apenas pelo TravelMenu para não avançar duas opções em uma tecla.

Todos os botões e textos ficam salvos na Hierarchy e podem ser selecionados pelo Inspector. Não é necessário reconstruir o menu toda vez que abrir o projeto.

## Scripts de execução

| Arquivo | Função |
|---|---|
| `Assets/TCCMenu/Runtime/TravelMenu.cs` | Lê as teclas, destaca a seleção, troca as telas e pede a abertura da fase. |
| `Assets/TCCMenu/Runtime/MenuPointer.cs` | Usa o mesmo destaque ao passar o mouse e encaminha o clique. |
| `Assets/TCCMenu/Runtime/TravelProgress.cs` | Salva e consulta o progresso local com PlayerPrefs. |
| `Assets/TCCEgito/Runtime/EgyptSceneTransition.cs` | Reaproveita a tela preta e a carga assíncrona também para o menu e o Brasil. |

No TravelMenu, `Start` prepara a primeira tela; `Update` lê o teclado; `MoveSelection` muda o índice; `UpdateSelection` muda cor, tamanho e bússola; `ShowDestinations` abre os países; `ConfirmSelection` executa a escolha. Os comentários explicam a intenção de cada trecho. A explicação completa do projeto será feita com a equipe depois, passo a passo.

## Progresso e retorno

O progresso é mantido apenas durante a viagem, inclusive na passagem entre exterior e interior do Egito. Ao voltar ao menu ou iniciar uma nova execução, as conquistas e conversas anteriores são reiniciadas. Assim, Egito e Brasil podem ser jogados novamente do começo.

## Artes e fonte

- `Assets/TCCMenu/Art/Menu_Fundo.png`: fundo preparado a partir da imagem enviada. Os textos dos botões ficam separados na Unity.
- `Assets/TCCMenu/Art/Bussola.png`: bússola com transparência para acompanhar a seleção.
- `Assets/TCCMenu/Art/Egito.png` e `Brasil.png`: capturas dos cenários do projeto. As janelas Itália/Japão usam imagens dos cenários e a indicação EXPLORAR, conforme a atualização de 05/11/2026.
- Botões usam [Cinzel, do repositório Google Fonts](https://github.com/google/fonts/tree/main/ofl/cinzel). A licença está em `Assets/TCCMenu/Fonts/OFL.txt`. Os textos auxiliares usam a Liberation Sans já presente no projeto.

As duas artes preparadas foram editadas com a ferramenta integrada image_gen. Prompts utilizados:

**Fundo:** “Use case: precise-object-edit. Edit target: the attached Crossing The World menu illustration. Produce a clean background plate for a Unity menu. Preserve the entire original image composition, 16:9 aspect ratio, title CROSSING THE WORLD, globe, navy panel, thin gold ornamental separators, explorer silhouette, map, landmarks, colors and all details. ONLY erase the four button words JOGAR, OPÇÕES, CRÉDITOS, SAIR and the small bright golden compass immediately LEFT of JOGAR. Fill only their former areas with seamlessly matching navy textured background. Keep the large faint decorative compasses and all other art unchanged. Leave no button words or active-selection compass. Do not redesign, reframe, add labels, or move any landmark.”

**Bússola:** “Use case: background-extraction. Extract ONLY the small gold compass rose immediately to the LEFT of the word JOGAR in the provided image. Output one isolated golden eight-point compass rose icon centered, with real transparent background, tightly framed with modest clear margin. Preserve its delicate hand-drawn antique gold/navy line style. No words, no map, no panel, no other objects, no glow, no backdrop. This is a static Unity menu selection cursor.”

## Ferramentas do Editor

`TravelMenuSetup` cria e salva os objetos da cena. Use Preparar menu principal apenas para reconstruir a interface, pois isso substitui alterações feitas manualmente nessa nova cena. `TravelMenuPreview` abre o menu e mantém o Play ligado. `TravelMenuValidation` verifica o fluxo com cenas reais e salva capturas; ele usa chaves próprias de teste, sem alterar as conquistas do jogador.

Esta entrega é voltada ao Play no Editor. A pendência anterior das configurações URP para exportar o executável continua documentada em `EGITO_AJUSTES_VISUAIS.md`.

## Histórico de validação

Os registros abaixo descrevem verificações anteriores, não testes executados nesta revisão documental. O comportamento atual é resumido no início deste guia.

### Validação em 26/10/2026

O teste em Play na Unity 6000.5.7f1 passou com W/S, A/D, seta para baixo, Enter e Esc simulados no Input System; seleção circular; destaque por mouse; cliques dos botões; textos selecionados cabendo na largura; destinos Em breve sem carga; viagem ao Brasil antes do Egito; conclusão e retorno de ambas as fases; e revisita sem entrega duplicada. Sete capturas foram conferidas, incluindo tela inicial, seleção de Opções, países e as duas conquistas.

Os testes usam as chaves `CrossingTheWorld.Tests.Menu.*` e as apagam ao terminar. A validação anterior do Brasil também foi isolada em suas próprias chaves para não sobrescrever o progresso real.

## Atualização de 04/11/2026

Validação de retorno atualizada para exigir uma partida nova, conforme solicitado. Em 05/11/2026, o teste integrado passou na Unity 6000.5.7f1: ações de seleção, destaque, viagens, conclusão de Brasil e Egito e reinício ao voltar ao menu. O teste aciona os métodos do menu diretamente; não simula o teclado físico.

## Exploração liberada em 05/11/2026

Itália e Japão agora abrem cenas de exploração pelo menu: Sofia nas pontes e Kenji no templo. As janelas exibem EXPLORAR e imagens dos cenários. Esc ou Voltar ao menu carrega o menu principal; selecione Jogar para abrir novamente a escolha de países.


## Retorno durante as fases — 06/11/2026

Brasil e Egito, tanto no exterior quanto dentro da pirâmide, exibem o mesmo botão Voltar ao menu no alto da tela. Ele tem fundo escuro semitransparente, contorno e texto dourados e destaque ao passar o mouse. O botão usa o retorno já existente com transição preta; o menu reinicia o progresso para permitir uma nova partida. Durante a breve entrega do escaravelho, o botão fica desabilitado até a animação terminar.

Verificação integrada aprovada na Unity 6000.5.7f1: retorno do Brasil e do Egito, revisita sem conquistas e transições. Interface conferida em capturas do jogo.

