# Zlora - sistema MVC para apresentação

Projeto em C# com ASP.NET Core MVC, Razor, Identity e MongoDB. A pasta foi copiada da versão mais recente do projeto e mantém os arquivos visuais da apresentação: paleta Zlora, tela inicial, login e imagem de marca.

## Executar

1. Instale o SDK .NET 9 e deixe o MongoDB Server funcionando em `mongodb://127.0.0.1:27017`.
2. Abra `iniciar.bat` ou execute `dotnet run` nesta pasta.
3. Acesse `http://localhost:5080`.
4. Escolha um plano na apresentação (o pagamento é pulado nesta etapa) e cadastre o estabelecimento.

O banco usado é o MongoDB `zlora_csharp`, seguindo as coleções planejadas em `Banco.txt`. A aplicação cria os índices necessários e, na primeira execução, prepara os planos, 50 empresas, 30 clientes, 4 profissionais, serviços e 28 agendamentos de demonstração. A massa fica concentrada na primeira empresa para facilitar a apresentação.

## Fluxo entregue

- Cadastro público do estabelecimento e login do administrador.
- Administração de usuários com os perfis Administrador, Secretaria e Profissional.
- Administrador com acesso completo à própria empresa.
- Secretaria visualiza clientes e a agenda de todos os profissionais.
- Profissional vê, cria, altera e cancela somente os próprios horários.
- Cadastro de clientes, serviços e profissional responsável.
- Link público de cada serviço para o cliente se cadastrar e agendar.
- Cancelamento público com validação de 24 horas de antecedência.
- Bloqueio de conflito no mesmo horário e profissional.
- Histórico filtrado por semana ou mês e botão para baixar PDF.
- Financeiro, estoque, pagamento online, notificações e aplicativo móvel aparecem como funcionalidades futuras, conforme o escopo da apresentação.

## Hospedagem

Para hospedar, publique o projeto ASP.NET Core e configure as variáveis `MongoConnection__ConnectionString`, `MongoConnection__Database` e, se desejar recuperação de senha por e-mail, as chaves de `EmailSettings`. Não coloque senha real de e-mail no repositório. A hospedagem também precisa de um MongoDB acessível pela internet, como um cluster gratuito compatível com MongoDB.

As contas de demonstração são criadas somente quando o banco ainda não possui empresas. O cadastro real de um estabelecimento cria sua própria empresa e seu primeiro profissional, permitindo que a administradora atenda sozinha ou cadastre mais colaboradores.
