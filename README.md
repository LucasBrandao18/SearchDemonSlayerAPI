# SearchDemonSlayerAPI

Aplicação **Blazor WebAssembly em C# e .NET 10**, desenvolvida a partir do material `lista demon slayer.pdf`, do professor Daniel Henrique Matos de Paiva.

## O que foi solicitado

- Criar um aplicativo Blazor WebAssembly em .NET 10 ou superior.
- Consumir o endpoint paginado de personagens de Demon Slayer.
- Permitir listar ou buscar personagens, como Tanjiro Kamado, Nezuko Kamado e Giyu Tomioka.
- Mostrar os campos `name`, `age`, `race`, `quote` e `img`.
- Publicar no GitHub e entregar o link no Google Classroom.

A implementação permite **listar e buscar**, com navegação entre páginas. A publicação no GitHub e a entrega no Google Classroom serão feitas manualmente pelo aluno, conforme solicitado. Nenhum envio foi realizado.

## Requisitos

- SDK do **.NET 10** instalado (`dotnet --version`).
- Navegador atualizado.
- Conexão com a internet para restaurar os pacotes NuGet, consultar a API e carregar as imagens.

Não é necessária chave de API nem banco de dados.

## Como executar

No PowerShell:

```powershell
cd "C:\Users\lucas\OneDrive\Documentos\vs code uni\homework-SearchDemonSlayerAPI"
dotnet restore
dotnet run
```

