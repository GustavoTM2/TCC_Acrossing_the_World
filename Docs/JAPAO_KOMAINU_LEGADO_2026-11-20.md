# Japão — missão Komainu e legado dos Okoizume
Atualização: 20/11/2026

## Projeto alterado
C:\Users\User\Downloads\TCC_Acrossing_the_World-main\TCC_Acrossing_the_World-main

## Estrutura da missão (inspirada no Egito)
1. Exterior: Kenji explora a área original do Japão e se aproxima do par de **komainu**, guardiões da entrada.
2. O komainu de boca aberta fala com Kenji, introduz a história do Japão, explica diferenças entre santuários xintoístas e templos budistas, os samurais e a simbologia das estátuas. A segunda estátua, de boca fechada, acompanha a cena.
3. **Somente depois da conversa com o komainu** é permitido entrar no templo.
4. Interior: Kenji se aproxima do monge original fornecido em FBX e conversa sobre a história japonesa e a lenda fictícia do clã samurai Okoizume.
5. O monge revela a razão para confiar a **Katana Cerimonial Okoizume** a Kenji, descendente do clã: preservar a memória, demonstrar respeito, paz, responsabilidade e sabedoria. A katana vem do FBX e textura fornecidos, não mais da geometria provisória.
6. O jogo apresenta a entrega da espada com animação, tela de conquista e visualização 3D giratória.
7. Kenji deve **voltar ao monge e completar a conversa final** sobre o legado dos Okoizume e o incentivo ao turismo cultural no Japão.
8. Só após a despedida a porta de saída é liberada, levando novamente ao exterior.

## Arte restaurada
- Exterior conserva o cenário original japonês e os materiais.
- Interior conserva o salão original BuddhaHall e suas luzes; colisões se aplicam aos objetos.
- Modelo real do monge: Assets/TCCJapao/Resources/JapanModels/MongeOkoizume.fbx
- Textura do monge: Assets/TCCJapao/Resources/JapanModels/MongeOkoizume.png
- Modelo real da katana: Assets/TCCJapao/Resources/JapanModels/KatanaOkoizume.fbx
- Textura da katana: Assets/TCCJapao/Resources/JapanModels/KatanaOkoizume.png
- Duas estátuas estilizadas de komainu surgem automaticamente ao entrar em Play no cenário externo. Elas usam um modelo original criado por código enquanto o asset licenciado da Unity Asset Store não está importado.
- A estátua falante tem efeito de luz sutil durante a conversa, sem áudio de voz gravado.

## Asset Store: substituição opcional pelo modelo final
- **A estátua komainu comercial não foi comprada nem baixada**: a Asset Store exige acesso à conta/licença de aquisição.
- Opção correspondente: Japanese Town - Low Poly Kit (tem o par Komainu; foi listado por US$ 19,99 em 20/11/2026):
  https://assetstore.unity.com/packages/3d/environments/historic/japanese-town-low-poly-kit-412024
- Outra opção (modelo mais detalhado e mais pesado): Komainu Wooden Decoration for Shrine 8K:
  https://assetstore.unity.com/packages/3d/props/exterior/komainu-wooden-decoration-for-shrine-8k-286751
- Se desejar a aparência fiel do asset da loja, instale um desses pacotes na Unity com a licença adequada, selecione o modelo INDIVIDUAL de komainu e crie o prefab **Assets/TCCJapao/Resources/JapanModels/Komainu.prefab**. O script KomainuGuardian.cs irá utilizá-lo automaticamente no lugar das estátuas estilizadas. Ele mantém as interações e diálogos independentemente da origem do modelo.
- Não use uma cópia não autorizada ou um link que não contenha licença válida.

## HUD e controles
- HUD do Japão organizado como o Egito: título/objetivo no alto à esquerda, interações embaixo, caixa grande de diálogo com nome e contador, recompensa 3D no alto à direita.
- W A S D: movimentar Kenji; E / Enter: interagir, avançar falas e aceitar conquista.
- Esc: abrir menu de pausa com Continuar, Volume, Luminosidade e Voltar ao menu principal.
- A conquista fica salva no TravelProgress, inclusive ao voltar do templo.

## Arquivos editados
- Assets/TCCJapao/Runtime/JapanQuest.cs
- Assets/TCCJapao/Runtime/KomainuGuardian.cs (novo)
- Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity (estátua e ponto de interação da entrada)
- Assets/TCCJapao/Editor/JapanQuestVerification.cs
- O restante das cenas e modelos originais permaneceu preservado.

## Verificações
- 20/20 verificações estáticas da integração do komainu, diálogos, monge, katana, portas e cena original.
- Compilação do jogo: 0 erros e 0 avisos.
- Compilação do Editor: 0 erros; 47 avisos de APIs anteriores não bloqueadores.
- **A inspeção visual e o playtest completos não foram realizados.** É necessário abrir o menu em Play na Unity e conferir se as estátuas estão na altura adequada do terreno, a cena mostra o monge e a espada com as texturas corretamente, a aproximação ativa E/Enter, a transição funciona, o diálogo final libera a saída e o menu Esc pausa a missão.
- O guardião "fala" por texto de diálogo; não existe dublagem/síntese de voz.

## Backup
C:\Users\User\Documents\TCC_JAPAO_BACKUP_KOMAINU_2026-11-20

## Contexto cultural
Os komainu são guardiões tradicionalmente exibidos em pares; um costuma ter a boca aberta e o outro fechada. A distinção de templos budistas e santuários xintoístas e a história geral dos samurais são tópicos históricos. Já o clã samurai Okoizume e o enredo da espada são ficção do jogo, não apresentados como clã historicamente documentado.
