namespace CrossingTheWorld.TCCEgypt
{
    // Falas do Egito fornecidas pela equipe em Falas_Horizon_Travel.docx.
    public static class EgyptDialogueContent
    {
        public const string GuideSpeaker = "Agente da Horizon Travel";
        public const string MummySpeaker = "Rami";

        // Cada caixa do roteiro vira uma fala, na mesma ordem do documento.
        public static EgyptLine[] CreateGuide() => new[]
        {
            new EgyptLine("Agente da Horizon Travel", "Bem-vinda ao Egito, Amira! Sou da Horizon Travel e vou te guiar por aqui."),
            new EgyptLine("Amira", "Valeu! Pra onde a gente vai primeiro?"),
            new EgyptLine("Agente da Horizon Travel", "Tá vendo aquela pirâmide? Tem uma múmia lá dentro esperando sua visita!"),
            new EgyptLine("Amira", "Uma múmia? Essa conversa vai ser interessante!"),
            new EgyptLine("Agente da Horizon Travel", "Pois é! Vá até a pirâmide e fale com ele. Ele tem algo pra te contar!"),
        };

        // Cada caixa do roteiro vira uma fala, na mesma ordem do documento.
        public static EgyptLine[] CreateMummy() => new[]
        {
            new EgyptLine("Rami", "Uma visitante! Pode chegar, Amira. Eu sou Rami! Não precisa ter medo dessas faixas… O mais assustador por aqui é ficar tanto tempo sem ninguém pra conversar!"),
            new EgyptLine("Rami", "Nasci há mais de três mil anos, perto do rio Nilo. Meu pai era escriba: trabalhava escrevendo e fazendo registros. Eu vivia pedindo que ele me ensinasse. Queria guardar todas as histórias que ouvia!"),
            new EgyptLine("Rami", "Naquela época, o Nilo enchia todos os anos. Quando a água baixava, deixava uma camada de terra fértil para plantar. Eu adorava ver as margens ficando verdes outra vez. Nossa vida seguia o ritmo do rio!"),
            new EgyptLine("Rami", "Meu pai me ensinou que os hieróglifos não eram só desenhos bonitos. Eles podiam representar sons, palavras e ajudar a entender o significado do que estava escrito. Imagina descobrir que um passarinho desenhado faz parte de uma mensagem!"),
            new EgyptLine("Rami", "E sabe uma coisa engraçada? Quando eu nasci, as grandes pirâmides de Gizé já tinham mais de mil anos! Elas foram construídas como túmulos de reis. Eu também olhava pra elas e pensava: “Como conseguiram fazer tudo isso?”"),
            new EgyptLine("Rami", "A Grande Pirâmide tinha um revestimento de calcário claro e liso. Era bem diferente das pedras expostas que você vê hoje! Algumas dessas pedras de revestimento chegaram de barco. Imagina o trabalho para transportar tudo!"),
            new EgyptLine("Rami", "Muito tempo depois, quando minha vida terminou, meu corpo foi mumificado. Nós acreditávamos numa vida após a morte, e preservar o corpo fazia parte dessa preparação. Um processo completo podia levar cerca de setenta dias. Haja paciência!"),
            new EgyptLine("Rami", "As faixas não faziam tudo sozinhas! Usavam natrão, uma mistura natural de sais, para secar o corpo, e depois vinha o linho. Também colocavam amuletos entre as camadas, porque acreditávamos que eles ofereciam proteção."),
            new EgyptLine("Rami", "O meu amuleto tinha um pedido especial: que minhas histórias nunca se perdessem. No meu caso, a magia funcionou até demais! Acordei, saí do meu antigo túmulo e vim para esta pirâmide contar o que lembrava."),
            new EgyptLine("Rami", "Hoje eu guardo essas lembranças, Amira. Porque o Egito também era feito de famílias, plantações e gente curiosa como você. Quando sair daqui, conte alguma coisa que aprendeu. Assim, um pedacinho da nossa história vai junto!"),
            new EgyptLine("Rami", "Antes de ir, Amira, leve este velho amuleto de escaravelho! É meu presente pra você. Cuide bem dele e lembre das nossas histórias!"),
        };
    }
}
