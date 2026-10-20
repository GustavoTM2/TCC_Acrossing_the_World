# Crossing The World — TCC

Projeto de aventura e exploração em Unity ligado ao turismo cultural. O Egito contém diálogos e a conquista de um artefato; o Brasil já possui Lucas explorando a praia em 2,5D.

## Abrir e testar

1. No Unity Hub, adicione a pasta deste repositório como projeto existente.
2. A versão de desenvolvimento é **Unity 6000.5.7f1**, com **URP 17.5.0**, conforme solicitado pela equipe. A exploração do Brasil foi testada em Play nessa versão; as duas cenas do Egito também passaram pela verificação de referências, chão e lógica de diálogo.
3. Aguarde a importação dos assets e a compilação dos scripts.
4. Para o Egito, abra `Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity` e entre em Play.
5. Para o Brasil, abra `Assets/TCCBrasil/Scenes/Brasil_Praia_Jogavel.unity`, ou use **TCC → Brasil → Abrir praia jogavel**, e entre em Play.

## Controles

| Tecla | Ação |
|---|---|
| A/D ou setas esquerda/direita | Caminhar lateralmente |
| W/S ou setas cima/baixo | Andar para o fundo ou para a frente |
| Espaço | Pular |
| E/Enter | Interagir e avançar as falas |

No Brasil, esta entrega inclui somente caminhada com A/D e W/S ou setas; o desafio com Espaço e as conversas serão implementados depois.

## Estado atual do Brasil

- Modelo, textura e caminhada do Lucas enviados pela equipe.
- Praia original preservada e cena jogável independente com a textura de areia existente.
- Controle em 2,5D com gravidade, câmera lateral, colisão com objetos e quatro paredes invisíveis.
- Faixa de exploração de 50 metros lateralmente, com 8 metros de profundidade.
- Aviso na tela com o nome da fase e as teclas de movimento.

A integração e os testes estão descritos em [Docs/BRASIL_EXPLORACAO.md](Docs/BRASIL_EXPLORACAO.md).

## Estado atual do Egito

- Duas cenas jogáveis: exterior e interior da pirâmide, com a estrutura original adaptada à escala da personagem.
- Movimentação com CharacterController, câmera lateral e profundidade limitada por paredes invisíveis.
- Conversas com o guia e a múmia, objetivos e progressão da missão.
- Múmia junto ao portal do patamar superior, acessível pela escadaria original, com câmera lateral e limites de caminhada ajustados ao percurso.
- Câmera interna presa a um trilho paralelo à parede: acompanha somente a lateral, mantendo profundidade, altura e ângulo fixos.
- Trilho dentro da sala, com enquadramento próximo e barreira invisível na frente para Amira não passar sob a câmera. Paredes opacas e fechamento das frestas do piso evitam mostrar o vazio externo.
- Câmera interna elevada à altura da cabeça da múmia no patamar, com leve inclinação fixa para baixo.
- Diálogos do roteiro Horizon Travel: 5 caixas na chegada e 11 de Rami, com a entrega do escaravelho após a última fala.
- Entrega animada do Velho Escaravelho ao concluir a conversa com a múmia, seguida da tela de conquista.
- Escaravelho 3D girando na conquista e no canto direito ao continuar explorando, inclusive após voltar ao exterior.
- Altura visual de Amira ajustada em 8 cm no exterior para aproximar os pés da areia.
- Movimentos sutis de cabeça e braços dos NPCs, com gestos da múmia durante suas falas.
- Transição com tela preta na entrada e na saída da pirâmide.
- A tela preta cobre toda a janela antes de carregar o destino e aguarda a nova câmera antes de revelar a cena.
- Caixas de diálogo com bordas, cores, contador de falas e tamanho proporcional à janela.
- Pilares que ficam translúcidos quando encobrem Amira.
- Textura de areia aplicada ao terreno exterior.
- Vegetação com materiais locais em tons secos e terrosos; Amira aproximada da câmera no exterior.
- Exploração de toda a sala interna e conclusão com **Continuar a fase** e **Voltar para o menu** (ainda desativado).

O progresso é mantido durante a sessão e não possui salvamento permanente. Como as texturas de cor das plantas não vieram nos arquivos recebidos, a cena usa materiais substitutos de cor sólida, preservando os modelos originais. Amira utiliza a animação de caminhada fornecida e ainda não tem um clip específico de salto.

## Próximas entregas planejadas

- Revisar a experiência completa com a equipe e confirmar a execução no Unity da escola.
- Expandir a fase brasileira com diálogos e um desafio de dez embaixadinhas consecutivas, usando Espaço no momento certo.

## Versionamento

`Assets`, incluindo os arquivos `.meta`, `Packages` e `ProjectSettings` fazem parte do repositório. Caches, logs e arquivos de compilação do editor são excluídos pelo `.gitignore`. Cada melhoria futura deve ser revisada e testada antes de gerar um novo commit.

## Correções e validação

A causa do interior vazio e as alterações nos scripts estão descritas em [Docs/EGITO_CORRECOES.md](Docs/EGITO_CORRECOES.md). Os testes físicos e em Play verificaram o caminho até a múmia, a ordem dos diálogos, o bloqueio durante a transição, a entrega animada, a recompensa única e o retorno ao exterior com o progresso preservado.

Os ajustes de altura, transição e escaravelho de 19/10/2026 estão registrados em [Docs/EGITO_AJUSTES_VISUAIS.md](Docs/EGITO_AJUSTES_VISUAIS.md), incluindo os testes em Play e a pendência de configuração que bloqueou a tentativa de exportar um executável no Unity 6000.5.7f1.
