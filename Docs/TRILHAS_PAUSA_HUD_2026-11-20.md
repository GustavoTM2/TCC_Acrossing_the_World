# Trilha sonora, menu de pausa e identidade visual — 20/11/2026

## Implementação

No projeto final foi criado o sistema global **Assets/TCCAudio/Runtime/StageExperience.cs**. Ele inicia automaticamente ao abrir qualquer cena no Play Mode, identifica o país pela cena ativa, carrega um áudio de Assets/TCCAudio/Resources/Music, toca em loop e faz uma transição suave (~1,4 s) entre trilhas.

As faixas são **composições instrumentais originais geradas para este projeto**, baseadas em escalas, ritmos e timbres que evocam os ambientes. Não utilizam trechos retirados de músicas comerciais; não são gravações de instrumentos reais. Arquivos WAV PCM mono, 22.050 Hz, 16 bits.

## Trilha por ambiente

| Cena/estado | Faixa WAV | Duração do loop | Inspiração musical |
|---|---|---:|---|
| Menu principal | Menu_Viagem.wav | 19,2 s | tema ambiental cinematográfico de viagem |
| Egito exterior | Egito_Deserto.wav | 21,4 s | escala oriental, dedilhados e percussão |
| Egito interior | Egito_Piramide.wav | 24 s | ambiência de templo, notas espaçadas e ressonância |
| Brasil praia | Brasil_Praia.wav | 14,7 s | pulsação rítmica tropical e melodia ensolarada |
| Brasil embaixadinhas | Brasil_Embaixadinhas.wav | 12,2 s | ritmo mais animado para a tarefa |
| Itália rua | Italia_Cidade.wav | 16,6 s | melodia europeia leve com instrumento dedilhado |
| Itália restaurante | Italia_Restaurante.wav | 15,4 s | clima de restaurante e notas delicadas |
| Itália minigame de pizza | Italia_Pizzaria_Desafio.wav | 11,5 s | ritmo acelerado de cozinha/desafio |
| Japão exterior | Japao_Jardim.wav | 22,1 s | escala pentatônica e notas de corda |
| Japão templo | Japao_Templo.wav | 26,2 s | clima contemplativo de templo e sino |

As faixas são selecionadas automaticamente pelo script. A mudança de fase ou de etapa do desafio troca também a música.

## Tecla Esc nas quatro fases

Pressionar **Esc** pausa o jogo e abre o mesmo menu organizado, com acentos de cor específicos do país:

- **Continuar jogando**: fecha o painel e retoma o tempo.
- **Volume**: controle deslizante entre 0% e 100%, usando a mesma preferência do menu principal.
- **Luminosidade**: controle entre 55% e 135% (intensidade de luz/ambiente, com escurecimento adicional nas opções abaixo de 100%).
- **Voltar ao menu principal**: retoma a escala de tempo e carrega a cena do menu com transição.

Esc novamente fecha o painel. A pausa também congela os relógios dos desafios porque usa Time.timeScale = 0. A música ambiente segue reproduzindo suavemente.

## Padrão visual das HUDs

A estrutura foi preservada e harmonizada entre países:

1. **Superior esquerdo**: título do país/local e objetivo.
2. **Superior centro/direita**: lembrete ESC · MENU DA FASE.
3. **Inferior esquerdo**: controles e instruções.
4. **Inferior central**: avisos contextuais ou diálogo.
5. **Superior direito**: recompensa conquistada, quando houver.
6. **Centro**: conquista/desafio ou menu Esc.

Acentos de cor: Egito (dourado/areia), Brasil (verde tropical), Itália (terracota), Japão (rosa-vermelho e creme). A fase japonesa passou a usar quadros escuros retos, borda vermelha e escala de HUD proporcional sem distorção. Os botões soltos "Voltar ao menu" foram removidos das HUDs do Egito, Brasil, Itália e Japão; a saída agora está dentro do menu Esc.

## Testes realizados

- 10 arquivos WAV criados, com cabeçalho RIFF/WAVE válido e amostras não vazias (0 arquivos inválidos).
- 31/31 verificações estáticas de presença de trilhas, cenas, salvamento, interface e bloqueio de HUD em pausa aprovadas.
- Compilação C# runtime: **0 erros, 0 avisos**.
- Compilação dos scripts de Editor: **0 erros**, 47 avisos de APIs deprecadas existentes.
- **Pendente:** audição das faixas e teste manual em Play Mode com as cenas gráficas, para checar volume, integração visual e movimentação/cronômetro após retomar.

## Localização no projeto

- Código: Assets/TCCAudio/Runtime/StageExperience.cs
- Música: Assets/TCCAudio/Resources/Music/*.wav
- Relatório estático: Docs/AUDITORIA_AUDIO_MENU_FASE_2026-11-20.txt
- Backup anterior à alteração: C:\Users\User\Documents\TCC_BACKUP_AUDIO_MENU_FASES_2026-11-20
- Gerador das faixas originais: C:\Users\User\Documents\TCC_GERAR_MUSICAS_ORIGINAIS.ps1

## Como verificar

1. Na Unity 6000.5.7f1, atualize a importação dos Assets caso ainda não apareçam os novos arquivos.
2. Abra Assets/TCCMenu/Scenes/Menu_Principal.unity e pressione Play.
3. Verifique a trilha no menu e abra um país pelo card.
4. Aperte Esc, teste Continuar, volume, luminosidade e Voltar ao menu.
5. No Brasil, abra o desafio de embaixadinhas para ouvir a faixa acelerada.
6. Na Itália, entre no restaurante e inicie a pizza; confirme que o timer congela durante a pausa e retoma depois.
7. Entre no interior da pirâmide e do templo para conferir a música e a transição de cada ambiente.

Os testes de compilação e integridade dos arquivos não substituem esse teste de áudio e interface em tempo real.
