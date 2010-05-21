Implementado:
- Set/update the user profile
- Post a message (envia para todos os seus amigos, caso não estejam ligados não é feita a repetição do envio).
- Send a friend request (a mesma politica que o Post Message)
- Accept/reject a friend request 
- Refresh view (pergunta a todos os amigos se tem mensagens novas)
- Persistencia de todas alterações de estado do servidor primário para as réplicas
- Comando info no Servidor

Como executar o projecto:
O projecto pode ser testado correndo cada aplicação em separado( Client.exe e Server.exe) directamente apartir da pasta dos executáveis.
Neste Caso seguir os seguintes passos:

1- Iniciar Server.exe
2- Indicar qual o endereço na qual este se vai executar (e.g. 127.0.0.1:8001)
3- Indicar o numero de servidores do cliente

4- Iniciar Client.exe
5- Indicar o seu endereço na caixa de texto repectiva
6- Indicar o endereço de pelo menos um servidor
7- usar a interface grafica do cliente para interagir com o sistema(e.g. dar um username)

Ou entao pode se optar por utilizar o executável de teste (Teste.exe), neste caso seguir os seguintes passos:

1- Iniciar Teste.exe
2- Introduzir o endereço do cliente (e.g. 127.0.0.1:8000)
3- Introduzir o numero de servidores do cliente
4- Na aplicação cliente Clicar em "Connect"
Repetir os passos (1 a 5) para iniciar mais aplicações do tipo Client e Server.

Nota: Como ainda não foi criado o anel que liga todos os servidores primários de cada cliente, algumas operações requerem que 
seja preenchido na tab Profile pelo menos o username.