Abra [http://localhost:5229](http://localhost:5229). A primeira página é carregada automaticamente.

- Use **Próxima** e **Anterior** para navegar pela lista.
- Digite um nome e clique em **Buscar**, ou pressione **Enter**.
- A busca aceita nomes parciais: por exemplo, `kamado`.
- Clique em **Listar todos** para limpar a busca e voltar à primeira página.
- Buscar com o campo vazio também retorna à primeira página.
- Se ocorrer uma falha, use **Tentar novamente** para repetir a consulta.

Para encerrar o servidor, pressione `Ctrl+C` no terminal.

No VS Code, abra a pasta raiz e use **Terminal > Executar Tarefa > executar SearchDemonSlayerAPI**. Também há uma tarefa de compilação em `.vscode/tasks.json`.

Se a porta estiver ocupada, encerre uma execução anterior ou use outra porta:

```powershell
dotnet run --urls http://localhost:5230
```

Nesse caso, abra a porta 5230.

## API utilizada

Listagem paginada:

```text
GET https://www.demonslayer-api.com/api/v1/characters?page=1
```

Busca por nome:

```text
GET https://www.demonslayer-api.com/api/v1/characters?name=nezuko
```

A busca ocorre no servidor, e não apenas nos personagens da página atual. O nome é codificado com `Uri.EscapeDataString` para preservar espaços e caracteres especiais na URL.

A API retorna os personagens no array `content`. Na listagem, o objeto `pagination` informa a página atual, o total de páginas e o total de personagens. A resposta da busca por nome não precisa conter `pagination`.

A aplicação utiliza a quantidade e os dados retornados pelo serviço, sem fixar a lista de personagens ou o total de páginas no código. Na consulta inicial realizada durante o desenvolvimento, a API informou 45 personagens, em 9 páginas de 5 itens; esses valores podem mudar.

## Como os códigos funcionam

### Inicialização e HttpClient

`Program.cs` inicializa o Blazor WebAssembly e registra o `HttpClient` com a URL base `https://www.demonslayer-api.com/api/v1/` e tempo limite de 20 segundos.

O C# roda no navegador via WebAssembly. O servidor iniciado por `dotnet run` entrega os arquivos do aplicativo; as consultas à API são feitas pelo navegador.

### Modelos da resposta

`Models/RespostaPersonagens.cs` reúne as classes:

- `RespostaPersonagens`: contém `Personagens` (JSON `content`) e `Paginacao` (JSON `pagination`).
- `Paginacao`: representa `currentPage`, `totalPages` e `totalElements`.
- `Personagem`: representa os campos usados pelos cartões.

| JSON | Propriedade C# | Exibição |
| --- | --- | --- |
| `id` | `Id` | Validação do personagem |
| `name` | `Nome` | Nome |
| `age` | `Idade` | Idade |
| `race` | `Raca` | Raça |
| `quote` | `Citacao` | Citação |
| `img` | `Imagem` | URL da imagem |

Os atributos `JsonPropertyName` associam os nomes JSON às propriedades C#. A idade é um inteiro anulável e aceita também números enviados como texto. Os demais campos da API são ignorados porque não são necessários ao exercício.

### Listagem, busca e paginação

`Pages/Home.razor` controla o fluxo:

1. `OnInitializedAsync` chama `Carregar(1)` ao abrir a página.
2. O método `Carregar` monta a consulta de página ou nome e chama `Http.GetAsync`.
3. O status HTTP é verificado antes da desserialização com `ReadFromJsonAsync<RespostaPersonagens>`.
4. A resposta é validada, incluindo a presença de `content`, IDs e nomes válidos e os metadados da listagem.
5. Cada personagem é apresentado pelo componente `CartaoPersonagem`.
6. Os botões de navegação usam os números retornados em `pagination`; a página anterior é bloqueada na primeira página e a próxima é bloqueada na última.

As URLs de navegação são montadas a partir do endpoint conhecido e do número da página. O código não precisa seguir links externos recebidos no JSON.

Durante uma consulta, os controles do formulário ficam desabilitados e a tela indica o carregamento. Os cartões antigos são removidos para não misturar resultados de consultas diferentes. O método impede consultas simultâneas e cancela a requisição pendente quando o componente é encerrado.

### Cartões e imagens

`Components/CartaoPersonagem.razor` exibe nome, idade, raça, citação e imagem. A imagem vem exclusivamente da URL HTTPS retornada em `img`; não há imagens locais fixas nem URLs de personagens construídas manualmente.

O evento `@onerror` trata falhas ao carregar imagens, preservando os demais dados. Quando a API não informa um campo, a tela apresenta uma mensagem em vez de falhar. Uma nova consulta permite tentar carregar as imagens novamente.

Os textos de raça e citação são apresentados como recebidos da API, normalmente em inglês. O código não inventa dados nem traduz as falas.

`Pages/Home.razor.css` organiza os cartões em uma grade que se adapta à largura da tela. Os estilos do componente ajustam a imagem sem recortá-la e permitem a quebra de textos longos.

### Tratamento de falhas

- Busca sem correspondências: a API retorna HTTP 404; a aplicação mostra **Nenhum personagem encontrado**.
- Lista vazia: a mesma mensagem de ausência de resultados.
- Outros erros HTTP: mensagem de falha e botão de nova tentativa.
- Falha de conexão e tempo limite: mensagens específicas.
- JSON inválido, dados incompletos ou paginação inconsistente: resposta rejeitada com aviso.
- Idade, raça, citação ou imagem ausentes: indicação no cartão.
- Falha de carregamento da imagem: aviso mantendo nome e demais campos.
- **Tentar novamente** repete a última página ou busca solicitada, sem perder o contexto da tentativa.

## Organização

```text
SearchDemonSlayerAPI.csproj       Configura .NET 10 e dependências
Program.cs                      Inicialização e cliente HTTP
Models/RespostaPersonagens.cs    Modelos de resposta, paginação e personagem
Pages/Home.razor                 Listagem, busca, navegação e tratamento de erros
Pages/Home.razor.css             Grade de resultados
Components/CartaoPersonagem.razor Apresentação de cada personagem
Components/CartaoPersonagem.razor.css  Estilos dos cartões
Layout/                         Menu e estrutura visual
App.razor                       Roteamento
_Imports.razor                  Importações comuns
Properties/launchSettings.json   Endereço local de execução
wwwroot/                        HTML, Bootstrap e arquivos estáticos
.vscode/                        Tarefas e recomendações de extensões
tests/SearchDemonSlayerAPI.Tests/ Testes automatizados
.gitignore                      Exclusão de arquivos gerados
README.md                       Documentação
```

As páginas demonstrativas Counter e Weather foram removidas para manter o foco no exercício.

## Compilação e testes

Na raiz do projeto:

```powershell
dotnet build
dotnet test "tests\SearchDemonSlayerAPI.Tests\SearchDemonSlayerAPI.Tests.csproj"
```

Foram aprovados **16 testes com bUnit e xUnit**, que verificam a listagem inicial, navegação, limites da paginação, busca no servidor, codificação do nome, retorno à lista, ausência de resultados, respostas inválidas, falhas HTTP/rede, tempo limite, nova tentativa, bloqueio de consultas duplicadas, campos opcionais e carregamento da imagem.

As respostas HTTP dos testes são simuladas para permitir verificação sem depender da disponibilidade da API. O projeto principal exclui a pasta `tests` da compilação e dos arquivos do aplicativo.

## Publicação manual

O `.gitignore` usa o modelo Visual Studio/.NET e ignora `bin/`, `obj/` e outros arquivos gerados. Essas pastas são recriadas automaticamente durante a compilação e não precisam ser enviadas ao GitHub.

Publique o código-fonte e os arquivos de configuração do projeto. Depois, entregue o link no Google Classroom, conforme a atividade.

## Referências

- Material fornecido: `lista demon slayer.pdf`.
- [Documentação da API](https://www.demonslayer-api.com/documentation).
- [Primeira página de personagens](https://www.demonslayer-api.com/api/v1/characters?page=1).
