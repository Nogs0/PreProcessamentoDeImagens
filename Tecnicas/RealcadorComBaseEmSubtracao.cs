using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace TecnicasPreProcessamentoDeImagens;

public class RealcadorComBaseEmSubtracao
{
    public static void ExecutarMetodoDeRealce()
    {        
        string outputPath = "2_corpoRealcado.png";
        Console.WriteLine("Forneça primeiro a imagem do fundo:");
        string? imagemFundoPath = Console.ReadLine();
        if (string.IsNullOrEmpty(imagemFundoPath))
            Console.WriteLine("Não foi possível identificar a imagem...");
        
        Console.WriteLine("Agora a imagem com o corpo:");
        string? imagemCorpoPath = Console.ReadLine();
        if (string.IsNullOrEmpty(imagemCorpoPath))
            Console.WriteLine("Não foi possível identificar a imagem...");

        using var outputImage = RealcarObjeto(imagemFundoPath, imagemCorpoPath);
        outputImage.Save(outputPath);
        Console.WriteLine($"Imagem processada salva em: {outputPath}");
    }

    private static Image RealcarObjeto(string imagemFundoPath, string imagemCorpoPath)
    {
        using var imagemFundoCinza = Image.Load<L8>(Directory.GetCurrentDirectory() + imagemFundoPath);
        using var imagemCorpoCinza = Image.Load<L8>(Directory.GetCurrentDirectory() + imagemCorpoPath);
        
        var width = imagemCorpoCinza.Width;
        var height = imagemCorpoCinza.Height;
        
        var matrizBinaria = new bool[height, width];
        byte limiar = 107;
        
        for (int y = 0; y < height; y++)
        {
            //Pegando a linha da matriz como um Span
            var fundoRow = imagemFundoCinza.DangerousGetPixelRowMemory(y).Span;
            var corpoRow = imagemCorpoCinza.DangerousGetPixelRowMemory(y).Span;

            for (int x = 0; x < width; x++)
            {
                /* PackedValue representa o valor do pixel como um único número, ideal para comparações,
                especialmente ao usar uma imagem em tons de cinza, onde representa a intensidade do cinza,
                para comparações utilizando uma imagem RGB o PackedValue pode não representar um resultado intuitivo
                */
                // Executando a subtração
                int diff = Math.Abs(fundoRow[x].PackedValue - corpoRow[x].PackedValue);
                matrizBinaria[y, x] = diff > limiar;
            }
        }

        // Aqui temos como objetivo encontrar o menor retângulo possível para delimitar o corpo na imagem
        int minX = width, minY = height, maxX = 0, maxY = 0;
        bool existeCorpo = false;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Caso seja encontrado algum valor, quer dizer que aquele é o limite máximo para plotar o retângulo
                // Para isso atualizamos os valores, com objetivo de que ao final do percurso
                if (matrizBinaria[y, x])
                {
                    existeCorpo = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // Recarregar a imagem original para desenhar nela
        Image outputImage = Image.Load<Rgba32>(Directory.GetCurrentDirectory() + imagemCorpoPath);

        if (existeCorpo)
        {
            var rect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            outputImage.Mutate(ctx => ctx.Draw(Pens.Solid(Color.Red, 5), rect));
        }

        return outputImage;
    }
}