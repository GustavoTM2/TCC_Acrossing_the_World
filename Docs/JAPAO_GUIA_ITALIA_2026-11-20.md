# Japão — mesmo guia turístico usado na Itália
Data: 20/11/2026

## Alterações concluídas
- Removido da missão o conceito de estátuas falantes na entrada.
- Substituído o NPC inicial pelo **mesmo arquivo FBX** utilizado na cena externa da Itália: `Assets/Scenes/Animações-20260914T104902Z-1-001/Animações/guiaturistico.fbx`.
- Foi criada uma cópia de bytes idênticos do FBX em `Assets/TCCJapao/Resources/GuiaItaliaOriginal.fbx` para carregamento pela Unity durante o jogo. O .meta foi recriado com GUID exclusivo para não conflitar com o guia da Itália. O arquivo original italiano não foi alterado.
- O guia nasce no marcador `Guia turistico - conversar` da fase externa japonesa, perto do caminho de entrada, com altura ajustada e voltado para Kenji.
- As falas de início são de um guia turístico humano e explicam a história do Japão, o papel dos templos e santuários, os samurais e a chegada de Kenji ao templo.
- A entrada no templo só é habilitada após concluir a conversa com o guia.
- Removido o script antigo de estátuas da pasta Runtime; nenhuma referência ao NPC antigo permanece na cena externa ou no código da missão.
- Permanecem inalterados: Kenji, as portas interna/externa, o monge, os diálogos e o legado dos Okoizume, a katana 3D, o HUD de conquista, as trilhas musicais e o menu de pausa Esc.

## Arquivos
- Alterado: `Assets/TCCJapao/Runtime/JapanQuest.cs`
- Alterado: `Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity`
- Alterado: `Assets/TCCJapao/Editor/JapanQuestVerification.cs`
- Criado: `Assets/TCCJapao/Resources/GuiaItaliaOriginal.fbx` e `.meta`
- Removido: `Assets/TCCJapao/Runtime/KomainuGuardian.cs` e `.meta`
- Compilação `Assembly-CSharp.csproj` ajustada para remover a referência ao script excluído.

## Verificações
- O hash SHA-256 do FBX original italiano e de sua cópia para o Japão são iguais.
- 15/15 verificações estáticas dos assets e do fluxo de diálogo concluídas.
- Compilação dos scripts do jogo: 0 erros e 0 avisos.
- Compilação dos scripts do Editor: 0 erros, 49 avisos de APIs existentes.
- Ainda falta **teste visual em Play Mode** para confirmar o material, a orientação exata e a proximidade do guia em relação ao jogador, além de percorrer o diálogo e a entrada.

## Backup anterior
`C:\Users\User\Documents\TCC_BACKUP_JAPAO_GUIA_ITALIA_2026-11-20`
