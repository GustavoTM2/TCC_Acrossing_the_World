# Crossing The World — TCC

Projeto de aventura e exploração em Unity ligado ao turismo cultural. Este primeiro envio registra a base do projeto e o protótipo jogável da fase do Egito.

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

- Duas cenas jogáveis: exterior e interior da pirâmide.
- Movimentação com CharacterController, câmera lateral e profundidade limitada por paredes invisíveis.
- Conversas com o guia e a múmia, objetivos e progressão da missão.
- Recebimento do Velho Escaravelho ao concluir a conversa com a múmia.
- Movimentos sutis de cabeça e braços dos NPCs.
- Textura de areia aplicada ao terreno exterior.

O progresso é mantido durante a sessão e não possui salvamento permanente. A apresentação do cenário após a passagem precisa de revisão; algumas plantas ainda têm texturas ausentes. Amira utiliza a animação de caminhada fornecida e ainda não tem um clip específico de salto.

## Próximas entregas planejadas

- Revisar o cenário e as animações após a passagem no Egito.
- Adicionar transição com escurecimento de tela e melhorar os diálogos.
- Desenvolver a fase brasileira com Lucas e um desafio de dez embaixadinhas consecutivas, usando Espaço no momento certo.

## Versionamento

`Assets`, incluindo os arquivos `.meta`, `Packages` e `ProjectSettings` fazem parte do repositório. Caches, logs e arquivos de compilação do editor são excluídos pelo `.gitignore`. Cada melhoria futura deve ser revisada e testada antes de gerar um novo commit.
