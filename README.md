# Crossing The World — TCC

Projeto de aventura e exploração em Unity ligado ao turismo cultural. A fase do Egito contém exploração, diálogos e a conquista de um artefato cultural.

## Abrir e testar

1. No Unity Hub, adicione a pasta deste repositório como projeto existente.
2. O projeto foi testado no **Unity 6000.6.4f1**, com **URP 17.6.0**. A escola utiliza **6000.6.0f1**; a execução nessa atualização ainda precisa ser confirmada.
3. Aguarde a importação dos assets e a compilação dos scripts.
4. Abra `Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity` e entre em Play.

## Controles

| Tecla | Ação |
|---|---|
| A/D ou setas esquerda/direita | Caminhar lateralmente |
| W/S ou setas cima/baixo | Andar para o fundo ou para a frente |
| Espaço | Pular |
| E/Enter | Interagir e avançar as falas |

## Estado atual do Egito

- Duas cenas jogáveis: exterior e interior da pirâmide, com a estrutura original adaptada à escala da personagem.
- Movimentação com CharacterController, câmera lateral e profundidade limitada por paredes invisíveis.
- Conversas com o guia e a múmia, objetivos e progressão da missão.
- Entrega animada do Velho Escaravelho ao concluir a conversa com a múmia, seguida da tela de conquista.
- Movimentos sutis de cabeça e braços dos NPCs, com gestos da múmia durante suas falas.
- Transição com tela preta na entrada e na saída da pirâmide.
- Caixas de diálogo com bordas, cores, contador de falas e tamanho proporcional à janela.
- Pilares que ficam translúcidos quando encobrem Amira.
- Textura de areia aplicada ao terreno exterior.

O progresso é mantido durante a sessão e não possui salvamento permanente. Alguns assets de plantas do exterior ainda têm texturas ausentes nos arquivos recebidos. Amira utiliza a animação de caminhada fornecida e ainda não tem um clip específico de salto.

## Próximas entregas planejadas

- Revisar a experiência completa com a equipe e confirmar a execução no Unity da escola.
- Desenvolver a fase brasileira com Lucas e um desafio de dez embaixadinhas consecutivas, usando Espaço no momento certo.

## Versionamento

`Assets`, incluindo os arquivos `.meta`, `Packages` e `ProjectSettings` fazem parte do repositório. Caches, logs e arquivos de compilação do editor são excluídos pelo `.gitignore`. Cada melhoria futura deve ser revisada e testada antes de gerar um novo commit.

## Correções e validação

A causa do interior vazio e as alterações nos scripts estão descritas em [Docs/EGITO_CORRECOES.md](Docs/EGITO_CORRECOES.md). Os testes físicos e em Play verificaram o caminho até a múmia, a ordem dos diálogos, o bloqueio durante a transição, a entrega animada, a recompensa única e o retorno ao exterior com o progresso preservado.
