// Reaproveita os arquivos originais sem modificá-los. Executar a partir da raiz do repositório.
const fs = require('fs');
const path = require('path');
const css = `
.contas-body{min-height:100vh;background:linear-gradient(120deg,#e9eaff,#c7caf3);color:#191b59;font-family:Arial,sans-serif}
.contas-nav{max-width:1380px;margin:auto;padding:20px 5%;display:flex;align-items:center;justify-content:space-between;gap:20px}
.contas-nav>a{font-size:28px;font-weight:bold;color:#4c469b}.contas-nav>div{display:flex;align-items:center;gap:22px}.contas-nav form{margin:0}.contas-nav a,.link-button{color:#413b91}
.link-button{background:none;border:0;padding:0;cursor:pointer;font:inherit}
.login-card{display:grid;grid-template-columns:1fr 1.09fr;max-width:1385px;min-height:680px;width:90%;margin:12px auto 40px;background:#f7f7ff;border:1px solid #eee;border-radius:24px;overflow:hidden;box-shadow:0 16px 45px #6267a733}
.login-form{padding:90px 15%;display:flex;flex-direction:column;justify-content:center}.login-form h1{font-size:42px;font-weight:700;letter-spacing:-1.5px;color:#161953;margin:0 0 8px}.login-subtitle{font-size:21px;color:#74759c;margin-bottom:34px}
.login-form label{display:block;font-size:16px;font-weight:bold;margin:22px 0 8px}.login-form input{width:100%;border:1px solid #d7d9ee;border-radius:12px;background:#ffffffc9;padding:15px 18px;color:#20215d;min-width:0}.login-form input:focus{outline:2px solid #8a84cf;outline-offset:2px}
.password-field{position:relative}.password-field input{padding-right:52px}.password-field button{position:absolute;right:10px;top:8px;height:38px;width:35px;border:0;background:none;color:#706fa8;cursor:pointer}
.login-submit{display:flex;justify-content:center;gap:20px;width:100%;padding:15px;background:linear-gradient(120deg,#a29cde,#7068c6);border:0;border-radius:12px;color:white;font-size:20px;font-weight:bold;cursor:pointer;margin-top:30px}.login-submit:hover{filter:brightness(.95)}
.login-brand{background-image:url('/images/login.jpeg');background-size:212% 118.3%;background-position:90.8% 50%;background-repeat:no-repeat}
.account-link{font-size:14px;margin:22px 0 0;text-align:center}.account-link a{color:#6655bf}.content-card{max-width:900px;width:92%;margin:30px auto;padding:40px;border-radius:18px;background:#fff;box-shadow:0 8px 35px #6267a722}.content-card h1{font-size:30px;color:#24205f}.content-card.wide{max-width:1250px}.registro .section-title{margin-bottom:28px}.registro .section-title h1{font-size:32px}.registro input,.registro select{height:auto;min-height:48px}.registro small{display:block;margin-top:6px;color:#6e6783}
.notice{max-width:900px;width:92%;margin:15px auto;padding:15px;background:#eef8f1;border:1px solid #b9d7c4;border-radius:10px;color:#235136}.validation{color:#9b243d;font-size:14px}.validation:empty,.validation-summary-valid{display:none}.validation ul{padding-left:18px}.role-form{display:flex;gap:8px;align-items:center}.role-form select{padding:9px;border:1px solid #c9c3df;border-radius:8px;background:white}.table td{vertical-align:middle}.table th{white-space:nowrap}
@media(max-width:900px){.login-form{padding:65px 10%}.login-form h1{font-size:32px}.login-subtitle{font-size:18px}.login-card{min-height:580px}}
@media(max-width:640px){.login-card{display:block;min-height:0}.login-brand{display:none}.login-form{padding:45px 8%}.contas-nav{padding:18px 5%}.contas-nav>div{gap:12px;font-size:14px}.content-card{padding:25px 20px}.registro .btn-lg{font-size:16px}.login-form h1{font-size:32px}}
`;
const js = `document.querySelectorAll('[data-toggle-password]').forEach(button => {
button.addEventListener('click', () => {const input = document.getElementById(button.dataset.togglePassword); const show = input.type === 'password'; input.type = show ? 'text' : 'password'; button.setAttribute('aria-label', show ? 'Ocultar senha' : 'Mostrar senha');});});`;

let home = fs.readFileSync('zloraF.html','utf8');
// O formulário real mantém os campos e o estilo do formulário original.
home = home.replace(/<form id="form-cadastro"[\s\S]*?<\/form>/, '<div class="text-center"><p>Cadastre seu estabelecimento e gerencie o acesso da sua equipe.</p><a class="btn btn-primary btn-lg" href="__REGISTRO__">Cadastrar estabelecimento</a></div>');
home = home.replace(/<div class="d-flex justify-content-center mb-5">[\s\S]*?<!-- TODO: VALIDAR — integrar o formulário ao endpoint real quando existir -->/, '');
home = home.replace(/href="#acessar" data-todo="login"/g,'href="__LOGIN__"');
home = home.replace(/(?:\.\/)?DS0HomePageDeslogada\//g,'/DS0HomePageDeslogada/');
// O bundle original depende de jQuery; carregá-lo antes evita erro na navbar.
home = home.replace(/    <script src="\/DS0HomePageDeslogada\/js\/bootstrap[^\n]+\n/, '');
home = home.replace(/(<script src="\/DS0HomePageDeslogada\/js\/jquery-3.2.1.min.js"><\/script>)/, '$1\n    <script src="/DS0HomePageDeslogada/js/bootstrap.bundle.min.js+jquery.mask.min.js+script.js,qv18.pagespeed.jc.DbS1S4ohOB.js"></script>');

for (const [root, view, login, register] of [
 ['Zlora_CSharp/wwwroot','Zlora_CSharp/Views/Home/Index.cshtml','/Contas/Login','/Contas/Registro'],
 ['Zlora_PHP/public','Zlora_PHP/app/Views/inicio.php','/index.php?pagina=login','/index.php?pagina=registro']
]) {
 fs.mkdirSync(root,{recursive:true});
 fs.cpSync('DS0HomePageDeslogada',path.join(root,'DS0HomePageDeslogada'),{recursive:true});
 for(const folder of ['css','js','images']) fs.mkdirSync(path.join(root,folder),{recursive:true});
 fs.copyFileSync('login.jpeg',path.join(root,'images/login.jpeg'));
 fs.writeFileSync(path.join(root,'css/contas.css'),css);
 fs.writeFileSync(path.join(root,'js/contas.js'),js);
 const output=home.replaceAll('__LOGIN__',login).replaceAll('__REGISTRO__',register);
 fs.writeFileSync(view,view.endsWith('cshtml')?'@{ Layout = null; }\n'+output.replaceAll('@','@@'):output);
 // Remove somente o código simulado de envio e a troca de tipo, que não têm formulário nesta etapa.
 let publicJs=fs.readFileSync('DS0HomePageDeslogada/js/zloara.js','utf8');
 publicJs=publicJs.replace(/    \/\* ========== ALTERNÂNCIA DE TIPO DE CADASTRO[\s\S]*?    \/\* ========== INICIALIZAÇÃO ========== \*\//,'    /* Inicialização dos comportamentos originais da página pública. */');
 publicJs=publicJs.replace(/        inicializarTipoCadastro\(\);|        inicializarMascara\(\);|        inicializarValidacaoFormulario\(\);/g,'');
 fs.writeFileSync(path.join(root,'DS0HomePageDeslogada/js/zloara.js'),publicJs);
}
console.log('Telas originais reaproveitadas nas duas versões.');
