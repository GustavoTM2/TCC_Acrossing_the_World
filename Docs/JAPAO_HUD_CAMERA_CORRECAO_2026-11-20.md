# Correção do Japão: HUD duplicada e câmera externa

Data: 20/11/2026

## Projeto alterado
`C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main`

## Problema identificado na captura
A imagem da Game View mostrava a nova HUD japonesa sobreposta ao script antigo `ExplorationStage` presente no GameObject `Exploracao_Japao`. Por isso apareciam simultaneamente dois títulos, duas barras de controles e o botão isolado `Voltar ao menu`. O script antigo ainda interceptava Esc, indo diretamente ao menu principal e impedindo a pausa correta.

### Correção definitiva
- Na cena `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity`, **`ExplorationStage.m_Enabled=0`**.
- Em `Assets/TCCExploracao/Runtime/ExplorationStage.cs`, quando `StageExperience.IsCountry` for verdadeiro, o script legado não desenha HUD nem captura Esc. Isso também evita conflitos caso outro país use uma cena antiga.
- `JapanQuest` continua sendo a HUD exclusiva do Japão externo, sem alteração dos diálogos com o guia, das portas ou da conquista.

## Aviso Esc
- Em `Assets/TCCAudio/Runtime/StageExperience.cs`, removido o quadro global `ESC · MENU DA FASE` na parte superior quando o jogo está rodando.
- **Esc continua abrindo o menu de pausa** com Continuar Jogando, Volume, Luminosidade e Voltar ao Menu Principal.
- Nos painéis de instruções inferiores do **Japão, Egito, Brasil e Itália** já há um lembrete `Esc · menu da fase`.
- No minigame de pizzas da Itália, o texto `Esc: menu` aparece na direita da barra verde inferior, tanto na montagem/forno como no corte/embalagem. Os avisos de receita foram redimensionados para não cobrir a dica.

## Câmera externa do Japão
Script: `Assets/TCCJapao/Runtime/JapanExteriorCamera25D.cs`

| Campo | Antes | Depois |
|---|---:|---:|
| Downward Angle (começo) | 64° | **44°** |
| Angle Near Temple | — | **54°** |
| Vertical FOV | 49° | **45°** |
| Height Above Player | 6,5 | **4,8** |
| Distance Behind Player | 4,0 | **5,2** |
| Smooth Time | 0,20 | **0,19** |
| Look Ahead | 0,18 | **0,15** |
| Boundary Safety | 0,75 | **0,55** |

A câmera passa a enquadrar Kenji mais de trás, mostrando o corpo e o caminho à frente, em vez de uma vista quase vertical. Ao se aproximar da porta do templo ela eleva gradualmente o ângulo até 54°, mantendo o personagem visível enquanto respeita a área limitada do mapa. O seguimento `SmoothDamp`, o ajuste para monitores largos e a detecção de paredes por `SphereCastAll` foram preservados.

O componente e a posição inicial da `Camera_Exploracao` foram atualizados na própria cena:
- Posição inicial aproximada: (371,5; 4,84; 961,75).
- Inclinação inicial: 44° voltada ao templo.
- Referências preservadas a Kenji e `Paredes_Invisiveis`.

## Outras fases
- Verificados os avisos de interação e os controles inferiores das HUDs Egito, Brasil e Itália.
- O aviso Esc que aparecia no alto foi removido **globalmente**, não só no Japão.
- O placar do Brasil continua protegido contra sobreposição do berimbau durante o minigame.
- O menu principal continua acessível apenas pela pausa ao pressionar Esc nas fases atuais.

## Arquivos alterados nesta correção
- `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity`
- `Assets/TCCJapao/Runtime/JapanExteriorCamera25D.cs`
- `Assets/TCCExploracao/Runtime/ExplorationStage.cs`
- `Assets/TCCAudio/Runtime/StageExperience.cs`
- `Assets/TCCItalia/Scripts/ItaliaBalcao.cs`

## Testes e limitações
- Auditoria de cena e código: **25/25 verificações estáticas aprovadas**, em `Docs/AUDITORIA_HUD_JAPAO_CORRIGIDA_2026-11-20.txt`.
- Compilação do jogo (Assembly-CSharp): **0 erros e 0 avisos**.
- Compilação dos scripts de Editor: **0 erros**, com **51 avisos de APIs antigas**.
- Preservada a mesma quantidade de objetos serializados na cena antes e após os ajustes (121 blocos).
- **Não foi possível realizar o Play Mode visual completo após as alterações**, porque a janela ativa do computador estava sendo utilizada em outro programa. O código compila, mas ainda é importante observar os extremos do mapa e a posição real da câmera.

### Como aplicar no Editor que já estava aberto
1. Se estiver em Play Mode, pare o Play.
2. Feche e reabra `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity`. Se a cena antiga ainda estiver aberta com alterações não salvas, não salve por cima do arquivo atualizado.
3. Pressione Play e verifique que aparece somente o título `JAPÃO / O SANTUÁRIO DOS ANTIGOS`, apenas um painel de controles inferior e nenhuma indicação Esc no alto.
4. Caminhe até o templo e até as quatro paredes invisíveis; Kenji deve continuar com o corpo visível e a câmera deve acompanhá-lo suavemente.
5. Pressione Esc para verificar o menu único de pausa.

## Backup
`C:\Users\User\Documents\TCC_BACKUP_JAPAO_HUD_CAMERA_2026-11-20_2010`
