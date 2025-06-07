# Projeto de Processamento de Imagens em Console (.NET)

Este programa de console em C# realiza diversas operações de processamento de imagens, utilizando bibliotecas poderosas para manipulação e análise.

---

## Bibliotecas Utilizadas

O projeto depende das seguintes bibliotecas NuGet:

* **SixLabors.ImageSharp**
* **SixLabors.ImageSharp.PixelFormats**
* **SixLabors.ImageSharp.Processing**
* **SixLabors.ImageSharp.Drawing.Processing**
* **SixLabors.ImageSharp.Advanced**
* **MathNet.Numerics.IntegralTransforms**

Você pode instalar os pacotes via linha de comando:

```bash
  dotnet add package SixLabors.ImageSharp
  dotnet add package MathNet.Numerics
```

---

## Funcionalidades

O programa oferece 4 técnicas diferentes para processamento de imagens, que podem ser escolhidas no menu de console:

1. **Transformação de tons de cinza baseado em clusterização**

    * Aplica uma transformação para converter a imagem em tons de cinza utilizando técnicas de clusterização.

2. **Subtração entre duas imagens**

    * Realça a área de um corpo presente em uma imagem através da subtração pixel a pixel entre duas imagens.

3. **Aplicação do filtro high-boost e comparação com o filtro passa alta**

    * Aplica o filtro high-boost na imagem e compara o resultado com o filtro passa alta para destacar detalhes.

4. **Demonstração do ganho computacional utilizando o Teorema da Convolução**

    * Compara o tempo de processamento entre a aplicação de um filtro por convolução no domínio espacial e no domínio da frequência (FFT).

---

## Como Usar

1. **Executar o programa**
   Ao rodar o programa, será exibido um menu com as opções acima:

   ```
   As técnicas possíveis são:
   (1) - Transformação de tons de cinza baseado em clusterização
   (2) - Subtração entre duas imagens para realçar a área de um corpo presente em uma imagem
   (3) - Aplicação do filtro high-boost e comparação com o filtro passa alta
   (4) - Demonstração do ganho computacional utilizando Teorema da Convolução

   Qual você escolhe?
   ```

2. **Escolha uma opção** digitando o número correspondente e pressionando Enter.

3. **Forneça os paths das imagens**, caso o método escolhido necessite de entradas.

4. **Verifique os resultados** gerados na pasta do programa, que podem incluir imagens processadas e tempos de execução exibidos no console.

---

## Estrutura Recomendada

Organize os arquivos de imagem e o executável na mesma pasta para facilitar o acesso e o uso dos caminhos relativos.

---

## Observações

* Imagens muito grandes podem demorar mais para serem processadas, especialmente ao aplicar FFT.
* Certifique-se de que as imagens estejam em formatos suportados pelo ImageSharp (ex: PNG, JPEG).
* Os resultados são salvos em arquivos de saída para visualização posterior.


## Em relação aos executáveis

Foram gerados dois executáveis, um para arquitetura Linux e outro  para arquitetura Windows, ambos são independentes, ou seja, não precisam de nenhuma configuração prévia para que funcionem, caso queira gerar um novo executável a partir desse código, seguem os comandos:

* Linux
```bash
  dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/linux
```

* Windows
```bash
  dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/windows
```