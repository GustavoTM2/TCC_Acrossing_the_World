# Correção: guia turístico não aparecia no Japão externo

O guia era gerado apenas em tempo de execução, enquanto seu marcador possuía o MeshRenderer desligado. Agora o modelo está **serializado diretamente na cena**, para aparecer na Hierarchy antes de apertar Play.

## Mudanças
- Em `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity` foi inserida uma instância real do mesmo FBX `guiaturistico.fbx` usado na Itália (GUID original mantido na referência).
- Nome do objeto na Hierarchy: `Guia_Turistico_Japao_Original`.
- Posição visual do guia e marcador de interação: **(370.1, 0.04, 955.3)**, perto de Kenji. Distância inicial horizontal aproximada: **2.61 unidades**, menor que o raio de interação de 2.8.
- O modelo já é visível no Editor; o script `JapanQuest.cs` reutiliza esta instância ao dar Play, em vez de criar um segundo guia.
- Em tempo de execução, o personagem tem altura e pés normalizados ao marcador, fica voltado para Kenji, recebe renderizadores habilitados e não impede o movimento com colisões.
- Se a instância estiver ausente por uma migração antiga, há fallback para `Resources/GuiaItaliaOriginal.fbx`.
- As falas históricas, o requisito de conversar antes de entrar, a câmera, o monge e a katana não foram alterados.

## Verificação
- 12/12 verificações estáticas de integridade e associação do modelo, posicionamento e interação aprovadas.
- Compilação dos scripts de jogo: 0 erros, 0 avisos.
- Compilação dos scripts de Editor: 0 erros; 50 avisos de APIs antigas.
- Não foi feito Play Mode visual completo, portanto o enquadramento/material requer inspeção na Game View.

## Instruções de conferência
1. Feche e reabra na Unity a cena `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity` para forçar o Editor a ler a cena atualizada.
2. Na Hierarchy procure `Guia_Turistico_Japao_Original`; ele está como objeto raiz, como o modelo original da Itália.
3. Pressione Play e olhe à frente, um pouco à esquerda de Kenji. O aviso `E / Enter — Conversar com o guia turístico` deve aparecer na proximidade.
4. Teste a interação por E/Enter, termine a conversa e siga até a porta do templo.

Backup anterior à edição: `C:\Users\User\Documents\TCC_BACKUP_GUIA_VISIVEL_2026-11-20`.
