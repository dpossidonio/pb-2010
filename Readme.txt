Implementado:
- Set/update the user profile
- Post a message (envia para todos os seus amigos, caso n�o estejam ligados n�o � feita a repeti��o do envio).
- Send a friend request (a mesma politica que o Post Message)
- Accept/reject a friend request 
- Refresh view (pergunta a todos os amigos se tem mensagens novas)
- Persistencia de todas altera��es de estado do servidor prim�rio para as r�plicas



Como executar o projecto:
O projecto pode ser testado correndo cada aplica��o em separado( Client e Server)
ou entao pode se optar por utilizar o projecto de teste.
 

1. Executar Teste.exe

2. Indicar o n� de aplica��es cliente que desejamos executar
(Para cada aplica��o aberta tamb�m abrir� automaticamente os seus 3 servidores)

3. Indicar em cada 3 servidores os seus portos para cada cliente
Exemplo:
Para o cliente com o porto 8000:
8001 num dos servidores abertos
8002 "		"	"
8003 "		"	"

4. Substituir na aplica��o cliente os portos escolhidos em cada um dos seus servidores nos campos definidos para esse efeito.

5. Clicar "Connect"

Nota: Como ainda n�o foi criado o anel que liga todos os servidores prim�rios de cada cliente, algumas opera��es requerem que 
seja preenchido na tab Profile pelo menos o username.