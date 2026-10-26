# Brasil — praia e desafio de embaixadinhas

Desenvolvimento em **Unity 6000.5.7f1**, com URP 17.5.0. A cena jogável é `Assets/TCCBrasil/Scenes/Brasil_Praia_Jogavel.unity`. A cena original `Assets/Scenes/brasil.unity` foi preservada.

## Jogar

Abra a cena e entre em Play, ou use **TCC → Brasil → Jogar fase completa**.

1. A/D e setas laterais movem Lucas pela praia. W/S e setas verticais controlam a profundidade.
2. Aproxime-se do guia e pressione E/Enter. A mesma tecla avança as cinco falas.
3. Siga pela praia até o morador e converse. As seis falas apresentam o desafio e a história do berimbau.
4. Na tela de instruções, pressione Espaço ou clique em Começar. Aguarde a contagem de três segundos.
5. Pressione e solte Espaço quando o marcador alcançar a faixa verde. A posição e a largura mudam a cada acerto; observe a nova faixa durante a breve pausa antes de o marcador avançar. São necessários dez acertos consecutivos.
6. Um toque antecipado, atrasado ou ausente encerra a tentativa. Espaço ou o botão correspondente inicia outra tentativa.
7. Após vencer, o berimbau aparece girando na tela de conquista. Continuar libera a caminhada e mantém o prêmio no canto da tela.

Esc fecha conversas ou cancela o desafio. O botão de menu está identificado como **em breve** e permanece desativado até a integração do menu principal. O progresso desta fase dura apenas a sessão atual de Play; reabrir a cena inicia outra visita.

## Chão e escala

O Terrain da cena jogável estava associado a um material de água. Agora usa `Areia_Terreno_URP.mat`, com o shader de terreno da URP e a textura de areia fornecida. A faixa jogável foi nivelada a aproximadamente 77,13 unidades de altura, com transição suave para o terreno ao redor. O TerrainCollider acompanha os mesmos dados.

`Praia_Alturas_Originais.asset` preserva os dados anteriores para que a preparação seja repetível. As alterações são aplicadas à cópia `Praia_Terreno.asset`, sem editar o terreno da cena original.

Os objetos são dimensionados em relação ao Lucas de 1,8 m: guarda-sóis com cerca de 2,8 m, cadeiras com 1,85 m, prancha com 1,8 m e torre de salva-vidas com 5,2 m. Os objetos são apoiados no chão e afastados da faixa central de caminhada. Quatro paredes invisíveis continuam delimitando aproximadamente 50 m de extensão por 8 m de profundidade.

## Componentes principais

| Código | Responsabilidade |
|---|---|
| `BrazilPlayer25D` | CharacterController, gravidade, movimento lateral/profundidade, direção do modelo, animação e bloqueio durante conversas/desafio. |
| `BrazilStage` | Objetivos, cinco falas do guia, seis do morador, estados da fase, contagem regressiva, tentativas e entrega única do prêmio. |
| `BrazilTimingChallenge` | Tempo de referência de 1,4 s, preparação de 0,35 s e faixas com posição e largura variáveis a cada acerto. Dez acertos seguidos. Não depende da interface. |
| `BrazilStageUI` | Objetivos, caixas de diálogo, instruções, faixa de tempo, placar, tentativa e tela do prêmio. |
| `BrazilKickPose` | Pequeno movimento procedural de coxa e perna durante os toques; restaura o Animator ao sair. |
| `BrazilRewardDisplay` | Câmera exclusiva, textura transparente e rotação do modelo 3D do berimbau. Libera os recursos ao sair da cena. |
| `BrazilResidentSetup` | Aplica o modelo e a textura do morador, ajusta escala e pés e preserva o ponto de interação. |
| `BrazilAdventureSetup` | Prepara terreno, proporções, personagens, bola, prêmio e referências da cena. |
| `BrazilAdventureValidation` | Testa regras de tempo e o fluxo em Play, incluindo paredes, diálogos, erros, prêmio e continuação. |
| `EgyptCamera25D`, `EgyptMovementBounds`, `EgyptInput`, `EgyptNpcIdle` | Componentes existentes reutilizados para câmera, limites, teclado e movimentos sutis dos personagens. |

O marcador mostra o mesmo tempo que a regra de acerto utiliza. O teclado é lido por pressionamento, portanto segurar Espaço não produz dez toques. Na conversa e no desafio, o movimento fica bloqueado, mas a gravidade mantém o jogador apoiado.

