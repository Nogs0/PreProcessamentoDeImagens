using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TecnicasPreProcessamentoDeImagens.Tecnicas;

public class ConversorParaTonsDeCinza
{
    public static void ExecutarMetodoDeConversao()
    {
        Console.WriteLine("Insira o caminho da imagem:"); 
        string? imagePath = Console.ReadLine();
        if (string.IsNullOrEmpty(imagePath))
        {
            Console.WriteLine("Nenhum imagem foi encontrada...");
            return;
        }

        using var imagemCinza = Converter(imagePath);
        string outputPath = "1_convertidaEmCinza.png";
        imagemCinza.Save(outputPath);
        Console.WriteLine($"Imagem processada salva em: {outputPath}");
    }
    
    private static Image<Rgba32> Converter(string imagePath)
    {
        Image<Rgba32> image = Image.Load<Rgba32>(imagePath);
        image.Mutate(ctx =>
        {
            ctx.Grayscale(); // converte para tons de cinza
        });

        image.ProcessPixelRows(accessor =>
        {
            // accessor permite acessar a imagem linha por linha
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];

                    // A imagem já foi convertida para tons de cinza, R = G = B
                    // Por isso se pegarmos o R, acaba representando o tom cinza
                    byte pixelCinza = pixel.R;

                    // Agrupamento por bloco de 4 (0–3, 4–7, ...)
                    int tamanhoDoAgrupamento = 4;
                    int novoCinza0a63 = pixelCinza / tamanhoDoAgrupamento;
                    int novoCinzaNormalizado = novoCinza0a63 * tamanhoDoAgrupamento;
                    byte byteCinzaNormalizado = (byte)novoCinzaNormalizado;

                    row[x] = new Rgba32(byteCinzaNormalizado, byteCinzaNormalizado, byteCinzaNormalizado);
                }
            }
        });

        return image;
    }
}