// Executar na raiz: node Zlora_CSharp/tests/testar-recuperacao.cjs
// SMTP capturado somente em memória, bancos descartáveis e nenhum e-mail externo.
const net = require('node:net');
const fs = require('node:fs');
const path = require('node:path');
const {spawn, spawnSync} = require('node:child_process');
const assert = require('node:assert/strict');
const stamp = Date.now().toString();
const php = 'C:/xampp/php/php.exe';
const root = process.cwd();
const mails = [];
const processes = [];
const databases = [];
let failureMode = false;
const smtp = net.createServer(socket => {
 let pending='', data=false, body=[], auth=0;
 socket.write('220 localhost Zlora teste\r\n');
 socket.on('data', chunk => {
  pending += chunk.toString();
  while(pending.includes('\r\n')) {
   const end=pending.indexOf('\r\n'),line=pending.slice(0,end);pending=pending.slice(end+2);
   if(data){if(line!=='.'){body.push(line);continue;}data=false;mails.push(body.join('\r\n'));body=[];socket.write('250 accepted\r\n');continue;}
   if(auth){if(auth===1){auth=2;socket.write('334 UGFzc3dvcmQ6\r\n');}else{auth=0;socket.write('235 authenticated\r\n');}continue;}
   if(/^EHLO|^HELO/.test(line)) socket.write('250-localhost\r\n250-AUTH PLAIN LOGIN\r\n250 OK\r\n');
   else if(/^AUTH PLAIN/.test(line)) socket.write('235 authenticated\r\n');
   else if(/^AUTH LOGIN/.test(line)){auth=1;socket.write('334 VXNlcm5hbWU6\r\n');}
   else if(/^DATA/.test(line)){if(failureMode)socket.write('451 temporary failure\r\n');else{data=true;socket.write('354 send data\r\n');}}
   else if(/^QUIT/.test(line)) socket.end('221 bye\r\n');
   else socket.write('250 OK\r\n');
  }
 });
 socket.on('error',()=>{});
});
class Browser {
 constructor(base){this.base=base;this.cookies=new Map();}
 async request(route,fields){
  const headers={Cookie:[...this.cookies].map(([k,v])=>`${k}=${v}`).join('; ')};
  if(fields)headers['Content-Type']='application/x-www-form-urlencoded';
  const r=await fetch(this.base+route,{method:fields?'POST':'GET',body:fields?new URLSearchParams(fields):undefined,headers,redirect:'manual'});
  for(const cookie of r.headers.getSetCookie()){const pair=cookie.split(';')[0],i=pair.indexOf('=');this.cookies.set(pair.slice(0,i),pair.slice(i+1));}
  return {status:r.status,text:await r.text(),location:r.headers.get('location')};
 }
 async form(page,fields,action=page){
  const r=await this.request(page);assert.equal(r.status,200);
  const m=r.text.match(/name="(csrf|__RequestVerificationToken)"[^>]*value="([^"]+)"/);assert.ok(m,'Token CSRF no formulário');
  return this.request(action,{...fields,[m[1]]:m[2]});
 }
}
const redirect=r=>assert.ok([302,303].includes(r.status),`Esperado redirect; recebido ${r.status}: ${r.text.slice(0,200)}`);
const fixture=(db,action,email='')=>{
 const r=spawnSync(php,['Zlora_PHP/tests/recuperacao-fixture.php',db,action,email],{cwd:root,encoding:'utf8'});
 assert.equal(r.status,0,r.stderr);return r.stdout;
};
function start(command,args,cwd,env){
 const p=spawn(command,args,{cwd,env:{...process.env,...env},windowsHide:true,stdio:['ignore','pipe','pipe']});
 p.logs='';p.stdout.on('data',d=>p.logs+=d);p.stderr.on('data',d=>p.logs+=d);processes.push(p);return p;
}
async function ready(base,p){for(let i=0;i<80;i++){try{const r=await fetch(base);if(r.ok)return;}catch{}if(p.exitCode!==null)throw Error(p.logs);await new Promise(r=>setTimeout(r,250));}throw Error('Servidor não iniciou: '+p.logs);}
function lastLink(){
 let raw=mails.at(-1);assert.ok(raw,'Mensagem recebida no SMTP');
 const body=raw.slice(raw.indexOf('\r\n\r\n')+4);
 if(/Content-Transfer-Encoding: base64/i.test(raw))raw=Buffer.from(body.replace(/\s/g,''),'base64').toString();
 else raw=body.replace(/=\r\n/g,'').replace(/=([0-9a-f]{2})/gi,(_,h)=>String.fromCharCode(parseInt(h,16)));
 const match=raw.match(/http:\/\/127\.0\.0\.1:\d+\/[^\s<>]+/);assert.ok(match,'Link no e-mail');return new URL(match[0]);
}
async function test(cs){
 const name=cs?'csharp':'php',db=`zlora_recuperacao_teste_${name}_${stamp}`;databases.push(db);
 const base=cs?'http://127.0.0.1:5081':'http://127.0.0.1:8001';
 let p;
 if(cs)p=start(path.join(root,'Zlora_CSharp/.runtime/dotnet.exe'),['bin/Debug/net9.0/Zlora.dll','--urls',base],path.join(root,'Zlora_CSharp'),{
  MongoConnection__Database:db,EmailSettings__SmtpServer:'127.0.0.1',EmailSettings__SmtpPort:'2525',EmailSettings__Security:'None',
  EmailSettings__SenderEmail:'remetente@teste.invalid',EmailSettings__Username:'teste',EmailSettings__Password:'somente-teste',EmailSettings__BaseUrl:base});
 else{
  fixture(db,'preparar');const sessions=path.join(root,'Zlora_PHP/tests/.sessions');fs.mkdirSync(sessions,{recursive:true});
  p=start(php,['-d',`session.save_path=${sessions}`,'-S','127.0.0.1:8001','-t','public'],path.join(root,'Zlora_PHP'),{
   ZLORA_MONGO_DATABASE:db,ZLORA_SMTP_HOST:'127.0.0.1',ZLORA_SMTP_PORT:'2525',ZLORA_SMTP_SECURITY:'none',ZLORA_SMTP_EMAIL:'remetente@teste.invalid',ZLORA_SMTP_USER:'teste',ZLORA_SMTP_PASSWORD:'somente-teste',ZLORA_BASE_URL:base});
 }
 await ready(base,p);
 const route=(a,b)=>cs?a:'/index.php?pagina='+b;
 const login=route('/Contas/Login','login'),register=route('/Contas/Registro','registro'),forgot=route('/Contas/EsqueciSenha','esqueci-senha'),reset=route('/Contas/RedefinirSenha','redefinir-senha'),painel=route('/Home/Painel','painel');
 const fields=o=>Object.fromEntries(Object.entries(o).map(([k,v])=>[cs?({email:'Email',senha:'Senha',nome:'Nome',nomecontato:'NomeContato',telefone:'Telefone'}[k]||k):k,v]));
 const email=`recuperacao.${stamp}@teste.invalid`,old='Teste@123',newPass='Nova@123';
 const owner=new Browser(base),guest=new Browser(base);
 redirect(await owner.form(register,fields({nome:'Salão teste',nomecontato:'Dono teste',telefone:'14999999999',email,senha:old})));
 redirect(await owner.form(login,fields({email,senha:old})));
 assert.equal((await guest.request(forgot,fields({email}))).status,400);
 assert.match((await guest.request(login)).text,/Esqueceu a senha/);
 let sent=mails.length;redirect(await guest.form(forgot,fields({email:'ausente@teste.invalid'})));assert.equal(mails.length,sent);
 redirect(await guest.form(forgot,fields({email})));assert.equal(mails.length,sent+1);
 let link=lastLink();const token=link.searchParams.get(cs?'token':'token'),id=link.searchParams.get(cs?'userId':'id');
 const stored=JSON.parse(fixture(db,'hash',email));assert.ok(stored.hash && stored.hash!==token);
 redirect(await guest.form(forgot,fields({email})));assert.equal(mails.length,sent+1,'Limite de um envio por minuto');
 const resetFields=(url,pass,confirm=pass)=>cs?{UserId:url.searchParams.get('userId'),Token:url.searchParams.get('token'),NovaSenha:pass,ConfirmarSenha:confirm}:{id:url.searchParams.get('id'),token:url.searchParams.get('token'),senha:pass,confirmar:confirm};
 const page=url=>url.pathname+url.search;
 assert.equal((await guest.request(reset,resetFields(link,newPass))).status,400);
 assert.match((await guest.form(page(link),resetFields(link,newPass,'Outra@123'),reset)).text,/coincidem/);
 const forged=new URL(link);forged.searchParams.set('token',token+'x');assert.match((await guest.form(page(forged),resetFields(forged,newPass),reset)).text,/Solicite outro link/);
 fixture(db,'expirar',email);assert.match((await guest.form(page(link),resetFields(link,newPass),reset)).text,/expirou/);
 fixture(db,'liberar-envio',email);redirect(await guest.form(forgot,fields({email})));link=lastLink();
 const superseded=new URL(link);fixture(db,'liberar-envio',email);redirect(await guest.form(forgot,fields({email})));link=lastLink();
 assert.match((await guest.form(page(superseded),resetFields(superseded,newPass),reset)).text,/Solicite outro link/);
 redirect(await guest.form(page(link),resetFields(link,newPass),reset));
 redirect(await owner.request(painel));
 assert.equal((await guest.form(login,fields({email,senha:old}))).status,200);
 redirect(await guest.form(login,fields({email,senha:newPass})));
 const second=new Browser(base);assert.match((await second.form(page(link),resetFields(link,'Outra@456'),reset)).text,/utilizado/);
 fixture(db,'liberar-envio',email);failureMode=true;redirect(await second.form(forgot,fields({email})));failureMode=false;
 assert.equal(JSON.parse(fixture(db,'hash',email)).hash,null,'Falha SMTP invalida o token');
 console.log(`${name}: recuperação SMTP, hash, limite, CSRF, confirmação, token adulterado, expirado, substituído, uso único, nova senha e invalidação de sessão OK.`);
 return {versao:name,resultado:'aprovado'};
}
(async()=>{
 await new Promise((resolve,reject)=>smtp.once('error',reject).listen(2525,'127.0.0.1',resolve));
 const results=[];for(const cs of [true,false])results.push(await test(cs));
 fs.writeFileSync('Zlora_CSharp/tests/resultado-recuperacao.json',JSON.stringify({data:new Date().toISOString(),smtp:'captura local; sem envio externo',resultados:results},null,2));
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(async()=>{
 for(const p of processes){p.kill();await new Promise(resolve=>{if(p.exitCode!==null)resolve();else p.once('exit',resolve);});}
 smtp.close();for(const db of databases)fixture(db,'limpar');
});