O pivô Visual_Lucas foi baixado em 16 cm (mais 8 cm no ajuste final) para deixar os pés dentro da areia e reduzir a sensação de flutuação. O deslocamento afeta apenas o modelo; a cápsula de colisão e a posição do jogador continuam iguais. A preparação de novas cenas aplica o mesmo ajuste.

## Assets e limites atuais

- Lucas, caminhada, bola, berimbau e texturas vieram dos arquivos da equipe. O guia reutiliza o modelo e a preparação de articulações já presentes no projeto.
- Em 26/10/2026, o visual provisório foi substituído pelo modelo enviado em Bandido.fbx e pela textura bandido.png. No jogo ele continua identificado como Morador brasileiro. O FBX foi importado em Assets/TCCBrasil/Art/Morador, com material URP, altura de 1,8 m e movimentos leves nos próprios ossos. O arquivo de edição Moradorbrasil.blend fica em SourceAssets/Brasil, fora de Assets, para não exigir Blender ao abrir a Unity.
- Não foi enviada uma animação específica de embaixadinha. O movimento de perna é procedural e simples; não equivale a um clip final de animação.
- As falas vêm de `Falas_Horizon_Travel.docx`. O nome de exibição do interlocutor do desafio é “Morador brasileiro”.
- O escopo é a demonstração em Play no Editor. Não foi exportado um executável para Windows nesta entrega.

## Validação da entrega de 21/10/2026

Executada em Play na Unity 6000.5.7f1: regras de tempo (cedo, tarde, sem toque, repetição e dez acertos); cinco falas do guia e seis do morador; bloqueio de movimento na conversa; falha e nova tentativa; entrega única do prêmio; retorno à exploração; movimento lateral e em profundidade; bloqueio nas quatro paredes.

Foram conferidas oito capturas da interface e a textura transparente do berimbau. A medição da malha animada encontrou os pés aproximadamente 1,2 cm acima do terreno na pose inicial.

O teste por linha de comando usa `CrossingTheWorld.TCCBrazil.Editor.BrazilAdventureValidation.Run`, salva os resultados em `Temp/BrazilValidation` e fecha o Editor ao concluir. A variável de ambiente `TCC_BRAZIL_VALIDATION_OUTPUT` permite escolher outra pasta. Para apenas jogar e deixar aberto, use o menu **TCC → Brasil → Jogar fase completa**.

## Atualização do morador — 26/10/2026

Modelo e textura enviados integrados ao ponto existente do desafio. Verificados em Play: diálogos, dez acertos, novas tentativas, berimbau, continuação e colisões. As oito capturas foram revisadas com o novo personagem. O teste automatizado passou a aguardar quadros reais do jogo para verificar a renderização do prêmio, inclusive com o Editor em segundo plano.

## Padrão visual das fases

O aviso de aproximação do guia e do morador usa o mesmo método de desenho do Egito: caixa centralizada na parte inferior, borda dourada, fundo escuro e texto claro de 19 pontos. O texto segue “E — Conversar com o guia/morador”. A escala acompanha a resolução da janela. Antes da primeira conversa com o guia, o morador mostra apenas o lembrete para procurar o guia.

Depois da primeira conversa completa, o guia usa um lembrete curto para encaminhar Lucas ao desafio. Após a vitória, o guia e o morador têm despedidas próprias; o morador relembra que o berimbau era do avô. Essas conversas voltam à exploração, sem reabrir o prêmio ou reiniciar o campeonato. Cancelar a apresentação inicial mantém as falas completas na próxima interação. O progresso vale durante a sessão atual da fase.

O cabeçalho segue o formato PAÍS / LOCAL, como EGITO / AREIAS DO TEMPO e BRASIL / RIO DE JANEIRO. Ao concluir o Brasil, o objetivo mostra “Berimbau conquistado — fase concluída.” O cartão do artefato fica no canto superior direito, alinhado ao cabeçalho, com a imagem girando à esquerda e o rótulo ARTEFATO e o nome Berimbau antigo à direita. Cores, borda dourada, tipografia e posições seguem a referência do Egito. As próximas fases devem seguir o mesmo padrão.

A faixa do minigame é sorteada entre regiões esquerda, central e direita. Região e largura sempre mudam em relação ao toque anterior e ficam estáveis até o acerto ou a falha. A indicação visual e a regra de acerto leem os mesmos limites. Uma pausa de 0,35 s permite observar cada faixa nova; manter Espaço pressionado não gera acertos automáticos.
