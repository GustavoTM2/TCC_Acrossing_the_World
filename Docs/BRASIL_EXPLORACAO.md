# Brasil — exploração inicial da praia

Esta entrega acrescenta Lucas caminhando pelo cenário de praia fornecido pela equipe. É a base de exploração da fase brasileira, inspirada no Rio de Janeiro. Campeonato, diálogos, recompensas e conclusão ficam para etapas futuras.

## Versão de desenvolvimento

A partir desta entrega, desenvolver e testar o projeto no **Unity 6000.5.7f1**, conforme solicitado por Gustavo. Não salvar os assets em uma versão mais recente. A versão fica registrada em ProjectSettings/ProjectVersion.txt e os pacotes de renderização devem ser compatíveis com ela.

## Abrir a praia

No Unity Hub, adicionar a pasta raiz do repositório. No Editor, abrir Assets/TCCBrasil/Scenes/Brasil_Praia_Jogavel.unity, ou usar **TCC → Brasil → Abrir praia jogavel**, e entrar em Play.

- **A/D ou setas esquerda/direita:** caminhar lateralmente.
- **W/S ou setas cima/baixo:** caminhar para o fundo ou para a frente.

Nesta etapa Lucas apenas caminha. Espaço ainda não inicia um desafio.

## Como foi feito

- **BrazilPlayer25D:** lê o teclado, normaliza a diagonal, aplica gravidade e desloca o CharacterController. Gira o modelo conforme o movimento e atualiza Speed no Animator.
- **CharacterController:** cápsula física para acompanhar o terreno e colidir com as paredes. O modelo visual é alinhado aos pés da cápsula.
- **Lucas_Movimento.controller:** alterna entre uma pose parada e a caminhada em loop enviada pela equipe. Root motion fica desligado: o script controla a posição.
- **EgyptMovementBounds:** componente existente reaproveitado para gerar quatro paredes invisíveis com BoxCollider.
- **EgyptCamera25D:** câmera existente reaproveitada para acompanhar Lucas lateralmente, com acompanhamento reduzido em profundidade.
- **BrazilExplorationHUD:** mostra o nome da fase e as instruções de movimento.
- **BrazilSceneSetup:** integra o modelo e sua textura/animação e oferece um atalho para abrir a cena pronta.
- **BrazilValidation:** verifica movimento físico, quatro limites, chão, câmera, textura e deformação da malha pela animação.

A cena jogável deriva de Assets/Scenes/brasil.unity e preserva a cena original. Modelo, caminhada e textura de Lucas vêm dos arquivos enviados pela equipe.

## Validação

Verificações concluídas no **Unity 6000.5.7f1**:

- Compilação dos scripts e resolução dos pacotes de renderização/interface compatíveis.
- Movimento lateral e em profundidade com o CharacterController, contato com o terreno e bloqueio nos quatro limites físicos.
- Textura correta no Lucas e deformação real da malha ao avaliar dois momentos da caminhada.
- Teste em Play: estabilização no chão, deslocamento lateral e em profundidade, entrada e saída do estado de caminhada, enquadramento da câmera e alinhamento do modelo à cápsula.
- Abertura das duas cenas do Egito sem scripts ausentes, referências do jogador válidas, contato com o chão e regras de diálogo/recompensa preservadas.
- Renderização de uma imagem da cena jogável para conferir visualmente Lucas, praia e materiais.

A área delimitada abrange aproximadamente **50 m ao longo da praia e 8 m em profundidade**. A posição inicial foi escolhida no terreno existente, perto dos objetos de praia. A textura ausente foi corrigida em uma cópia do TerrainData e da camada de areia, sem alterar o terreno da cena original.

O programa independente para Windows não foi compilado nesta entrega; a demonstração solicitada foi validada no Play do Editor.
