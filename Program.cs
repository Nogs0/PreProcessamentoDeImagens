using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TecnicasPreProcessamentoDeImagens;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("As técnicas possíveis são:");
        Console.WriteLine("(1) - Transformação de tons de cinza baseado em clusterização");
        Console.WriteLine("(2) - Subtração entre duas imagens para realçar a área de um corpo presente em uma imagem");
        Console.WriteLine("(3) - Aplicação do filtro high-boost e comparação com o filtro passa alta");
        Console.WriteLine("(4) - Demonstração do ganho computacional utilizando Teorema da Convolução");
        Console.WriteLine();
        Console.WriteLine("Qual você escolhe?");
        string? tecnica = Console.ReadLine();
        Console.WriteLine("Opções: 1 (sim) / 0 (não)");
        switch (tecnica)
        {
            case "1":
                Console.WriteLine(
                    "Você optou pela transformação de tons de cinza baseado em clusterização, deseja prosseguir?");
                break;
            case "2":
                Console.WriteLine(
                    "Você optou pela subtração entre duas imagens para realçar a área de um corpo presente em uma imagem, deseja prosseguir?");
                break;
            case "3":
                Console.WriteLine(
                    "Você optou pela aplicação do filtro high-boost e comparação com o filtro passa alta, deseja prosseguir?");
                break;
            case "4":
                Console.WriteLine(
                    "Você optou pela demonstração do ganho computacional utilizando Teorema da Convolução, deseja prosseguir?");
                break;
            default:
                Console.WriteLine("Opção inválida...");
                return;
        }

        string? opcao = Console.ReadLine();
        if (opcao != "1")
        {
            Console.WriteLine("Bom, adeus...");
            return;
        }

        switch (tecnica)
        {
            case "1":
                ConversorParaTonsDeCinza.ExecutarMetodoDeConversao();
                break;
            case "2":
                RealcadorComBaseEmSubtracao.ExecutarMetodoDeRealce();
                break;
            case "3":
                AplicadorFiltroHighBoost.ExecutarComparacaoDeResultados();
                break;
            case "4":
                DemonstradorTeoremaDaConvolucao.ExecutarDemonstracaoDeGanho();
                break;
            default:
                Console.WriteLine("Opção inválida...");
                return;
        }
    }
}