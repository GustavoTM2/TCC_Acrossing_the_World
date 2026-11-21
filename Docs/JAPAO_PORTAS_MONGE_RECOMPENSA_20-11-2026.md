# Japão — portas, monge, nascimento e katana 3D
Data: 20/11/2026

## Projeto alterado
C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main

## Ajustes nas duas cenas

### Exterior — Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity
- Kenji continua no início do caminho em (371.5, 0.04, 957.5), virado em direção ao templo.
- Marcador invisível **Entrada do templo - interagir** ajustado para (371.5, 1.25, 942.0), centralizado no corredor da fachada do templo japonês.
- A entrada continua dependendo da conversa com o leão guardião komainu.
- Quando Kenji volta do interior, reaparece do lado externo da porta, orientado para fora.
- A cena original do jardim/templo, seus modelos, materiais, NPC e HUD foram preservados.

### Interior — Assets/TCCJapao/Scenes/JAPAO_INTERIOR.unity
- Marcador invisível **Saida do templo**: (0.0, 1.25, 8.6), na região da porta frontal do BuddhaHall.
- Kenji nasce dentro do templo junto à porta: (0.0, 0.12, 6.7).
- Kenji nasce voltado para o fundo do salão, em direção ao monge, e em idle.
- **Monge - conversar**: (2.15, 0, -2.4), à direita do corredor interno, para ficar visível e acessível.
- **Katana - recompensa**: (-2.2, 1.3, -3.25), à esquerda da área do monge.
- As posições foram ajustadas a partir do layout/transformações salvos na cena. **Não foram confirmadas por um Play Mode visual**; se os limites exatos da malha do templo forem diferentes dos presumidos, pode haver necessidade de pequeno ajuste fino.

## Fluxo da katana como no Egito
1. A katana FBX fornecida (com textura original) fica visível no cenário, exposta em um pedestal.
2. Kenji interage com o monge e conclui o diálogo sobre a história do Japão e o legado fictício Okoizume.
3. A katana se move do pedestal até Kenji em uma animação suave de entrega, desaparece do cenário ao concluir.
4. A tela de **CONQUISTA DESBLOQUEADA** é apresentada, com o modelo 3D real da Katana Cerimonial Okoizume.
5. Depois de pressionar **Continuar explorando**, a katana é exibida girando permanentemente no canto superior direito, com o nome da conquista.
6. A rotação na interface agora utiliza uma câmera ortográfica isolada, RenderTexture e solicitação explícita do pipeline URP, da mesma forma que o escaravelho no Egito.
7. A conquista é salva e permanece após trocar de cena.
8. Kenji deve concluir a conversa final com o monge antes de poder interagir com a saída.

## Arquivos alterados
- Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity
- Assets/TCCJapao/Scenes/JAPAO_INTERIOR.unity
- Assets/TCCJapao/Runtime/JapanQuest.cs
- Assets/TCCJapao/Editor/JapanQuestVerification.cs
- Removido o código temporário de inspeção de geometria que não foi executado.

## Verificações
- 20/20 verificações estáticas de cena, posições, estado da katana, animação de entrega, câmera 3D, HUD, progressão e saída aprovadas.
- Compilação C# runtime: **0 erros, 0 avisos**.
- Compilação de ferramentas do Editor: **0 erros, 49 avisos de compatibilidade de APIs**.
- A integridade estrutural foi verificada, mas **não foi possível executar o percurso completo no Play Mode** para confirmar enquadramento de câmera, colisões e alinhamento milimétrico das portas.

## Como testar
1. Abra a cena **Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity**. Inicie Play.
2. Confirme Kenji virado para o templo, converse com o komainu e chegue à entrada, pressione E ou Enter.
3. Na cena interna, Kenji deve aparecer logo após a porta, olhando na direção do monge à direita.
4. Antes de falar com o monge, verifique a katana sobre o pedestal ao lado oposto.
5. Termine o diálogo. A espada deve ir até Kenji, sumir do pedestal e abrir a tela de conquista.
6. Pressione Continuar explorando e confirme a katana girando no canto superior direito.
7. Fale com o monge novamente para finalizar a missão. Depois volte pela porta interna para a cena externa.
8. Pressione Esc para confirmar que o menu da fase funciona.

## Backup
C:\Users\User\Documents\TCC_BACKUP_JAPAO_PORTAS_E_KATANA_2026-11-20

## Relatório técnico
Docs/JAPAO_PORTAS_RECOMPENSA_AUDITORIA_2026-11-20.txt
