# Exploração do Japão e da Itália

Unity 6000.5.7f1 — 05/11/2026.

Japão: Kenji, com a caminhada e textura fornecidas pela equipe, no pátio e entrada do templo. Itália: Sofia, com sua caminhada e textura, na região das pontes existente no projeto.

Use A/D ou setas para os lados e W/S para frente e fundo. Esc ou o botão Voltar ao menu permite escolher outro país. As entradas do menu são EXPLORAR. Não foram adicionados diálogos, desafios ou recompensas nesta etapa.

Cenas: Assets/TCCExploracao/Scenes/Japao_Exploracao.unity e Assets/TCCExploracao/Scenes/Italia_Exploracao.unity. As cenas originais da equipe continuam preservadas.

O movimento reaproveita BrazilPlayer25D com a referência de desafio vazia; a câmera usa EgyptCamera25D, sem acompanhar a profundidade. EgyptMovementBounds cria as quatro paredes invisíveis. MeshCollider permite caminhar no chão e nas pontes e bloqueia as construções. ExplorationStage desenha os avisos e retorna ao menu. Os componentes são comuns às fases para manter a programação simples.

Verificação realizada em Play: movimentos laterais e em profundidade, posições dentro da área delimitada e entrada em ambos os países pelo menu aprovados. A Itália ainda utiliza o cenário base com pontes e chão sem acabamento visual.
