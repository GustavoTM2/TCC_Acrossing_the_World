# Japão — arte original restaurada no TCC final

Atualização: 20/11/2026.

## Fonte consultada

Projeto original do Japão:
`C:\Users\User\Desktop\TCC_Acrossing_the_World_JAPAO\TCC_Acrossing_the_World-main`

Projeto final atualizado:
`C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main`

## Assets originais conferidos

Os materiais, modelos e texturas originais já estavam presentes no TCC final, por isso **não foram copiados em duplicidade nem tiveram os GUIDs trocados**. A conferência encontrou:

- `Assets/Japao`: 237 arquivos, nenhum faltando. Inclui templo japonês, torii, paredes, lanternas, casas, placas e texturas.
- `Assets/BuddhaHall`: 44 arquivos, nenhum faltando. Inclui o salão original, estátuas, decoração, iluminação e materiais.
- `Assets/TCCExploracao/Art/Kenji`: 4 arquivos, nenhum faltando.
- `Assets/TCCExploracao/Generated`: 18 arquivos, nenhum faltando.

As três cenas de referência original foram mantidas **exatamente iguais**, sem sobrescrever:
`Assets/Scenes/Japao.unity`,
`Assets/Scenes/Japao (interior) 1.unity` e
`Assets/TCCExploracao/Scenes/Japao_Exploracao.unity`.

## Cenas jogáveis restauradas

**Exterior:** `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity`

- Conserva todas as instâncias de modelos japoneses, posições e materiais da cena original.
- Conserva Kenji e a câmera de exploração.
- Oculta visualmente os marcadores temporários de guia e entrada sem remover sua função de interação.
- O ponto de entrada foi alinhado ao caminho em direção ao templo original (x 371,5; z 942), em vez de ficar lateralmente deslocado (x 362,5; z 957,5).
- O guia continua aparecendo usando um prefab de personagem animado.

**Interior:** `Assets/TCCJapao/Scenes/JAPAO_INTERIOR.unity`

- Conserva o modelo FBX original `BuddhaHall` e as luzes da cena fonte.
- Oculta o piso cinza provisório, mas mantém seu `BoxCollider` invisível para evitar quedas.
- Adiciona colisão aos meshes da arquitetura e dos objetos de cena, sem trocar suas malhas visuais.
- Ativa `Read/Write` na importação de `Assets/BuddhaHall/fbx/BuddhaHall.fbx.meta`, necessário para maior compatibilidade da construção dinâmica de colisores.
- Oculta os marcadores geométricos provisórios do monge e da saída.
- Reposiciona o marcador de saída para a região frontal do templo (x 0; z 8,6).
- Mantém Kenji, monge animado, diálogos, câmera jogável, progressão de missão, tela de recompensa, retorno à fase externa, HUD, áudio e menu Esc.

## Observação importante sobre a katana

No projeto original do Desktop **não foi encontrado um modelo 3D separado de katana em FBX/OBJ/prefab**. Portanto, não seria correto afirmar que o modelo específico da espada foi recuperado. O prêmio existente na cena jogável `TCCJapao` continua sendo uma composição geométrica 3D provisória, com a lógica da conquista funcionando no script. Para reproduzir uma espada específica exatamente como antes, será necessário que o modelo original seja fornecido ou localizado em outro arquivo.

## Verificação e limitações

- Conferência de inventário: 303 arquivos originais conferidos, nenhum ausente.
- Referência ao templo exterior e ao BuddhaHall interior confirmadas nas cenas jogáveis.
- Compilação do Assembly-CSharp: 0 erros, 0 avisos.
- Compilação do Assembly-CSharp-Editor: 0 erros; avisos de APIs antigas não bloqueadores.
- **Ainda não foi feito um playtest visual completo** de aproximação às portas, posição do personagem, colisão e diálogo com o monge. Os pontos de interação foram realinhados usando os dados geométricos das cenas, mas merecem conferência jogando.

## Backup antes desta restauração

`C:\Users\User\Documents\TCC_JAPAO_BACKUP_ANTES_RESTAURAR_ARTE_2026-11-20`

Este backup inclui as cenas, scripts e o metadado original do FBX `BuddhaHall`.
