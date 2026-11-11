# Arquitetura e roteiro técnico

## Organização

| Diretório | Responsabilidade |
|---|---|
| `Assets/TCCMenu` | Menu, seleção de destinos e progresso |
| `Assets/TCCEgito` | Exploração, diálogos, câmera e transição do Egito |
| `Assets/TCCBrasil` | Exploração da praia, interface e desafio |
| `Assets/TCCExploracao` | Exploração do Japão e da Itália |
| `Packages` | Dependências e versões do projeto |
| `ProjectSettings` | Configuração do editor, entrada e cenas |

Os módulos TCC separam scripts de execução em `Runtime` e ferramentas do editor em `Editor`. As cenas originais e os assets permanecem nas pastas existentes.

## Fluxo principal

1. A cena `Assets/TCCMenu/Scenes/Menu_Principal.unity` abre o menu.
2. [TravelMenu](../Assets/TCCMenu/Runtime/TravelMenu.cs) seleciona um dos quatro destinos e solicita a transição.
3. Brasil e Egito usam [TravelProgress](../Assets/TCCMenu/Runtime/TravelProgress.cs) para registrar estado local.
4. Japão e Itália usam [ExplorationStage](../Assets/TCCExploracao/Runtime/ExplorationStage.cs) para apresentar instruções e retornar ao menu.
5. Ao carregar o menu, `TravelMenu.Start` chama `TravelProgress.Reset`. Uma nova execução também reinicia o progresso.

## Pontos para leitura do código

- [BrazilTimingChallenge](../Assets/TCCBrasil/Runtime/BrazilTimingChallenge.cs): classe de regras separada da interface, com meta de dez acertos, janela variável e semente opcional em `Begin`.
- [TravelMenu](../Assets/TCCMenu/Runtime/TravelMenu.cs): seleção circular, entrada por teclado e clique e verificação de disponibilidade da cena.
- [TravelProgress](../Assets/TCCMenu/Runtime/TravelProgress.cs): gravação, consulta e limpeza de chaves PlayerPrefs.
- [ExplorationStage](../Assets/TCCExploracao/Runtime/ExplorationStage.cs): retorno ao menu por Esc ou botão.

A existência desses componentes não estabelece quem os implementou individualmente.

## Reprodução e validação

A versão registrada em `ProjectSettings/ProjectVersion.txt` é **6000.5.7f1**. As versões dos pacotes estão em [manifest.json](../Packages/manifest.json). A lista de cenas está em [EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset).

Roteiro sugerido para a próxima validação manual, ainda não executado nesta revisão:

- Abrir o menu e visitar os quatro destinos.
- Concluir o desafio do Brasil e a missão do Egito.
- Verificar a manutenção do progresso entre exterior e interior do Egito.
- Retornar ao menu e confirmar o reinício do progresso.
- Registrar versão do editor, resultado, erros e capturas reais.

Os testes históricos estão nos documentos de cada módulo. Não foram medidos desempenho, cobertura de testes ou segurança nesta revisão.

