# Ajustes do Egito — 19/10/2026

Versão utilizada: **Unity 6000.5.7f1**. As mudanças já estão aplicadas nas duas cenas jogáveis.

## O que mudou

- **Diálogos Horizon Travel:** aplicadas as 5 caixas da chegada de Amira e as 11 caixas de Rami, mantendo o texto e a ordem de `Falas_Horizon_Travel.docx`. `EgyptDialogueContent` centraliza as falas; `EgyptDialogueSetup` grava os novos arrays nas duas cenas jogáveis. As referências históricas do documento não são exibidas no diálogo. Rami aparece com seu nome na conversa e na interação, e seus gestos reconhecem esse nome. A recompensa continua sendo concedida somente ao encerrar a última fala.
- **Amira no exterior:** o modelo foi abaixado em 0,08 m. O objeto `Altura_Visual_Amira` fica entre a cápsula física e o modelo animado. Assim a animação não desfaz o ajuste e a colisão continua com as mesmas medidas. O campo `Visual Height Offset` de `EgyptPlayer25D` controla a altura ao iniciar o jogo. O interior mantém a altura anterior.
- **Entrada e saída da pirâmide:** `EgyptSceneTransition` cria um Canvas de tela inteira que permanece durante a troca. A tela escurece em 0,65 s, chega ao preto completo e então carrega a cena. Depois da carga, espera a inicialização da câmera e mais 0,3 s antes de clarear em 0,65 s. O tempo de carregamento varia conforme o computador. A movimentação e novas interações ficam bloqueadas até terminar.
- **Escaravelho na interface:** `EgyptScarabDisplay` usa uma cópia do modelo e da textura fornecidos pela equipe. Uma câmera exclusiva desenha o modelo em uma RenderTexture transparente de 512 × 512 pixels. O modelo gira a 35 graus por segundo. `EgyptStageUI` mostra essa imagem no painel de conquista e, ao continuar explorando, no canto direito. O artefato volta a aparecer ao sair da pirâmide, mantendo o progresso da sessão.
- **Múmia e escadaria:** a múmia fica no patamar original, a aproximadamente 2,43 m de altura, junto ao portal. Amira começa na extremidade oposta da sala, em Z = 16,5. A/D percorrem seu comprimento e W/S exploram a largura. A câmera corre em um trilho paralelo à parede lateral: acompanha apenas o comprimento da sala. A profundidade, a altura e a rotação ficam fixas, inclusive quando Amira pula ou sobe a escada.
- **Posição no exterior:** Amira começa em Z = 956,03, 1,2 m à frente da posição anterior, em direção à câmera. O offset da câmera passou de 9 para 7,8 m para ela ficar efetivamente mais próxima, em vez de deslocar câmera e personagem juntas. Os limites externos acompanham a nova faixa e a altura inicial é calculada no terreno.
- **Vegetação:** 36 objetos receberam materiais locais `Vegetacao_*`, compatíveis com URP, com cores secas, terrosas e verde-oliva. Os arquivos de cor originais do TerrainSampleAssets estão ausentes tanto no projeto quanto nos ZIPs recebidos. Esta é uma substituição por materiais de cor sólida, não uma recuperação das imagens originais. Os modelos, as posições e os assets originais foram preservados.

`EgyptInteriorRepair` configura o percurso, o ponto da múmia, a saída e o escaravelho entregue por ela. Os 62 módulos visuais originais continuam no cenário. Uma camada visual adicional, abaixo do piso e sem collider, fecha pequenas frestas entre os módulos e a escada. Os degraus têm cerca de 24 cm e são percorridos com os MeshColliders originais e `CharacterController.stepOffset = 0.35`, sem uma rampa adicional. As paredes invisíveis contornam a sala: X entre -2 e 6,7 e Z entre -7,2 e 17,7. Pilares, paredes e corrimãos continuam bloqueando a passagem. A câmera fica dentro da sala e as paredes, o piso e o teto permanecem opacos. Apenas pilares e corrimãos que obstruem a personagem podem ficar translúcidos. A nova barreira da frente reduz o espaço explorável para impedir que Amira passe embaixo ou atrás da câmera. `EgyptInteractionPoint.CanInteractFrom` verifica distância e diferença de altura: a múmia aceita interação a até 1,5 m e com diferença vertical de até 30 cm. Os demais pontos mantêm o comportamento anterior quando o limite vertical é zero.

