# Correções da fase do Egito

## Interior da pirâmide

O raycast usado para encontrar o piso atingia primeiro o Tile_B que compõe o teto. Amira, a múmia e a câmera ficavam acima da sala. A busca inicial agora prioriza o piso inferior.

`EgyptInteriorRepair` organiza os módulos da cópia jogável sob `Cenario_Interior_Original`, ajusta a escala para 0,1, posiciona os personagens no piso interno e configura câmera e paredes invisíveis. A estrutura preserva os 62 renderers originais. Os materiais URP usam as texturas do DungeonModularPack; cenas, prefabs e materiais originais permanecem preservados.

O comando **TCC > Corrigir interior da piramide** aplica o reparo em versões anteriores da cena jogável. A cena deste repositório já está corrigida. Reaplicar o comando mantém uma única estrutura de cenário, sem multiplicar a escala.

## Câmera e personagens

`EgyptCamera25D` permite um enquadramento horizontal fixo na sala interna. `EgyptCameraObstruction` usa SphereCastAll para detectar os módulos entre a câmera e Amira e suavizar sua transparência. Seus colliders continuam ativos; os materiais usados na transparência são cópias temporárias, restauradas quando o módulo sai da frente da personagem.

`EgyptNpcIdle` calcula movimentos suaves com Mathf.Sin sobre o rig leve de cabeça e braços. A múmia recebe uma amplitude um pouco maior e gesticula enquanto seu nome corresponde ao falante do diálogo.

## Entrega do escaravelho

`EgyptStage.DeliverScarab` executa uma coroutine de 1,25 segundo. Vector3.Lerp, Mathf.SmoothStep e um arco senoidal levam o artefato até Amira, com rotação e redução de escala. Ao terminar, o objeto visual é ocultado, a missão registra a conquista e os eventos de recompensa e conclusão são disparados uma única vez.

O movimento fica bloqueado durante a entrega. O estado conquistado permanece ao trocar entre as cenas durante a sessão.

## Transição e interface

`EgyptSceneTransition` persiste com DontDestroyOnLoad. Primeiro escurece a tela, depois carrega o destino com SceneManager.LoadSceneAsync. Aguarda a inicialização da nova câmera, abre a imagem gradualmente e libera o controle. Esse fluxo funciona na entrada e na saída da pirâmide.

`EgyptStageUI` reúne os estilos de IMGUI: painéis arredondados, bordas douradas, identificação do falante, contador de falas, botão de continuação e tela de conquista. A interface usa uma referência de 1280 × 720 com escala proporcional e ajusta o tamanho do texto para caber no diálogo.

## Verificações realizadas

- Piso interno, câmera abaixo do teto e 62 módulos visuais com texturas.
- Caminho físico até a múmia e até o ponto de saída.
- Movimento dos vértices da múmia ao variar sua pose.
- Ordem da missão, validação de destino ausente e recompensa concedida uma única vez.
- Em Play: entrada com tela preta, personagem bloqueada durante a transição, entrega em movimento e retorno preservando a conquista.

Os testes utilizaram Unity 6000.6.4f1. A versão 6000.6.0f1 da escola ainda precisa ser conferida. O progresso é mantido durante a sessão; salvamento permanente e animação específica de salto continuam fora desta entrega.
