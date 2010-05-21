Implementado:
- Set/update the user profile
- Post a message (envia para todos os seus amigos, caso n�o estejam ligados n�o � feita a repeti��o do envio).
- Send a friend request (a mesma politica que o Post Message)
- Accept/reject a friend request 
- Refresh view (pergunta a todos os amigos se tem mensagens novas)
- Persistencia de todas altera��es de estado do servidor prim�rio para as r�plicas
- Comando info no Servidor

Como executar o projecto:
O projecto pode ser testado correndo cada aplica��o em separado( Client.exe e Server.exe) directamente apartir da pasta dos execut�veis.
Neste Caso seguir os seguintes passos:

1- Iniciar Server.exe
2- Indicar qual o endere�o na qual este se vai executar (e.g. 127.0.0.1:8001)
3- Indicar o numero de servidores do cliente

4- Iniciar Client.exe
5- Indicar o seu endere�o na caixa de texto repectiva
6- Indicar o endere�o de pelo menos um servidor
7- usar a interface grafica do cliente para interagir com o sistema(e.g. dar um username)

Ou entao pode se optar por utilizar o execut�vel de teste (Teste.exe), neste caso seguir os seguintes passos:

1- Iniciar Teste.exe
2- Introduzir o endere�o do cliente (e.g. 127.0.0.1:8000)
3- Introduzir o numero de servidores do cliente
4- Na aplica��o cliente Clicar em "Connect"
Repetir os passos (1 a 5) para iniciar mais aplica��es do tipo Client e Server.

Nota: Como ainda n�o foi criado o anel que liga todos os servidores prim�rios de cada cliente, algumas opera��es requerem que 
seja preenchido na tab Profile pelo menos o username.

PROCURAS:

1- Definir nome do utilizador, restantes campos s�o opcionais;
2- Na consola do servidor prim�rio inserir join;
3- Inserir o endere�o de um servidor que j� esteja no anel, no caso inicial � indiferente;
4- comando "ring" apresenta informa��o do anel sucessores e predecessor
5- comando "searchinfo" apresenta a informa��o que o n� det�m no anel