O estúdio de prévia fica afastado do cenário, usa a camada 31 e é destruído junto com sua câmera e RenderTexture quando a cena termina. O material da interface não depende das luzes do cenário.

A prévia é atualizada pelo próprio `EgyptScarabDisplay` em `LateUpdate`, usando `UniversalRenderPipeline.SingleCameraRequest`. A câmera da prévia permanece desativada para evitar renderização duplicada. O painel abre automaticamente após a entrega, com o escaravelho girando e os botões **Voltar para o menu** (desativado até a integração da cena de menu) e **Continuar a fase**. E/Enter deixa de fechar diretamente esse painel; a escolha é feita pelo botão. Ao continuar, a exploração é liberada e o artefato permanece no canto direito.

## Como conferir

1. Abra `Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity` e entre em Play.
2. Observe os pés de Amira e experimente caminhar e pular.
3. Converse com o guia usando E ou Enter. Vá à entrada e aperte E para verificar o escurecimento completo.
4. Dentro da pirâmide, siga com D até subir a escadaria. Aproxime-se da múmia junto ao portal e converse com E. Ao terminar, o escaravelho aparece girando no painel de conquista.
5. Confira o escaravelho e os dois botões. Clique em **Continuar a fase**. O artefato permanece no canto direito.
6. Desça a escadaria com A e continue até a saída. Volte ao exterior usando E e confira a transição e a permanência da conquista.

O menu **TCC → Egito → Atualizar altura e escaravelho na tela** reaplica apenas as referências da apresentação e o ajuste de altura. Não é necessário usá-lo para abrir as cenas já atualizadas.

O menu **TCC → Egito → Aplicar falas Horizon Travel** reaplica os textos fornecidos nas duas cenas. Ele substitui os arrays de falas salvos no Inspector pelo conteúdo de `EgyptDialogueContent`.

O menu **TCC → Corrigir interior da piramide** reaplica o percurso lateral e a posição da múmia. O comando de atualizar os modelos também conserva esse layout. Para conferir apenas o interior, abra `Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity` e entre em Play.

O menu **TCC → Egito → Atualizar exploracao e vegetacao** reaplica a posição externa, os materiais e a exploração da sala. **TCC → Egito → Jogar para testar** abre o exterior em Play e mantém o jogo aberto para teste manual.

**TCC → Egito → Testar interior da piramide** abre diretamente a sala interna em Play. Nela, `EgyptCamera25D.useWallRail` projeta a posição de Amira sobre um segmento paralelo à parede: X = -7,5 e Y ≈ 4,03 permanecem fixos, enquanto Z varia suavemente de -3,8 a 14,5. A altura corresponde ao piso da múmia mais 1,6 m, aproximadamente na altura da cabeça. A direção do olhar é constante, inclinada cerca de 11 graus para baixo para mostrar também o piso inferior. O campo de visão é de 58 graus. O modo de trilho está habilitado apenas no interior do Egito; as outras cenas conservam o acompanhamento anterior.

## Verificação realizada

