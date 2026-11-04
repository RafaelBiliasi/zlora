// Teste de integração das duas aplicações em execução. Node.js 20+.
// Cria somente contas identificadas por @teste.zlora.invalid. Veja limpar-testes.php.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const stamp = Date.now().toString();
console.log('Identificador dos dados de teste: '+stamp);
const senha = 'Teste@123';
class Browser {
 constructor(base){ this.base=base; this.cookies=new Map(); }
 async request(route, data){
  const headers = {Cookie:[...this.cookies].map(([k,v])=>`${k}=${v}`).join('; ')};
  if(data) headers['Content-Type']='application/x-www-form-urlencoded';
  const r=await fetch(this.base+route,{method:data?'POST':'GET',body:data?new URLSearchParams(data):undefined,headers,redirect:'manual'});
  for(const c of r.headers.getSetCookie()){const [pair]=c.split(';');const i=pair.indexOf('=');this.cookies.set(pair.slice(0,i),pair.slice(i+1));}
  return {status:r.status,location:r.headers.get('location'),text:await r.text()};
 }
 async form(route,data,action=route){
  const page=await this.request(route); assert.equal(page.status,200,`GET ${route}: ${page.text.slice(0,180)}`);
  const token=page.text.match(/name="(__RequestVerificationToken|csrf)"[^>]*value="([^"]+)"/);
  assert.ok(token,`CSRF ${route}`);return this.request(action,{...data,[token[1]]:token[2]});
 }
}
const configs=[
 {name:'CSharp',base:'http://localhost:5080',home:'/',login:'/Contas/Login',registro:'/Contas/Registro',painel:'/Home/Painel',usuarios:'/Usuarios',criar:'/Usuarios/Criar',perfil:'/Usuarios/Perfil',logout:'/Contas/Logout',map:{nome:'Nome',nomecontato:'NomeContato',telefone:'Telefone',email:'Email',senha:'Senha',perfil:'Perfil'}},
 {name:'PHP',base:'http://localhost:8000',home:'/index.php',login:'/index.php?pagina=login',registro:'/index.php?pagina=registro',painel:'/index.php?pagina=painel',usuarios:'/index.php?pagina=usuarios',criar:'/index.php?pagina=criar-usuario',perfil:'/index.php?pagina=perfil',logout:'/index.php?pagina=logout',map:{}},
];
const redirects=r=>assert.ok([302,303].includes(r.status),`Esperado redirecionamento, recebido ${r.status}: ${r.text.slice(0,300)}`);
const results=[];
async function test(c){
 let count=0;const ok=label=>{count++;console.log(`${c.name}: OK ${label}`);};
 const fields=d=>Object.fromEntries(Object.entries(d).map(([k,v])=>[c.map[k]||k,v]));
 const owner=new Browser(c.base),other=new Browser(c.base),secretaria=new Browser(c.base),professional=new Browser(c.base),guest=new Browser(c.base);
 const email=type=>`${c.name.toLowerCase()}.${stamp}.${type}@teste.zlora.invalid`;
 const register=async(b,type)=>b.form(c.registro,fields({nome:'Salão teste '+type,nomecontato:'Dono '+type,telefone:'14999999999',email:email(type),senha}));
 const login=async(b,type)=>b.form(c.login,fields({email:email(type),senha}));
 assert.equal((await guest.request(c.home)).status,200);ok('página inicial');
 redirects(await guest.request(c.usuarios));ok('visitante bloqueado na gestão');
 assert.equal((await guest.request(c.registro,fields({email:email('csrf'),senha}))).status,400);ok('cadastro sem CSRF bloqueado');
 const invalid=await guest.form(c.registro,fields({nome:' ',nomecontato:' ',telefone:'1',email:'invalido',senha:'1'}));assert.equal(invalid.status,200);ok('validação de dados inválidos');
 redirects(await register(owner,'dono'));redirects(await login(owner,'dono'));assert.match((await owner.request(c.painel)).text,/Administrador/);ok('dono cadastra empresa e entra como admin');
 assert.equal((await register(guest,'dono')).status,200);ok('e-mail duplicado rejeitado');
 const caps=await guest.form(c.registro,fields({nome:'Empresa repetida',nomecontato:'Dono',telefone:'14999999999',email:email('dono').toUpperCase(),senha}));assert.equal(caps.status,200);ok('e-mail duplicado ignorando maiúsculas');
 for(const [type,perfil] of [['secretaria','Secretaria'],['profissional','Profissional']]){
  const data={nome:'Equipe '+type,nomecontato:'Equipe '+type,telefone:'14999999999',email:email(type),senha,perfil,empresa_id:'empresa-forjada'};
  // C# usa Nome para o colaborador, PHP usa nomecontato.
  redirects(await owner.form(c.criar,fields(data)));
 }ok('admin cadastra secretária e profissional');
 const escalate=await owner.form(c.criar,fields({nome:'Intruso',nomecontato:'Intruso',telefone:'14999999999',email:email('elevacao'),senha,perfil:'Administrador'}));assert.equal(escalate.status,200);ok('colaborador não recebe perfil Administrador por adulteração');
 redirects(await login(secretaria,'secretaria'));redirects(await login(professional,'profissional'));
 assert.match((await secretaria.request(c.painel)).text,/Secretaria/);assert.match((await professional.request(c.painel)).text,/Profissional/);ok('login dos dois colaboradores');
 for(const b of [secretaria,professional]){
  for(const route of [c.usuarios,c.criar]){const denied=await b.request(route);assert.ok(denied.status===403 || (denied.status===302 && denied.location.includes('AcessoNegado')));}
 }ok('ambos impedidos de gerenciar usuários');
 redirects(await register(other,'outra'));redirects(await login(other,'outra'));
 const ownList=await owner.request(c.usuarios),otherList=await other.request(c.usuarios);
 assert.ok(ownList.text.includes(email('secretaria')));assert.ok(!otherList.text.includes(email('secretaria')));assert.ok(!ownList.text.includes(email('outra')));ok('listas isoladas por empresa');
 const row=ownList.text.match(/<tr>[\s\S]*?<\/tr>/g).find(row=>row.includes(email('secretaria')));
 const id=row.match(/name="id" value="([^"]+)"/)[1];
 assert.equal((await other.form(c.usuarios,{id,perfil:'Profissional'},c.perfil)).status,404);ok('admin de outra empresa não altera colaborador');
 assert.equal((await owner.form(c.usuarios,{id,perfil:'Administrador'},c.perfil)).status,400);ok('promoção forjada bloqueada');
 assert.equal((await owner.request(c.perfil,{id,perfil:'Profissional'})).status,400);ok('alteração sem CSRF bloqueada');
 redirects(await owner.form(c.usuarios,{id,perfil:'Profissional'},c.perfil));
 redirects(await secretaria.request(c.painel));redirects(await login(secretaria,'secretaria'));assert.match((await secretaria.request(c.painel)).text,/Profissional/);ok('mudança de perfil invalida a sessão antiga');
 assert.equal((await owner.request(c.logout)).status,405);ok('logout exige POST');
 redirects(await owner.form(c.painel,{},c.logout));redirects(await owner.request(c.usuarios));ok('logout encerra acesso');
 for(let i=0;i<5;i++) assert.equal((await guest.form(c.login,fields({email:email('profissional'),senha:'SenhaErrada'}))).status,200);
 assert.equal((await login(guest,'profissional')).status,200);ok('bloqueio após cinco senhas incorretas');
 assert.equal((await guest.request('/css/contas.css')).status,200);assert.equal((await guest.request('/images/login.jpeg')).status,200);ok('estilos e referência visual disponíveis');
 results.push({versao:c.name,verificacoes:count});
}
(async()=>{for(const c of configs.filter(c=>!process.argv[2] || c.name===process.argv[2]))await test(c);fs.writeFileSync('Zlora_CSharp/tests/ultimo-resultado'+(process.argv[2]?'-'+process.argv[2]:'')+'.json',JSON.stringify({data:new Date().toISOString(),identificador:stamp,resultados:results},null,2));console.log('TODOS OS TESTES PASSARAM. Identificador: '+stamp);})().catch(e=>{console.error(e);process.exitCode=1;});
