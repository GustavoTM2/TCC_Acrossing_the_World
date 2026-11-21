# Egito — entrada da pirâmide e inversão de W/S

Data: 20/11/2026

## Alterações

### Entrada externa da pirâmide
- A interação **Entrada_Piramide** na cena `Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity` foi reposicionada de `(414, 1.490242, 954.83)` para `(418.5, 1.490242, 954.83)`, mais próxima do corredor por onde Amira chega.
- O raio de interação aumentou de **2.5 para 5.8 unidades** para alcançar a porta mesmo quando existe colisão com a arquitetura, sem depender de encostar no centro geométrico da pirâmide.
- A proximidade das portas `Entrance` e `Exit` agora é calculada no plano horizontal X/Z, sem reprovar porque a personagem está alguns centímetros acima ou abaixo do marcador, em terrenos inclinados.
- A saída do interior foi mantida na mesma posição, mas teve seu raio aumentado de **1.5 para 3.0**, evitando o mesmo tipo de falha no retorno.
- O aviso passou a exibir **E / Enter — Entrar na pirâmide**. O objetivo da fase também explica que é necessário aproximar-se da entrada e pressionar E ou Enter.
- **A conversa com o guia continua obrigatória** antes de entrar; o progresso é mantido normalmente e a entrada continua levando à cena `Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity`.
- As cenas externa e interna foram conferidas na lista do Build Profile.

### Controles no Egito
- Apenas no **Egito**, as teclas W e S foram invertidas: **S** agora faz a movimentação que anteriormente correspondia a **W**, e **W** faz a que correspondia a **S**.
- As setas permanecem convencionais: ↑ para a parte de cima e ↓ para a parte de baixo.
- A e D continuam fazendo deslocamentos laterais.
- A HUD egípcia foi alterada para `S · cima | W · baixo`.
- A alteração vale para **o Egito exterior e interior** porque ambos usam `EgyptPlayer25D`.
- A nova propriedade `EgyptInput.EgyptDepth` isola essa inversão. O Brasil continua usando `EgyptInput.Depth`, sem mudança.

## Arquivos alterados
- `Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity`
- `Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity`
- `Assets/TCCEgito/Runtime/EgyptInput.cs`
- `Assets/TCCEgito/Runtime/EgyptPlayer25D.cs`
- `Assets/TCCEgito/Runtime/EgyptInteractionPoint.cs`
- `Assets/TCCEgito/Runtime/EgyptStageUI.cs`
- `Assets/TCCEgito/Runtime/EgyptStage.cs`

## Auditoria e testes
- **18/18 verificações estáticas** de pontos, raios, cena interna, requisito do guia, E/Enter, controles e ausência de efeito sobre o Brasil passaram.
- Compilação do jogo: **0 erros e 0 avisos**.
- Compilação dos scripts de Editor: **0 erros** e 51 avisos de APIs de Unity obsoletas.
- Ainda falta **testar a caminhada e a transição em Play Mode**, especialmente para confirmar que a colisão do modelo original da pirâmide não bloqueia o caminho até a área ampliada de interação. A verificação estática não confirma esse comportamento em jogo.

## Como conferir
1. Pare o Play e reabra `Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity` no Editor, evitando sobrescrever a cena atualizada com uma versão antiga ainda aberta.
2. Pressione Play, aproxime-se do guia e conclua todas as falas dele.
3. Caminhe até a entrada da pirâmide com **S para o fundo** e W para a frente (posição relativa à câmera). As setas não mudaram.
4. Quando aparecer `E / Enter — Entrar na pirâmide`, pressione E ou Enter. A tela deve escurecer e carregar o interior.
5. Dentro da pirâmide, teste o diálogo com Rami e o retorno pela porta de saída.

## Backup
`C:\Users\User\Documents\TCC_BACKUP_EGITO_ENTRADA_CONTROLES_2026-11-20`

Auditoria: `Docs/AUDITORIA_ENTRADA_EGITO_WS_2026-11-20.txt`