- Roteiro do documento comparado com os arrays serializados nas cenas: 5 caixas do guia e 11 de Rami, na ordem original. Teste com E confirmou que não há recompensa antes de concluir as 11 caixas. Medição com o estilo da interface confirmou que as 16 falas cabem na área de texto; capturas do guia e de Rami foram conferidas.
- Câmera elevada a aproximadamente 4,03 m, equivalente ao patamar da múmia mais 1,6 m. Nova passagem pela sala verificou enquadramento dos pés/cabeça, barreiras, trilho fixo, escadaria e fechamento do fundo.
- Compilação dos scripts no Unity 6000.5.7f1.
- Teste automatizado no Play do Editor com entradas de teclado E, caminhada e pulo.
- Entrada e saída reais entre as cenas, com a opacidade em 100% quando a cena de destino é ativada.
- Capturas da imagem do jogo: tela preta nos quatro cantos e no centro durante a troca, painel de conquista, artefato no canto e retorno ao deserto.
- Saída da pirâmide com `Time.timeScale = 0`, verificando que a transição usa tempo independente da pausa.
- Comparação de duas imagens da prévia para comprovar mudança pela rotação, com fundo transparente.
- Recompensa concedida uma única vez, progresso preservado e uma única câmera de prévia após o retorno.
- Subida e descida completas da escadaria com `TickMovement` e o CharacterController real, sem pulo ou teleporte no percurso interno. O teste chegou ao patamar em `(0, 2.46, -5.28)`, próximo da múmia em `(0, 2.43, -6.25)`, e voltou caminhando até a saída.
- Bloqueio pelas duas paredes de profundidade, ausência de interação no piso inferior e conversa com E no patamar. Capturas conferidas na base, nos degraus e junto ao portal.
- Exploração da faixa acessível: caminhada entre a barreira frontal e o lado direito da sala, subida ao patamar e retorno à saída da parede oposta.
- Câmera no trilho: verificação em todos os quadros do percurso interno confirmou X/Y e rotação constantes, inclusive no pulo e nos degraus. O deslocamento lateral percorreu os 18,3 m do trilho. Pés e cabeça ficaram dentro da imagem nos pontos de verificação, incluindo os limites de profundidade e os lados do patamar.
- Aproximação da câmera: trilho dentro da parede, em X = -7,5; teste físico da barreira frontal em X = -2 (centro da cápsula para próximo de -1,7). Capturas verificadas na entrada, piso e patamar. Uma passagem de teste com fundo magenta detectou a fresta entre o piso e os degraus; após o fechamento inferior, a checagem de fundo visível passou nos pontos capturados.
- Leitura da RenderTexture sem solicitar renderização pelo teste: o escaravelho é desenhado e gira pelo próprio componente do jogo. A tecla E não fecha mais o painel de conclusão.
- Medição da malha pelos ossos e pesos da animação: na pose inicial, o ponto mais baixo dos pés ficou aproximadamente 1,9 cm dentro do terreno na posição de partida. A altura varia naturalmente ao longo da caminhada.

As capturas foram feitas na janela Game do Editor, em formato amplo. Não representam uma verificação em todas as resoluções.

## Pendências fora desta entrega

Em 26/10/2026, as conversas de retorno receberam falas próprias: depois da apresentação, o guia lembra Amira de visitar Rami na pirâmide e diz que aguardará seu retorno. Após a conquista, Rami agradece a conversa e se despede; o guia também reconhece a viagem concluída. Essas interações não repetem a entrega do escaravelho nem reabrem o painel de conclusão.

A tentativa de gerar um executável de verificação foi bloqueada pelo asset `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`, herdado da versão anterior do projeto: ele está serializado na versão 11, enquanto a URP 17.5 do Unity 6000.5.7f1 exige a versão 10. Será necessário preparar essas configurações na versão correta antes de exportar o jogo. Nenhum número de versão foi alterado artificialmente para contornar essa checagem. A validação desta entrega foi concluída no Play do Editor.

Continuam pendentes a integração do botão **Voltar para o menu** e a porta do interior. As falas do guia e de Rami já foram substituídas pelo roteiro fornecido. O painel abre após a entrega, sem exigir clique no artefato, conforme o pedido mais recente. Não há salvamento permanente da conquista entre sessões.
