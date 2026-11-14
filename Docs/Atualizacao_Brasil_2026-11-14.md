# Atualização do cenário do Brasil — 14/11/2026

Cena jogável: `Assets/TCCBrasil/Scenes/Brasil_Praia_Jogavel.unity`. Unity 6000.5.7f1.

Integrado o cenário recebido nos dois ZIPs Assets de 14/11: decoração da praia, figurantes LowPolyPeople e dois terrenos no fundo. A cena existente foi atualizada; não foi criada uma segunda fase. Foram preservados os scripts, a câmera, os controles, as barreiras, os diálogos, o desafio e o prêmio.

Os arquivos recebidos referenciavam um céu, um material de terreno e texturas Cliff/Mossy Rock ausentes. Foi mantido o céu anterior, aplicado o material URP Terrain/Lit existente aos terrenos e usadas as texturas Rock e Grass_Moss incluídas no próprio pacote como substitutas. Portanto, o acabamento das montanhas pode diferir da cópia do autor.

Packages e ProjectSettings foram comparados, sem substituir as configurações locais. Terrain Tools não foi adicionado: não é necessário para executar o mapa.

Validação: fluxo de diálogos, erros/acertos do minigame, dez embaixadinhas, prêmio único, continuar, apoio no chão e quatro limites de movimentação. Relatórios e capturas ficam fora do repositório, em `brasil-mapa-2026-11-14-validacao-final`, na pasta de trabalho tcc.

Backup dos arquivos substituídos: `backups/brasil-2026-11-14`, na pasta de trabalho tcc.
