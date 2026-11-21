# Revisão do TCC — 20/11/2026

Projeto alterado: Downloads/TCC_Acrossing_the_World-main/TCC_Acrossing_the_World-main

## Alterações

- Menu principal: não apaga mais conquistas ao retornar de uma fase; mostra o status de conclusão do Egito, Brasil, Itália e Japão.
- Progresso da sessão: adicionados ItalyPrize, JapanGuide, JapanPrize e JapanFarewell. Uma execução nova começa sem progresso antigo.
- Japão: duas cenas em Assets/TCCJapao/Scenes (JAPAO_EXTERNO e JAPAO_INTERIOR); diálogo com guia, entrada do templo, diálogo com monge, conquista da katana e conversa final obrigatória antes de sair; desbloqueios mantidos ao atravessar as cenas; menu aponta para esta fase.
- Japão: ao voltar do interior, Kenji reaparece próximo à entrada; jogador não se move durante diálogo e tela de conquista; NPCs usam prefabs animados em Assets/TCCJapao/Resources. A katana é um modelo geométrico provisório.
- Egito: gravidade continua sendo aplicada quando o movimento é bloqueado por diálogo/recompensa; W/S corrigidos para cima/baixo relativamente à câmera.
- Brasil: Esc na exploração retorna ao menu; usa a mesma entrada de W/S do Egito.
- Itália: conquista da Ferrari F40 persiste no menu; Esc retorna ao menu inclusive durante minigame; W/S corrigidos para cima/baixo relativamente à câmera; tempo e nota mínima existentes preservados.
- Controles: E, Enter e Enter numérico nas interações (novo e antigo Input System).
- Animações: preservados os estados iniciais idle de Kenji, Amira, Lucas e Sofia e transições configuradas para caminhada.

## Verificações realizadas

- Auditoria anterior: 8 cenas estruturais abertas, 5 controladores Animator com idle padrão e 0 erros de referência na verificação automatizada.
- Compilação C# runtime do projeto final: 0 erros, 0 avisos (Unity 6000.5.7f1).
- Compilação das ferramentas do Editor: 0 erros, 42 avisos de APIs deprecadas (não bloqueadores).
- Validação prática integral de todas as missões e apresentação gráfica ainda requer playtest na Unity; a auditoria estrutural não substitui a jogabilidade.
- Uma cópia independente de QA foi criada para testes: Documents/TCC_QA_2026_11_20.

## Backup

Documents/TCC_backup_revisao_todas_fases_2026-11-20
Documents/TCC_FINAL_backup_antes_integracao_Japao_2026-11-20

## Observação

O interior do Japão usa elementos de protótipo (solo auxiliar e katana geométrica) criados pela integração; convém substituir pela arte final e posicionar esses elementos conforme a decoração real do templo após inspeção visual.
