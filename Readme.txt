Implementado:
- Set/update the user profile
- Post a message (envia para todos os seus amigos, caso não estejam ligados não é feita a repetição do envio).
- Send a friend request (a mesma politica que o Post Message)
- Accept/reject a friend request 
- Refresh view (pergunta a todos os amigos se tem mensagens novas)
- Persistencia de todas alterações de estado do servidor primário para as réplicas



Como executar o projecto:
O projecto pode ser testado correndo cada aplicação em separado( Client e Server)
ou entao pode se optar por utilizar o projecto de teste.
 

1. Executar Teste.exe

2. Indicar o nº de aplicações cliente que desejamos executar
(Para cada aplicação aberta também abrirá automaticamente os seus 3 servidores)

3. Indicar em cada 3 servidores os seus portos para cada cliente
Exemplo:
Para o cliente com o porto 8000:
8001 num dos servidores abertos
8002 "		"	"
8003 "		"	"

4. Substituir na aplicação cliente os portos escolhidos em cada um dos seus servidores nos campos definidos para esse efeito.

5. Clicar "Connect"

Nota: Como ainda não foi criado o anel que liga todos os servidores primários de cada cliente, algumas operações requerem que 
seja preenchido na tab Profile pelo menos o username.