# Verificação — 15/10/2026

- C#: compilação `net9.0` concluída com **zero erros e zero avisos**; aplicação executada no runtime **9.0.21**.
- PHP: verificação de sintaxe aprovada em todos os arquivos; execução com PHP **8.2.12** e extensão MongoDB **1.19.0**.
- Banco: **MongoDB local real**, com bancos separados `zlora_csharp` e `zlora_php`.
- **40 verificações de integração aprovadas**, 20 em cada versão. Resultado registrado em `ultimo-resultado.json`.
- Conferência visual: login C#, formulário de cadastro PHP e página inicial reaproveitada. Nenhum erro de JavaScript registrado no navegador da página inicial.
- Conferência no banco: senhas com hash, vínculos de empresa válidos e um perfil por conta C#.
- Todos os usuários e empresas criados pelos testes foram removidos por identificadores específicos. Nenhuma conta real foi removida.

## Casos cobertos em cada versão

1. Página inicial acessível.
2. Visitante sem acesso à gestão de usuários.
3. Cadastro sem token CSRF rejeitado.
4. Dados inválidos rejeitados.
5. Dono cadastra empresa e entra como Administrador.
6. E-mail duplicado rejeitado.
7. Duplicidade também rejeitada com letras maiúsculas.
8. Administrador cadastra Secretaria e Profissional.
9. Formulário adulterado não cria outro Administrador como colaborador.
10. Login dos dois perfis de colaboradores.
11. Ambos impedidos de gerenciar usuários.
12. Listagem de usuários isolada por empresa.
13. Administrador de outra empresa não altera colaborador.
14. Promoção adulterada para Administrador rejeitada.
15. Alteração de perfil sem CSRF rejeitada.
16. Alteração válida de perfil invalida sessão anterior e aparece no novo login.
17. Logout por GET rejeitado.
18. Logout por POST encerra acesso.
19. Cinco senhas incorretas bloqueiam temporariamente o login.
20. CSS e imagem do login disponíveis.

## Limites

Esta validação cobre autenticação e gestão inicial de usuários. Não é uma validação de agenda ou cobrança: esses módulos não fazem parte desta entrega. Não foi realizado teste de carga, publicação externa ou auditoria completa de segurança.
